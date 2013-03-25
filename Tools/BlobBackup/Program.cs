/*
 * Copyright 2010-2012 10gen Inc.
 * file : Program.cs
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

    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.WindowsAzure.Backup;
    using MongoDB.WindowsAzure.Common;

    class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 2)
            {
                PrintUsage();
                return;
            }

            // Run the requested command and record whether it worked.
            bool success = Run(args[0], args[1]);

            Environment.Exit(success ? 0 : -1);
        }

        private static bool Run(string command, string arg)
        {
            // Ensure they're running a valid command first.
            if (!new List<string> { "snapshot", "snapshotAndBackup", "backup" }.Contains(command))
            {
                Console.WriteLine("Error: command \"" + command + "\" not recognized.");
                PrintUsage();
                return false;
            }

            // Determine which actions we need to run.
            command = command.ToLower();
            bool doSnapshot = command.Contains("snapshot");
            bool doBackup = command.Contains("backup");

            // Part 1: Snapshot (or read the URI from the arguments).
            Uri snapshotUri = null;
            if (doSnapshot)
            {
                var mongoDBRoleCount = ConnectionUtilities.GetDatabaseWorkerRoles().Count;
                int instanceId = 0;
                bool isInt = int.TryParse(arg, out instanceId);
                if (!isInt || instanceId < 0 || instanceId > (mongoDBRoleCount-1))
                {
                    Console.WriteLine("ERROR: \"{0}\" is not a valid instance number.", arg);
                    return false;
                }
                else
                {
                    snapshotUri = Snapshot(instanceId);
                }
            }
            else
            {
                if (!Uri.TryCreate(arg, UriKind.Absolute, out snapshotUri))
                {
                    Console.WriteLine("ERROR: \"{0}\" is not a valid url", arg);
                    return false;
                }
            }

            // Part 2: Backup.
            if (doBackup)
            {
                return Backup(snapshotUri);
            }
            else
            {
                return true; // Already done.
            }
        }

        /// <summary>
        /// Snapshots the instance with the given number. Returns the URI of the snapshot.
        /// </summary>
        private static Uri Snapshot(int instance)
        {
            Console.WriteLine("Snapshotting instance #" + instance + "...");

            var snapshotUri = SnapshotManager.MakeSnapshot(instance, RoleSettings.StorageCredentials, RoleSettings.ReplicaSetName);
            Console.WriteLine("Done: " + snapshotUri);
            return snapshotUri;

        }

        /// <summary>
        /// Creates a backup of the blob with the given URI; returns whether successful.
        /// </summary>
        private static bool Backup(Uri snapshotUri)
        {
            if (!RoleEnvironment.IsAvailable)
            {
                Console.Write("To run a backup, the tool must be run from with an Azure deployed environment.");
                return false;
            }

            if (snapshotUri == null)
            {
                Console.WriteLine("Snapshot URI cannot be null.");
                return false;
            }

            Console.WriteLine("Starting backup on " + snapshotUri + "...");
            var job = new BackupJob(snapshotUri, RoleSettings.StorageCredentials, true);

            job.StartBlocking(); // Runs for a while...

            return true;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage: BlobBackup <command>");
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("   snapshot <id>");
            Console.WriteLine("   snapshotAndBackup <id>");
            Console.WriteLine("   backup <url>");
        }
    }
}
