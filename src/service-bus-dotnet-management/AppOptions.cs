namespace service_bus_dotnet_management
{
    public class AppOptions
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SubscriptionId { get; set; }
        public string DataCenterLocation { get; set; }
        public string ServiceBusSku { get; set; }
    }
}