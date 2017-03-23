using System.Globalization;

namespace ResourceConfiguration
{
    public class ServiceBusConfiguration
    {
        public string ConnectionString => string.Format(CultureInfo.InvariantCulture, ConnectionStringTemplate, Password);
        public string TopicName { get; set; }
        public string Password { get; set; }
        public string BatchLayerSubscription { get; set; }
        public string SpeedLayerSubscription { get; set; }
        public string ServiceBusNamespace { get; set; }
        public string ConnectionStringTemplate { get; set; }
    }
}
