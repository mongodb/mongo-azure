using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using MongoDB.WindowsAzure.Common;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient.Protocol;

namespace MongoDB.WindowsAzure.Tools
{
    /// <summary>
    /// Manages the creation and retrieval of MongoDB data drive snapshots.
    /// </summary>
    public class SnapshotManager
    {
        /// <summary>
        /// Returns all the snapshots available.
        /// </summary>
        public static List<CloudBlob> GetSnapshots(string credentials, string replicaSetName = "rs")
        {
            var client = CloudStorageAccount.Parse(credentials).CreateCloudBlobClient();

            // Load the container.
            var container = client.GetContainerReference(String.Format(Constants.MongoDataContainerName, replicaSetName));

            // Collect all the snapshots!
            return container.ListBlobs(new BlobRequestOptions()
            {
                BlobListingDetails = BlobListingDetails.Snapshots,
                UseFlatBlobListing = true
            }).Select(item => ((CloudBlob) item)).Where(item => item.SnapshotTime.HasValue).ToList();
        }

        /// <summary>
        /// Snapshots the data drive of the given instance ID.
        /// </summary>
        public static Uri MakeSnapshot(int instanceNum, string credentials, string replicaSetName = "rs")
        {
            var client = CloudStorageAccount.Parse(credentials).CreateCloudBlobClient();

            // Load the blob...
            var container = client.GetContainerReference(String.Format(Constants.MongoDataContainerName, replicaSetName));
            var blob = container.GetPageBlobReference(String.Format("mongoddblob{0}.vhd", instanceNum));

            // Snapshot it.
            var snapshot = blob.CreateSnapshot();
            return GetSnapshotUri(snapshot);
        }

        /// <summary>
        /// Deletes the blob with the given URI.
        /// </summary>
        public static void DeleteBlob(string uri, string credentials)
        {
            var client = CloudStorageAccount.Parse(credentials).CreateCloudBlobClient();
            var blob = client.GetBlobReference(uri);

            blob.Delete();         
        }

        /// <summary>
        /// Returns the snapshot's *full* URI including the snapshot date/time.
        /// </summary>
        public static Uri GetSnapshotUri(CloudBlob blob)
        {
            return BlobRequest.Get(blob.Uri, 0, blob.SnapshotTime.Value, null).Address;
        }
    }
}
