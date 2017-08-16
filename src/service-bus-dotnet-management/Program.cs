// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureServiceBusDotNetManagement
{
	public class Program
	{
		static void Main(string[] args)
		{
			ServiceBusManagementSample.Run().GetAwaiter().GetResult();
		}
	}
}
