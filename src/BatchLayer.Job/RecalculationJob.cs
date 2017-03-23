using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Hyak.Common;
using Microsoft.Azure.Management.HDInsight.Job;
using Microsoft.Azure.Management.HDInsight.Job.Models;
using ResourceConfiguration;
using Utils;

namespace BatchLayer.Job
{
    public class RecalculationJob
    {
        private readonly StorageConfiguration _storageConfiguration;
        private readonly HDInsightConfiguration _configuration;
        private HDInsightJobManagementClient _hdiJobManagementClient;
        private AzureStorageAccess _storageAccess;

        public RecalculationJob(StorageConfiguration storageConfiguration, HDInsightConfiguration configuration)
        {
            _storageConfiguration = storageConfiguration;
            _configuration = configuration;
            
            _storageAccess = new AzureStorageAccess(
                _configuration.DefaultStorageAccountName, 
                _configuration.DefaultStorageAccountKey,
                _configuration.DefaultStorageContainerName);

            var clusterCredentials = new BasicAuthenticationCloudCredentials { Username = _configuration.ExistingClusterUsername, Password = _configuration.ExistingClusterPassword };
            _hdiJobManagementClient = new HDInsightJobManagementClient(_configuration.ExistingClusterUri, clusterCredentials);
        }

        public void Execute(DateTime dateTime)
        {
            var query = string.Format(CultureInfo.InvariantCulture, HiveQueries.CountMessagesByLocation, 
                _configuration.MasterDatasetPath, 
                dateTime.DateTimeToUnixTimestamp(),
                _configuration.MasterDatasetPath + _storageConfiguration.BatchViewsContainerName);
            SubmitHiveJob(query.Replace(Environment.NewLine, ""));
        }

        private void SubmitHiveJob(string query)
        {
            var parameters = new HiveJobSubmissionParameters
            {
                Query = query
            };

            // submitting the Hive job to the cluster
            JobSubmissionResponse jobResponse = _hdiJobManagementClient.JobManagement.SubmitHiveJob(parameters);
            string jobId = jobResponse.JobSubmissionJsonResponse.Id;

            // wait for job completion
            JobDetailRootJsonObject jobDetail = _hdiJobManagementClient.JobManagement.GetJob(jobId).JobDetail;
            while (!jobDetail.Status.JobComplete)
            {
                Thread.Sleep(1000);
                jobDetail = _hdiJobManagementClient.JobManagement.GetJob(jobId).JobDetail;
            }

            // get job output
            Stream output = jobDetail.ExitValue == 0
                ? _hdiJobManagementClient.JobManagement.GetJobOutput(jobId, _storageAccess)
                : _hdiJobManagementClient.JobManagement.GetJobErrorLogs(jobId, _storageAccess);
            
            // handle output
            using (var reader = new StreamReader(output, Encoding.UTF8))
            {
                string value = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(value))
                {
                    throw new BatchViewCalculationException(value);
                }
            }
        }
    }
}