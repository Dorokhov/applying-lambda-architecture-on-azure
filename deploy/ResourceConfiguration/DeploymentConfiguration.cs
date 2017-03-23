namespace ResourceConfiguration
{
    public class DeploymentConfiguration
    {
        private DeploymentConfiguration()
        {
        }

        public ServiceBusConfiguration ServiceBus { get; set; }
        public StorageConfiguration Storage { get; set; }
        public HDInsightConfiguration HDInsight { get; set; }
        public RedisConfiguration Redis { get; set; }
        public ResourceManagerSettings ResourceManagerSettings { get; set; }
        public ServiceFabrikSettings ServiceFabrikSettings { get; set; }

        public static DeploymentConfiguration Default { get; } = new DeploymentConfiguration()
        {
            ServiceBus = new ServiceBusConfiguration()
            {
                ConnectionStringTemplate = "Endpoint=sb://lambdaarch.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={0}",
                TopicName = "lambdaarchtopic",
                BatchLayerSubscription = "batchlayer",
                SpeedLayerSubscription = "speedlayer",
                ServiceBusNamespace = "lambdaarch"
            },
            Storage = new StorageConfiguration()
            {
                ConnectionStringTemplate =
                    "DefaultEndpointsProtocol=https;AccountName=storage;AccountKey={0}",
                MasterDataSetContainerName = "data",
                BatchViewsContainerName = "batchviews",
                Name = "storage",
                Location = "westeurope",
                AccountType = "Standard_RAGRS"
            },
            HDInsight = new HDInsightConfiguration()
            {
                ExistingClusterName = "lambdacluster",
                ExistingClusterUri = "lambdacluster.azurehdinsight.net",
                ExistingClusterUsername = "admin",
                ExistingClusterPassword = "",
                DefaultStorageAccountName = "storage",
                DefaultStorageAccountKey = "",
                DefaultStorageContainerName = "lambdacluster",
                MasterDatasetPath = "wasbs://data@storage.blob.core.windows.net/"
            },
            Redis = new RedisConfiguration()
            {
                ConnectionStringTemplate = "{0},allowAdmin=True,connectTimeout=60000,ssl=false,abortConnect=false,password={1}",
                Database = 1,
                Name = "lamdaredis",
                Location = "westeurope",
                Server = "lamdaredis.redis.cache.windows.net:6379"
            },
            ResourceManagerSettings = new ResourceManagerSettings()
            {
                GroupName = "Default-Networking",
                Location = "westeurope",
                SubscriptionId = "",
                DeploymentName = "mydepl",
                ClientId = "",
                ClientSecret = "",
                Authority = ""
            },
            ServiceFabrikSettings = new ServiceFabrikSettings()
            {
                ClusterName = "dorlambdacluster",
                ClusterLocation = "westeurope",
                ComputeLocation = "westeurope",
                AdminUsername = "vdorokhov",
                AdminPassword = "",
                NicName = "NIC-dorlambdacluster",
                PublicIPAddressName = "dorlambdacluster - PubIP",
                VmStorageAccountName = "sfvmdorlambdacluster2029",
                DnsName = "dorlambdacluster",
                VirtualNetworkName = "VNet-dorlambdacluster",
                LbName = "LB-dorlambdacluster",
                LbIPName = "LBIP-dorlambdacluster",
                ApplicationDiagnosticsStorageAccountName = "sfdgdorlambdacluster3902",
                SupportLogStorageAccountName = "sflogsdorlambdaclust8216"
            }
        };
    }
}