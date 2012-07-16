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
    public class SnapshotManager
    {
        public static Uri MakeSnapshot(int instanceNum, TextWriter output = null)
        {
            var replicaSetName = RoleEnvironment.GetConfigurationSettingValue(Constants.ReplicaSetNameSetting);
            var credentialString = RoleEnvironment.GetConfigurationSettingValue(Constants.MongoDataCredentialSetting);

            return MakeSnapshot(credentialString, instanceNum, replicaSetName, output);
        }

        public static Uri MakeSnapshot(string credentials, int instanceNum, string replicaSetName = "rs", TextWriter output = null)
        {
            output = output ?? Console.Out; // Output defaults to Console

            var storageAccount = CloudStorageAccount.Parse(credentials);
            var client = storageAccount.CreateCloudBlobClient();

            // Load the blob and snapshot it.
            output.WriteLine("Loading the drive...");
            var container = client.GetContainerReference(String.Format(Constants.MongoDataContainerName, replicaSetName));
            var blob = container.GetPageBlobReference(String.Format("mongoddblob{0}.vhd", instanceNum));

            output.WriteLine("Snapshotting the drive...");
            var snapshot = blob.CreateSnapshot();
            var uri = ToSnapshotUri(snapshot);

            output.WriteLine("...snapshotted to: " + uri);
            return uri;
        }

        public static void DeleteBlob(string uri, string credentials)
        {
            var storageAccount = CloudStorageAccount.Parse(credentials);
            var client = storageAccount.CreateCloudBlobClient();

            var blob = client.GetBlobReference(uri);
            blob.Delete();         
        }

        public static List<CloudBlob> GetSnapshots(TextWriter output = null)
        {
            var replicaSetName = RoleEnvironment.GetConfigurationSettingValue(Constants.ReplicaSetNameSetting);
            var credentialString = RoleEnvironment.GetConfigurationSettingValue(Constants.MongoDataCredentialSetting);

            return GetSnapshots(credentialString, replicaSetName, output);
        }

        public static List<CloudBlob> GetSnapshots(string credentials, string replicaSetName = "rs", TextWriter output = null)
        {
            output = output ?? Console.Out; // Output defaults to Console

            var storageAccount = CloudStorageAccount.Parse(credentials);
            var client = storageAccount.CreateCloudBlobClient();

            // Load the container.
            var container = client.GetContainerReference(String.Format(Constants.MongoDataContainerName, replicaSetName));

            // Collect all the snapshots!
            return container.ListBlobs(new BlobRequestOptions()
            {
                BlobListingDetails = BlobListingDetails.Snapshots,
                UseFlatBlobListing = true
            }).Select(item => ((CloudBlob) item)).Where(item => item.SnapshotTime.HasValue).ToList();           
        }

        public static Uri ToSnapshotUri(CloudBlob blob)
        {
            return BlobRequest.Get(blob.Uri, 0, blob.SnapshotTime.Value, null).Address;
        }
    }
}
