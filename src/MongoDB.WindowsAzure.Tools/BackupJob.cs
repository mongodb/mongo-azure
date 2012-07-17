/*
 * Copyright 2010-2012 10gen Inc.
 * file : BackupJob.cs
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
    /// A long-running job that backs up MongoDB's data.
    /// </summary>
    public class BackupJob
    {
        private static int nextJobId = 1;

        //=================================================================================
        //
        //  PROPERTIES
        //
        //=================================================================================

        /// <summary>
        /// The job's ID. These are used in the UI to reference jobs.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The URI of the blob snapshot (must be the full URI including the date/time of the snapshot) that we are backing up.
        /// </summary>
        public Uri UriToBackup { get; private set; }

        /// <summary>
        /// The storage credentials we are using to run the backup.
        /// </summary>
        public string Credentials { get; private set; }

        /// <summary>
        /// The name of the container where we will store the backup file.
        /// </summary>
        public string BackupContainerName { get; private set; }

        /// <summary>
        /// The full log history of this job (thread-safe, returns a copy).
        /// </summary>
        public LinkedList<string> LogHistory
        {
            get
            {
                lock (log)
                {
                    return new LinkedList<string>(log);
                }
            }
        }

        /// <summary>
        /// The last log message this job recorded (thread-safe, fast).
        /// </summary>
        public string LastLongEntry
        {
            get
            {
                lock (log)
                {
                    return log.Last.Value;
                }
            }
        }

        /// <summary>
        /// The date the job finished - or null if it has not.
        /// </summary>
        public DateTime? DateFinished { get; private set; }

        //=================================================================================
        //
        //  PRIVATE VARIABLES
        //
        //=================================================================================

        private LinkedList<string> log;

        private Thread thread;

        //=================================================================================
        //
        //  CONSTRUCTORS
        //
        //=================================================================================

        public BackupJob(Uri blobUri, string credentials, string backupContainerName = Constants.BackupContainerName)
        {
            this.Id = nextJobId++; // TODO should probably keep an atomic lock on nextJobId.
            this.UriToBackup = blobUri;
            this.Credentials = credentials;
            this.BackupContainerName = backupContainerName;
            this.log = new LinkedList<string>();
            thread = new Thread(RunSafe);
        }

        //=================================================================================
        //
        //  PUBLIC METHODS
        //
        //=================================================================================

        public void Start()
        {
            thread.Start();
        }

        /// <summary>
        /// Converts this job into a simplified object for network transmission.
        /// </summary>
        public object ToJson()
        {
            return new { id = Id, lastLine = LastLongEntry, uri = UriToBackup };
        }

        //=================================================================================
        //
        //  PRIVATE METHODS
        //
        //=================================================================================

        /// <summary>
        /// Runs the backup, but escapes any exceptions and sends them to the log.
        /// </summary>
        private void RunSafe()
        {
            try
            {
                Run();
            }
            catch (Exception e)
            {
                Log("=========================");
                Log("FAILURE: " + e.Message);
                Log(e.StackTrace);
                Log("");
                Log("Terminating now.");
            }
            finally
            {
                DateFinished = DateTime.Now;
            }
        }

        /// <summary>
        /// The actual backup logic itself.
        /// We mount the VHD snapshot, then TAR and copy the contents to a new blob.
        /// </summary>
        private void Run()
        {
            Log("Backup started for " + UriToBackup + "...");

            // Set up the cache, storage account, and blob client.
            Log("Getting the cache...");
            LocalResource localResource = RoleEnvironment.GetLocalResource(Constants.BackupLocalStorageName);
            Log("Initializing the cache...");
            CloudDrive.InitializeCache(localResource.RootPath, localResource.MaximumSizeInMegabytes);
            Log("Setting up storage account...");
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Credentials);
            CloudBlobClient client = storageAccount.CreateCloudBlobClient();

            // Mount the snapshot.
            Log("Mounting the snapshot...");
            CloudDrive snapshottedDrive = new CloudDrive(UriToBackup, storageAccount.Credentials);
            string driveLetter = snapshottedDrive.Mount(0, DriveMountOptions.None);
            Log("...snapshot mounted to " + driveLetter);

            // Create the destination blob.
            Log("Opening (or creating) the backup container...");
            CloudBlobContainer backupContainer = client.GetContainerReference(BackupContainerName);
            backupContainer.CreateIfNotExist();
            var blobFileName = String.Format("backup_{0}-{1}-{2}_{3}-{4}.tar", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
            var blob = backupContainer.GetBlobReference(blobFileName);

            // Write everything in the mounted snapshot, to the TarWriter stream, to the BlobStream, to the blob.            
            Log("Backing up:\n\tpath: " + driveLetter + "\n\tto blob: " + blobFileName + "\n");
            using (var outputStream = blob.OpenWrite())
            {
                using (var tar = new TarWriter(outputStream))
                {
                    Log("Writing to the blob/tar...");
                    AddAllToTar(driveLetter, tar);
                }
            }

            // Set the blob's metadata.
            Log("Setting the blob's metadata...");
            blob.Metadata["FileName"] = blobFileName;
            blob.Metadata["Submitter"] = "BlobBackup";
            blob.SetMetadata();

            // Lastly, unmount the drive.
            Log("Unmounting the drive...");
            snapshottedDrive.Unmount();
            Log("Done.");
        }

        /// <summary>
        /// Sends the message to our "log".
        /// </summary>
        private void Log(string message)
        {
            lock (log)
            {
                log.AddLast(message);
            }
        }

        /// <summary>
        /// Adds every file in the directory to the tar, and recurses into subdirectories.
        /// </summary>
        private void AddAllToTar(string root, TarWriter tar)
        {
            Log("Opening in " + root + "...");

            // Add subdirectories...
            foreach (var directory in Directory.GetDirectories(root))
                AddAllToTar(directory, tar);

            foreach (var file in Directory.GetFiles(root))
            {
                var info = new FileInfo(file);
                Log("Writing " + info.Name + "... (" + Util.FormatFileSize(info.Length) + ")");

                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    tar.Write(fs);
            }
        }
    }
}
