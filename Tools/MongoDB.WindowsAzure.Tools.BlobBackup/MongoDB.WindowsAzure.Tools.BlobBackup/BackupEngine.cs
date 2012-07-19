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

namespace MongoDB.WindowsAzure.Tools.BlobBackup
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

    class BackupEngine
    {
        /// <summary>
        /// Backups up the contents of the first MongoDB data drive in the specified storage account.
        /// The actual files in the data drive are backed up, not just the vhd image.
        /// The backup is stored as a TAR file, with a timestamp-derived name, in the specified container.
        /// 
        /// This function must be called from within Azure. (Mounting a cloud drive is otherwise impossible)
        /// 
        /// Procedure:
        ///    1) The data drive is snapshotted. (This prevents inconsistent data)
        ///    2) This snapshot is mounted.
        ///    3) The destination blob is created, and a TarWriter is created to write to it.
        ///    4) All the files in the mount are written to the blob, through the TarWriter.
        /// </summary>
        /// <param name="credentials">The Azure storage credentials to use.</param>
        /// <param name="replicaSetName">Name of the replica set (default is "rs")</param>
        /// <param name="backupContainerName">Name of the container that will store the backups (default is "mongobackups")</param>
        public static Uri Snapshot(string credentials, TextWriter output, string replicaSetName = "rs", string vhdToBackup = "mongoddblob1.vhd")
        {
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
            CloudDrive originalDrive = new CloudDrive(dataContainer.GetPageBlobReference(vhdToBackup).Uri, storageAccount.Credentials);
            output.WriteLine("Snapshotting the drive...");
            Uri snapshotUri = originalDrive.Snapshot();
            output.WriteLine("...snapshotted to: " + snapshotUri);
            return snapshotUri;
        }

        public static void Backup(string credentials, Uri snapshotUri, TextWriter output, string backupContainerName = "mongobackups")
        {
            if (snapshotUri == null)
                throw new ArgumentNullException("Snapshot URI cannot be null");

            // Set up the cache, storage account, and blob client.
            output.WriteLine("Getting the cache...");
            LocalResource localResource = RoleEnvironment.GetLocalResource(Constants.BackupLocalStorageName);
            output.WriteLine("Initializing the cache...");
            CloudDrive.InitializeCache(localResource.RootPath, localResource.MaximumSizeInMegabytes);
            output.WriteLine("Setting up storage account...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(credentials);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // Mount the snapshot.
            output.WriteLine("Mounting the snapshot...");
            CloudDrive snapshottedDrive = new CloudDrive(snapshotUri, storageAccount.Credentials);
            string driveLetter = snapshottedDrive.Mount(0, DriveMountOptions.None);
            output.WriteLine("...snapshot mounted to " + driveLetter);

            // Open the backups container.
            output.WriteLine("Opening (or creating) the backup container...");
            CloudBlobContainer backupContainer = client.GetContainerReference(backupContainerName);
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
