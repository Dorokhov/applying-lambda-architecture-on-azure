using System.Globalization;

namespace ResourceConfiguration
{
    public class HDInsightConfiguration
    {
        public string ExistingClusterName { get; set; }

        public string ExistingClusterUri { get; set; }
        public string ExistingClusterUsername { get; set; }
        public string ExistingClusterPassword { get; set; }
        public string DefaultStorageAccountName { get; set; }
        public string DefaultStorageAccountKey { get; set; }
        public string DefaultStorageContainerName { get; set; }
        public string MasterDatasetPath { get; set; }
    }
}