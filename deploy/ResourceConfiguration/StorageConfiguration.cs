using System.Globalization;

namespace ResourceConfiguration
{
    public class StorageConfiguration
    {
        public string ConnectionString => string.Format(CultureInfo.InvariantCulture, ConnectionStringTemplate, Password);
        public string MasterDataSetContainerName { get; set; }
        public string BatchViewsContainerName { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string AccountType { get; set; }
        public string Password { get; set; }
        public string ConnectionStringTemplate { get; set; }
    }
}