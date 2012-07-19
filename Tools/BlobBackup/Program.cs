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
    using Microsoft.WindowsAzure;
    using MongoDB.WindowsAzure.Tools.BlobBackup.Properties;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using MongoDB.WindowsAzure.Common;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("BlobBackup starting...");
            
            // Verify that we are running from within Azure.
            Console.Write("Verifying role environment...");
            if (RoleEnvironment.IsAvailable)
                Console.WriteLine(" success.");
            else
            {
                Console.WriteLine("failed.\nPlease run this tool from within Azure.");
                return;
            }

            Console.WriteLine("Replica set: " + RoleSettings.ReplicaSetName);
            var uri = SnapshotManager.MakeSnapshot(0, RoleSettings.StorageCredentials, RoleSettings.ReplicaSetName);
            var job = new BackupJob(uri, RoleSettings.StorageCredentials);
            job.Start();
        }
    }
}
