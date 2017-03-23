using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using ResourceConfiguration;

namespace LambdaArchitecture.Setup
{
    public class ResourceManager
    {
        private const string AzureManagementUrl = "https://management.azure.com/";
        private ResourceManagerSettings _settings { get; }

        public ResourceManager(ResourceManagerSettings settings)
        {
            _settings = settings;
        }

        public void AddResource(string templatePath, string parametersPath, params string[] parameters)
        {
            DeploymentExtended result = null;
            Task<DeploymentExtended> dpResult = CreateTemplateDeploymentAsync(templatePath, parametersPath, parameters);

            try
            {
                result = dpResult.Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured");
                Console.WriteLine(dpResult.Exception.InnerException.Message);
                Console.ReadLine();
                return;
            }

            Console.WriteLine(result.Properties.ProvisioningState);
        }

        public async Task DeleteResourceGroupAsync()
        {
            ResourceManagementClient resourceManagementClient = CreateResourceManagementClient();
            await resourceManagementClient.ResourceGroups.DeleteAsync(_settings.GroupName);
        }

        public async Task CreateResourceGroupAsync()
        {
            ResourceManagementClient resourceManagementClient = CreateResourceManagementClient();
            await resourceManagementClient.ResourceGroups.CreateOrUpdateAsync(_settings.GroupName, new ResourceGroup(_settings.Location));
        }

        protected async Task<AuthenticationResult> GetAccessTokenAsync()
        {
            var clientCredential = new ClientCredential(_settings.ClientId, _settings.ClientSecret);
            var context = new AuthenticationContext(_settings.Authority);
            var token = await context.AcquireTokenAsync(AzureManagementUrl, clientCredential);

            if (token == null)
            {
                throw new InvalidOperationException("Could not get the token.");
            }

            return token;
        }

        public async Task<DeploymentExtended> CreateTemplateDeploymentAsync(string templatePath, string parametersPath, params object[] args)
        {
            Console.WriteLine("Creating the template deployment...");
            var deployment = new Deployment
            {
                Properties = new DeploymentProperties
                {
                    Mode = DeploymentMode.Incremental,
                    Template = File.ReadAllText(templatePath),
                    Parameters = SetTemplateParams(File.ReadAllText(parametersPath), args)
                }
            };

            ResourceManagementClient resourceManagementClient = CreateResourceManagementClient();
            return await resourceManagementClient.Deployments.CreateOrUpdateAsync(
                _settings.GroupName,
                _settings.DeploymentName,
                deployment);
        }

        private ResourceManagementClient CreateResourceManagementClient()
        {
            var token = GetAccessTokenAsync();
            var credentials = new TokenCredentials(token.Result.AccessToken);
            var resourceManagementClient = new ResourceManagementClient(credentials)
            {
                SubscriptionId = _settings.SubscriptionId,
                GenerateClientRequestId = true
            };

            return resourceManagementClient;
        }

        private string SetTemplateParams(string source, params object[] parameters) 
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                source = source.Replace($@"{{{i}}}", parameters[i].ToString());
            }

            return source;
        }
    }
}
