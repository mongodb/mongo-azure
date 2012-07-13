using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using MongoDB.WindowsAzure.Common;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;

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
            output = output ?? Console.Out;

            // Set up the cache, storage account, and blob client.
            output.WriteLine("Getting the cache...");
            LocalResource localResource = RoleEnvironment.GetLocalResource(Constants.BackupLocalStorageName);
            output.WriteLine("Initializing the cache...");
            CloudDrive.InitializeCache(localResource.RootPath, localResource.MaximumSizeInMegabytes);
            output.WriteLine("Setting up storage account...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(credentials);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // Open the container that stores the MongoDBRole data drives.
            output.WriteLine("Loading the MongoDB data drive container...");
            CloudBlobContainer dataContainer = new CloudBlobContainer(String.Format(Constants.MongoDataContainerName, replicaSetName), client);

            // Load the drive and snapshot it.
            output.WriteLine("Loading the drive...");
            CloudDrive originalDrive = new CloudDrive(dataContainer.GetPageBlobReference(String.Format("mongoddblob{0}.vhd", instanceNum)).Uri, storageAccount.Credentials);
            output.WriteLine("Snapshotting the drive...");
            Uri snapshotUri = originalDrive.Snapshot();
            output.WriteLine("...snapshotted to: " + snapshotUri);
            return snapshotUri;
        }

        public static List<DateTime> GetSnapshots(string credentials, string replicaSetName = "rs", TextWriter output = null)
        {
            // Set up the cache, storage account, and blob client.           
            output.WriteLine("Setting up storage account...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(credentials);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // Open the container that stores the MongoDBRole data drives.
            output.WriteLine("Loading the MongoDB data drive container...");
            CloudBlobContainer dataContainer = new CloudBlobContainer(String.Format(Constants.MongoDataContainerName, replicaSetName), client);

            // Collect all the snapshots!

            //List<DateTime snaps
            foreach (var blobInfo in dataContainer.ListBlobs())
            {
                var blob = dataContainer.GetBlobReference(blobInfo.Uri.ToString());
                blob.FetchAttributes();
                Console.WriteLine(blob.Uri.ToString());
            }

            //output.WriteLine("Loading the drive...");
            //CloudDrive originalDrive = new CloudDrive(dataContainer.GetPageBlobReference(vhdToBackup).Uri, storageAccount.Credentials);
            //output.WriteLine("Snapshotting the drive...");
            //Uri snapshotUri = originalDrive.Snapshot();
            //output.WriteLine("...snapshotted to: " + snapshotUri);
            return null;
        }
    }
}
