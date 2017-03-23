using System.Globalization;

namespace ResourceConfiguration
{
    public class RedisConfiguration
    {
        public string ConnectionString => string.Format(CultureInfo.InvariantCulture, ConnectionStringTemplate, Server, Password);
        public int Database { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Server { get; set; }
        public string ConnectionStringTemplate { get; set; }
    }
}