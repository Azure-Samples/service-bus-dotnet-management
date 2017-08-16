// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureServiceBusDotNetManagement
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.ResourceManager;
    using Microsoft.Azure.Management.ResourceManager.Models;
    using Microsoft.Azure.Management.ServiceBus;
    using Microsoft.Azure.Management.ServiceBus.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Rest;

    public static class ServiceBusManagementSample
	{
		private const string QueueName = "queue1";
		private const string TopicName = "topic1";
		private const string SubscriptionName = "topic1";
		private const string RuleName = "rule1";

		private static readonly IConfigurationRoot settingsCache;
	    private static AppOptions appOptions;
	    private static string tokenValue = string.Empty;
	    private static DateTime tokenExpiresAtUtc = DateTime.MinValue;

	    private static string resourceGroupName = string.Empty;
	    private static string namespaceName = string.Empty;

	    static ServiceBusManagementSample()
		{
			var builder = new ConfigurationBuilder();
			builder.AddJsonFile("appsettings.json", true, true);

			settingsCache = builder.Build();
		    appOptions = new AppOptions();
            settingsCache.Bind(appOptions);
		}

		public static async Task Run()
		{
            await CreateResourceGroup().ConfigureAwait(false);
			await CreateNamespace().ConfigureAwait(false);
			await CreateQueue().ConfigureAwait(false);
			await CreateTopic().ConfigureAwait(false);
			await CreateSubscription().ConfigureAwait(false);
		    await CreateRule().ConfigureAwait(false);


            Console.WriteLine("Press a key to exit.");
			Console.ReadLine();
		}

		private static async Task CreateResourceGroup()
		{
			try
			{
				Console.WriteLine("What would you like to call your resource group?");
				resourceGroupName = Console.ReadLine();

				var token = await GetToken();

				var creds = new TokenCredentials(token);
				var rmClient = new ResourceManagementClient(creds)
				{
					SubscriptionId = appOptions.SubscriptionId
				};

				var resourceGroupParams = new ResourceGroup()
				{
					Location = appOptions.DataCenterLocation,
					Name = resourceGroupName,
				};

				Console.WriteLine("Creating resource group...");
				await rmClient.ResourceGroups.CreateOrUpdateAsync(resourceGroupName, resourceGroupParams);
				Console.WriteLine("Created resource group successfully.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not create a resource group...");
				Console.WriteLine(e.Message);
				throw e;
			}
		}

		private static async Task CreateNamespace()
		{
			try
			{
				Console.WriteLine("What would you like to call your Service Bus namespace?");
				namespaceName = Console.ReadLine();

				var token = await GetToken();

				var creds = new TokenCredentials(token);
				var sbClient = new ServiceBusManagementClient(creds)
				{
					SubscriptionId = appOptions.SubscriptionId,
				};

                var namespaceParams = new SBNamespace
				{
					Location = appOptions.DataCenterLocation,
					Sku = new SBSku()
					{
						Tier = appOptions.ServiceBusSkuTier,
						Name = appOptions.ServiceBusSkuName,
					}
				};

				Console.WriteLine("Creating namespace...");
				await sbClient.Namespaces.CreateOrUpdateAsync(resourceGroupName, namespaceName, namespaceParams);
				Console.WriteLine("Created namespace successfully.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not create a namespace...");
				Console.WriteLine(e.Message);
				throw e;
			}
		}

		private static async Task CreateQueue()
		{
			try
			{
				if (string.IsNullOrEmpty(namespaceName))
				{
					throw new Exception("Namespace name is empty!");
				}

				var token = await GetToken();

				var creds = new TokenCredentials(token);
				var sbClient = new ServiceBusManagementClient(creds)
				{
					SubscriptionId = appOptions.SubscriptionId,
				};

				var queueParams = new SBQueue
				{
					EnablePartitioning = true
				};

				Console.WriteLine("Creating queue...");
				await sbClient.Queues.CreateOrUpdateAsync(resourceGroupName, namespaceName, QueueName, queueParams);
				Console.WriteLine("Created queue successfully.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not create a queue...");
				Console.WriteLine(e.Message);
				throw e;
			}
		}

		private static async Task CreateTopic()
		{
			try
			{
				if (string.IsNullOrEmpty(namespaceName))
				{
					throw new Exception("Namespace name is empty!");
				}

				var token = await GetToken();

				var creds = new TokenCredentials(token);
				var sbClient = new ServiceBusManagementClient(creds)
				{
					SubscriptionId = appOptions.SubscriptionId,
				};

				var topicParams = new SBTopic
				{
					EnablePartitioning = true
				};

				Console.WriteLine("Creating topic...");
				await sbClient.Topics.CreateOrUpdateAsync(resourceGroupName, namespaceName, TopicName, topicParams);
				Console.WriteLine("Created topic successfully.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not create a topic...");
				Console.WriteLine(e.Message);
				throw e;
			}
		}

		private static async Task CreateSubscription()
		{
			try
			{
				if (string.IsNullOrEmpty(namespaceName))
				{
					throw new Exception("Namespace name is empty!");
				}

				var token = await GetToken();

				var creds = new TokenCredentials(token);
				var sbClient = new ServiceBusManagementClient(creds)
				{
					SubscriptionId = appOptions.SubscriptionId,
				};

				var subscriptionParams = new SBSubscription
				{
                    MaxDeliveryCount = 10
				};

				Console.WriteLine("Creating subscription...");
				await sbClient.Subscriptions.CreateOrUpdateAsync(resourceGroupName, namespaceName, TopicName, SubscriptionName, subscriptionParams);
				Console.WriteLine("Created subscription successfully.");
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not create a subscription...");
				Console.WriteLine(e.Message);
				throw e;
			}
		}

	    private static async Task CreateRule()
	    {
	        try
	        {
	            if (string.IsNullOrEmpty(namespaceName))
	            {
	                throw new Exception("Namespace name is empty!");
	            }

	            var token = await GetToken();

	            var creds = new TokenCredentials(token);
	            var sbClient = new ServiceBusManagementClient(creds)
	            {
	                SubscriptionId = appOptions.SubscriptionId,
	            };

	            var ruleParams = new Rule
	            {
	                SqlFilter = new SqlFilter("myproperty='test'")
                };

	            Console.WriteLine("Creating rule...");
	            await sbClient.Rules.CreateOrUpdateAsync(resourceGroupName, namespaceName, TopicName, SubscriptionName, RuleName, ruleParams);
	            Console.WriteLine("Created rule successfully.");
	        }
	        catch (Exception e)
	        {
	            Console.WriteLine("Could not create a rule...");
	            Console.WriteLine(e.Message);
	            throw e;
	        }
	    }

        private static async Task<string> GetToken()
		{
			try
			{
				// Check to see if the token has expired before requesting one.
				// We will go ahead and request a new one if we are within 2 minutes of the token expiring.
				if (tokenExpiresAtUtc < DateTime.UtcNow.AddMinutes(-2))
				{
					Console.WriteLine("Renewing token...");

					var tenantId = appOptions.TenantId;
					var clientId = appOptions.ClientId;
					var clientSecret = appOptions.ClientSecret;

					var context = new AuthenticationContext($"https://login.microsoftonline.com/{tenantId}");

					var result = await context.AcquireTokenAsync(
						"https://management.core.windows.net/",
						new ClientCredential(clientId, clientSecret)
					);

					// If the token isn't a valid string, throw an error.
					if (string.IsNullOrEmpty(result.AccessToken))
					{
						throw new Exception("Token result is empty!");
					}

					tokenExpiresAtUtc = result.ExpiresOn.UtcDateTime;
					tokenValue = result.AccessToken;
					Console.WriteLine("Token renewed successfully.");
				}

				return tokenValue;
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not get a new token...");
				Console.WriteLine(e.Message);
				throw e;
			}
		}
	}
}
