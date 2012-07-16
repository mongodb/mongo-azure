/*
 * Copyright 2010-2012 10gen Inc.
 * file : BackupEngine.cs
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

namespace MongoDB.WindowsAzure.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Microsoft.WindowsAzure.StorageClient;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using System.IO;
    using tar_cs;
    using MongoDB.WindowsAzure.Common;
    using System.Threading;

    /// <summary>
    /// Backs up blobs that store MongoDB's data.
    /// </summary>
    public class BackupJob
    {
        private static int nextJobId = 1;

        public int Id { get; private set; }

        public Uri SnapshotUri { get; private set; }

        public string Credentials { get; private set; }

        public string BackupContainerName { get; private set; }

        private Thread thread;     

        public BackupJob( Uri blobUri, string credentials,  string backupContainerName = "mongobackups" )
        {
            this.Id = nextJobId++;
            this.SnapshotUri = blobUri;
            this.Credentials = credentials;
            this.BackupContainerName = backupContainerName;
            thread = new Thread(Run);
        }

        public void Start()
        {            
            thread.Start();
        }

        private void Run()
        {
            var output = Console.Out;

            // Set up the cache, storage account, and blob client.
            output.WriteLine("Getting the cache...");
            LocalResource localResource = RoleEnvironment.GetLocalResource(Constants.BackupLocalStorageName);
            output.WriteLine("Initializing the cache...");
            CloudDrive.InitializeCache(localResource.RootPath, localResource.MaximumSizeInMegabytes);
            output.WriteLine("Setting up storage account...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Credentials);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // Mount the snapshot.
            output.WriteLine("Mounting the snapshot...");
            CloudDrive snapshottedDrive = new CloudDrive(SnapshotUri, storageAccount.Credentials);
            string driveLetter = snapshottedDrive.Mount(0, DriveMountOptions.None);
            output.WriteLine("...snapshot mounted to " + driveLetter);

            // Open the backups container.
            output.WriteLine("Opening (or creating) the backup container...");
            CloudBlobContainer backupContainer = client.GetContainerReference(BackupContainerName);
            backupContainer.CreateIfNotExist();

            // Create the destination blob.
            string blobFileName = String.Format("backup_{0}-{1}-{2}_{3}-{4}.tar", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
            var blob = backupContainer.GetBlobReference(blobFileName);

            // Write everything in the mounted snapshot, to the TarWriter stream, to the BlobStream, to the blob.            
            output.WriteLine("Backing up:\n\tpath: " + driveLetter + "\n\tto blob: " + blobFileName + "\n");
            using (var outputStream = blob.OpenWrite())
            {
                using (var tar = new TarWriter(outputStream))
                {
                    output.WriteLine("Writing to the blob/tar...");
                    AddAllToTar(driveLetter, tar, output);
                }
            }

            // Set the blob's metadata.
            output.WriteLine("Setting the blob's metadata...");
            blob.Metadata["FileName"] = blobFileName;
            blob.Metadata["Submitter"] = "BlobBackup";
            blob.SetMetadata();

            // Lastly, unmount the drive.
            output.WriteLine("Unmounting the drive...");
            snapshottedDrive.Unmount();
            output.WriteLine("Done.");
        }

        /// <summary>
        /// Adds every file in the directory to the tar, and recurses into subdirectories.
        /// </summary>
        private static void AddAllToTar(string root, TarWriter tar, TextWriter output)
        {
            output.WriteLine("Opening in " + root + "...");

            // Add subdirectories...
            foreach (var directory in Directory.GetDirectories(root))
                AddAllToTar(directory, tar, output);

            foreach (var file in Directory.GetFiles(root))
            {
                var info = new FileInfo(file);
                output.WriteLine("Writing " + info.Name + "... (" + Util.FormatFileSize(info.Length) + ")");

                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    tar.Write(fs);
            }
        }
    }
}
