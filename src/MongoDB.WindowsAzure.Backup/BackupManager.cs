/*
 * Copyright 2010-2012 10gen Inc.
 * file : BackupManager.cs
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace MongoDB.WindowsAzure.Backup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;
    using System.IO;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using MongoDB.WindowsAzure.Common;
    using Microsoft.WindowsAzure;

    /// <summary>
    /// Manages backup files that are stored in Azure blob storage.
    /// </summary>
    public static class BackupManager
    {      
        /// <summary>
        /// Returns all TAR backups available as a list of blobs.
        /// <param name="credential">The Azure Storage credential string to use</param>
        /// <param name="replicaSetName">The name of the MongoDB replica set</param>
        /// </summary>
        public static List<CloudBlob> GetBackups(string credential, string replicaSetName)
        {
            var storageAccount = CloudStorageAccount.Parse(credential);
            var client = storageAccount.CreateCloudBlobClient();

            // Load the container.
            try
            {
                var container = client.GetContainerReference(Constants.BackupContainerName);
                container.FetchAttributes();

                // Collect all the blobs!
                return container.ListBlobs().Select(item => ((CloudBlob) item)).Where(item => item.Name.EndsWith(".tar")).ToList();
            }
            catch (StorageClientException) // Container not found...
            {
                return new List<CloudBlob>();
            }
        }
    }
}
