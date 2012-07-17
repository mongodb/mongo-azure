using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using MongoDB.WindowsAzure.Common;
using Microsoft.WindowsAzure;

namespace MongoDB.WindowsAzure.Tools
{
    public class BackupManager
    {      
        public static List<CloudBlob> GetBackups(string credentials, string replicaSetName = "rs", TextWriter output = null)
        {
            output = output ?? Console.Out; // Output defaults to Console

            var storageAccount = CloudStorageAccount.Parse(credentials);
            var client = storageAccount.CreateCloudBlobClient();

            // Load the container.
            var container = client.GetContainerReference(Constants.BackupContainerName);
            container.FetchAttributes();

            // Collect all the snapshots!
            return container.ListBlobs().Select(item => ((CloudBlob) item)).Where(item => item.Name.EndsWith(".tar")).ToList();
        }
    }
}
