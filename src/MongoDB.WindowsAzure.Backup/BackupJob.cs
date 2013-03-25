/*
 * Copyright 2010-2013 10gen Inc.
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

namespace MongoDB.WindowsAzure.Backup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    using MongoDB.WindowsAzure.Common;
    
    using tar_cs;

    /// <summary>
    /// A long-running job that backs up MongoDB's data.
    /// </summary>
    public class BackupJob
    {
        private static int nextJobId = 1;
        private static readonly List<string> skipDirectories = new List<string> { "$RECYCLE.BIN" };

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
        public string Credential { get; private set; }

        /// <summary>
        /// The name of the container where we will store the backup file.
        /// </summary>
        public string BackupContainerName { get; private set; }

        /// <summary>
        /// Whether log messages are being sent to the console.
        /// </summary>
        public bool LogToConsole { get; private set; }

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

        /// <summary>
        /// Creates the backup job with the given parameters, but does not start it.
        /// </summary>
        public BackupJob(Uri blobUri, string credentials, bool logToConsole = false, string backupContainerName = Constants.BackupContainerName)
        {
            this.Id = nextJobId++; // TODO we should probably add an atomic lock on nextJobId, so two jobs never have the same ID.
            this.UriToBackup = blobUri;
            this.Credential = credentials;
            this.LogToConsole = logToConsole;
            this.BackupContainerName = backupContainerName;
            this.log = new LinkedList<string>();            
            thread = new Thread(Run);
        }

        //=================================================================================
        //
        //  PUBLIC METHODS
        //
        //=================================================================================

        /// <summary>
        /// Starts this backup job asynchronously.
        /// </summary>
        public void Start()
        {
            thread.Start();
        }

        /// <summary>
        /// Starts this backup job synchronously, on the current thread.
        /// This WILL BLOCK the current thread until the backup is done.
        /// </summary>
        public void StartBlocking()
        {
            Run();
        }

        /// <summary>
        /// Converts this job into a simplified object for network transmission.
        /// </summary>
        public object ToAjaxObject()
        {
            return new { id = Id, lastLine = LastLongEntry, uri = UriToBackup };
        }

        //=================================================================================
        //
        //  PRIVATE METHODS
        //
        //=================================================================================

        /// <summary>
        /// The actual backup logic itself.
        /// We mount the VHD snapshot, then TAR and copy the contents to a new blob.
        /// </summary>
        private void Run()
        {
            CloudDrive snapshottedDrive = null;
            bool mountedSnapshot = false;

            try
            {
                Log("Backup started for " + UriToBackup + "...");

                // Set up the cache, storage account, and blob client.
                Log("Getting the cache...");
                var localResource = RoleEnvironment.GetLocalResource(Constants.BackupLocalStorageName);
                Log("Initializing the cache...");
                CloudDrive.InitializeCache(localResource.RootPath, localResource.MaximumSizeInMegabytes);
                Log("Setting up storage account...");
                var storageAccount = CloudStorageAccount.Parse(Credential);
                var client = storageAccount.CreateCloudBlobClient();

                // Mount the snapshot.
                Log("Mounting the snapshot...");
                snapshottedDrive = new CloudDrive(UriToBackup, storageAccount.Credentials);
                string driveLetter = snapshottedDrive.Mount(0, DriveMountOptions.None);
                mountedSnapshot = true;
                Log("...snapshot mounted to " + driveLetter);

                // Create the destination blob.
                Log("Opening (or creating) the backup container...");
                CloudBlobContainer backupContainer = client.GetContainerReference(BackupContainerName);
                backupContainer.CreateIfNotExist();
                var blobFileName = String.Format(Constants.BackupFormatString, DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
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

                Log("Unmounting the drive..."); // Keep this here because we want "terminating now" to be the last log event in a failure.
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
                // Unmount the drive.
                if (mountedSnapshot)
                {
                    snapshottedDrive.Unmount();
                }

                DateFinished = DateTime.Now;
            }
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

            if (LogToConsole)
            {
                Console.WriteLine(message);
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
            {
                var dirName = new DirectoryInfo(directory).Name;
                if (skipDirectories.Contains(dirName.ToUpperInvariant()))
                {
                    Log("Skipping directory "+directory+" and its subdirectories");
                }
                else
                {
                    AddAllToTar(directory, tar);
                }
            }

            foreach (var file in Directory.GetFiles(root))
            {
                var info = new FileInfo(file);
                Log("Writing " + info.Name + "... (" + Util.FormatFileSize(info.Length) + ")");

                using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    tar.Write(fs);
                }
            }
        }
    }
}
