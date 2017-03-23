using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using ResourceConfiguration;

namespace LambdaArchitecture.Setup
{
    /// <summary>
    /// DevOps. Setups all required infrustructure: storages, services on Azure
    /// </summary>
    public static class LambdaCluster
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager(DeploymentConfiguration.Default.ResourceManagerSettings);

        private static void Main(string[] args)
        {
         // FullCleanUp();
            IntegrationTestSetup();

            Console.ReadLine();
        }

        public static void FullSetup()
        {
            FullCleanUp();

            // 1. Create Resource group
            CreateResourceGroup();

            // 2. Creates blob storage
            CreateBlobStorage();

            // 3. Creates Redis storage
            CreateRedisStorage();

            // 4. Creates HdInsight cluster
            CreateHdInsightCluster();

            // 5. Creates publish-subscribe by Service Bus brokered messaging
            CreateServiceBus();

            // 6. Creates Stream, Serving and Batch instances on Azure
            CreateServiceFabrik();

            // 7. Deploy Stream, Serving and Batch services on Azure
            DeployServices();
        }

        public static void IntegrationTestSetup()
        {
            // 1. Create Resource group
           // CreateResourceGroup();

            // 2. Creates blob storage
         //   CreateBlobStorage();

            // 3. Creates Redis storage
          //  CreateRedisStorage();

            // 4. Creates HdInsight cluster
            CreateHdInsightCluster();

            // 5. Creates publish-subscribe by Service Bus brokered messaging
            CreateServiceBus();
        }

        private static void CreateResourceGroup()
        {
            Console.WriteLine("Creating Resource group on Azure");
            var createTask = _resourceManager.CreateResourceGroupAsync();
            try
            {
                createTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Resource group is created");
        }

        private static void CreateBlobStorage()
        {
            Console.WriteLine("Creating Blob storage on Azure");
            _resourceManager.AddResource(
                "../../Templates/AzureStorage.json",
                "../../Templates/AzureStorageParameters.json",
                DeploymentConfiguration.Default.Storage.Name,
                DeploymentConfiguration.Default.Storage.Location,
                DeploymentConfiguration.Default.Storage.AccountType);
            Console.WriteLine("Blob storage is created");
        }

        private static void CreateRedisStorage()
        {
            Console.WriteLine("Creating Redis storage on Azure");
            _resourceManager.AddResource(
                "../../Templates/RedisStorage.json",
                "../../Templates/RedisStorageParameters.json",
                DeploymentConfiguration.Default.Redis.Name,
                DeploymentConfiguration.Default.Redis.Location);
            Console.WriteLine("Redis is created");
        }

        private static void CreateHdInsightCluster()
        {
            Console.WriteLine("Creating HdIndight on Azure");
            _resourceManager.AddResource(
                "../../Templates/HdInsightCluster.json",
                "../../Templates/HdInsightClusterParameters.json",
                DeploymentConfiguration.Default.HDInsight.ExistingClusterName);
            Console.WriteLine("HdInsight is created");
        }

        private static void CreateServiceBus()
        {
            Console.WriteLine("Creating Service Bus on Azure");
            _resourceManager.AddResource(
                "../../Templates/ServiceBusTopic.json",
                "../../Templates/ServiceBusTopicParameters.json",
                DeploymentConfiguration.Default.ServiceBus.ServiceBusNamespace,
                DeploymentConfiguration.Default.ServiceBus.TopicName,
                DeploymentConfiguration.Default.ServiceBus.BatchLayerSubscription,
                DeploymentConfiguration.Default.ServiceBus.SpeedLayerSubscription);
            Console.WriteLine("Service Bus is created");
        }

        private static void CreateServiceFabrik()
        {
            Console.WriteLine("Creating Service Fabrik on Azure");
            _resourceManager.AddResource(
                "../../Templates/ServiceFabrikCluster.json",
                "../../Templates/ServiceFabrikClusterParameters.json",
                DeploymentConfiguration.Default.ServiceFabrikSettings.ClusterName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.ClusterLocation,
                DeploymentConfiguration.Default.ServiceFabrikSettings.ComputeLocation,
                DeploymentConfiguration.Default.ServiceFabrikSettings.AdminUsername,
                DeploymentConfiguration.Default.ServiceFabrikSettings.AdminPassword,
                DeploymentConfiguration.Default.ServiceFabrikSettings.NicName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.PublicIPAddressName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.VmStorageAccountName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.DnsName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.VirtualNetworkName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.LbName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.LbIPName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.ApplicationDiagnosticsStorageAccountName,
                DeploymentConfiguration.Default.ServiceFabrikSettings.SupportLogStorageAccountName);
            Console.WriteLine("Service Fabrik is created");
        }

        private static void DeployServices()
        {
            Console.WriteLine("Deploying Services on Azure");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = @"powershell.exe";
            startInfo.Arguments = string.Format(CultureInfo.InvariantCulture, @"'{0}'", Path.GetFullPath("../../../LambdaArchitecture.Application/Scripts/Deploy-FabricApplication.ps1"));
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;

            Process process = new Process { StartInfo = startInfo };
            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine("PowerShell is executed");
            Console.WriteLine(output);
            string errors = process.StandardError.ReadToEnd();
            Console.WriteLine(errors);
            Console.WriteLine("Services are deployed");
        }

        public static void FullCleanUp()
        {
            Console.WriteLine("Deleting resource group");
            var deleteTask = _resourceManager.DeleteResourceGroupAsync();
            try
            {
                deleteTask.Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("Resource group is deleted");
        }
    }
}