/*
 * Copyright 2010-2013 10gen Inc.
 * file : SnapshotManager.cs
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
    using Microsoft.WindowsAzure.StorageClient.Protocol;
    
    using MongoDB.WindowsAzure.Common;

    /// <summary>
    /// Manages the creation and retrieval of MongoDB data drive snapshots.
    /// </summary>
    public static class SnapshotManager
    {
        /// <summary>
        /// Returns all the snapshots available.
        /// </summary>
        /// <param name="credential">The Azure Storage credential string to use</param>
        /// <param name="replicaSetName">The name of the MongoDB replica set</param>
        public static List<CloudBlob> GetSnapshots(string credential, string replicaSetName)
        {
            var client = CloudStorageAccount.Parse(credential).CreateCloudBlobClient();

            // Load the container.
            var container = client.GetContainerReference(ConnectionUtilities.GetDataContainerName(replicaSetName));

            // Collect all the snapshots!
            return container.ListBlobs(new BlobRequestOptions()
            {
                BlobListingDetails = BlobListingDetails.Snapshots,
                UseFlatBlobListing = true
            }).Select(item => ((CloudBlob) item)).Where(item => item.SnapshotTime.HasValue).ToList();
        }

        /// <summary>
        /// Snapshots the data drive of the given instance number.
        /// </summary>
        /// <param name="instanceNum">The number of the instance to snapshot.</param>
        /// <param name="credential">The Azure Storage credential string to use</param>
        /// <param name="replicaSetName">The name of the MongoDB replica set</param>
        public static Uri MakeSnapshot(int instanceNum, string credential, string replicaSetName)
        {
            var client = CloudStorageAccount.Parse(credential).CreateCloudBlobClient();

            // Load the blob...
            var container = client.GetContainerReference(ConnectionUtilities.GetDataContainerName(replicaSetName));
            var blob = container.GetPageBlobReference(String.Format(Constants.MongoDataBlobName, instanceNum));

            // Snapshot it.
            var snapshot = blob.CreateSnapshot();
            return GetSnapshotUri(snapshot);
        }

        /// <summary>
        /// Deletes the blob with the given URI.
        /// <param name="uri">The URI of the blob to delete.</param>
        /// <param name="credential">The Azure Storage credential string to use</param>
        /// </summary>
        public static void DeleteBlob(string uri, string credential)
        {
            var client = CloudStorageAccount.Parse(credential).CreateCloudBlobClient();
            var blob = client.GetBlobReference(uri);

            blob.Delete();
        }

        /// <summary>
        /// Returns the snapshot's *full* URI including the snapshot date/time.
        /// <param name="blob">The blob to get the URL of</param>
        /// </summary>
        public static Uri GetSnapshotUri(CloudBlob blob)
        {
            return BlobRequest.Get(blob.Uri, 0, blob.SnapshotTime.Value, null).Address;
        }
    }
}
