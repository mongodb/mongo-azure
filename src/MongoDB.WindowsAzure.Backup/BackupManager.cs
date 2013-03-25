/*
 * Copyright 2010-2013 10gen Inc.
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

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.StorageClient;
    
    using MongoDB.WindowsAzure.Common;

    /// <summary>
    /// Manages backup files that are stored in Azure blob storage.
    /// </summary>
    public static class BackupManager
    {      
        /// <summary>
        /// Returns all TAR backups available as a list of blobs.
        /// <param name="credential">The Azure Storage credential string 
        ///     to use</param>
        /// <param name="replicaSetName">The name of the MongoDB replica
        ///     set</param>
        /// <returns>The list of all Blobs. An empty list of no backsups 
        ///     have yet been made</returns>
        /// </summary>
        public static List<CloudBlob> GetBackups(string credential, string replicaSetName)
        {
            var storageAccount = CloudStorageAccount.Parse(credential);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(Constants.BackupContainerName);

            try
            {
                // Collect all the blobs!
                return container.ListBlobs().Select(item => ((CloudBlob) item)).
                    Where(item => item.Name.EndsWith(".tar", 
                        StringComparison.OrdinalIgnoreCase)).
                        ToList();
            }
            catch (StorageClientException sce) // Container not found...
            {
                if (sce.ErrorCode == StorageErrorCode.ContainerNotFound)
                {
                    return new List<CloudBlob>();
                }
                throw;
            }
        }
    }
}
