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
    /// <summary>
    /// Manages backup files that are stored in Azure blob storage.
    /// </summary>
    public class BackupManager
    {      
        /// <summary>
        /// Returns all TAR backups availablr as a list of blobs.
        /// </summary>
        public static List<CloudBlob> GetBackups(string credentials, string replicaSetName = "rs")
        {
            var storageAccount = CloudStorageAccount.Parse(credentials);
            var client = storageAccount.CreateCloudBlobClient();

            // Load the container.
            var container = client.GetContainerReference(Constants.BackupContainerName);
            container.FetchAttributes();

            // Collect all the blobs!
            return container.ListBlobs().Select(item => ((CloudBlob) item)).Where(item => item.Name.EndsWith(".tar")).ToList();
        }
    }
}
