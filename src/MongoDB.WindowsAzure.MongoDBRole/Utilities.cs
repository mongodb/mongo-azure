/*
 * Copyright 2010-2012 10gen Inc.
 * file : Utilities.cs
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

namespace MongoDB.WindowsAzure.MongoDBRole
{

    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Microsoft.WindowsAzure.StorageClient;

    using System;
    using System.Text.RegularExpressions;

    internal static class Utilities
    {
        internal static string GetMountedPathFromBlob(
            string localCachePath,
            string cloudDir,
            string containerName,
            string blobName,
            int driveSize,
            out CloudDrive mongoDrive)
        {
            DiagnosticsHelper.TraceInformation("Mounting cloud drive for dir \"{0}\" as container \"{1}\" with blob \"{2}\"",
                cloudDir,
                containerName,
                blobName);

            CloudStorageAccount storageAccount = CloudStorageAccount.FromConfigurationSetting(cloudDir);
            
            var blobClient = storageAccount.CreateCloudBlobClient();

            DiagnosticsHelper.TraceInformation("Get container");
            // this should be the name of your replset
            var driveContainer = blobClient.GetContainerReference(containerName);

            // create blob container (it has to exist before creating the cloud drive)
            try
            {
                driveContainer.CreateIfNotExist();
            }
            catch (StorageException e)
            {
                DiagnosticsHelper.TraceErrorException(
                    "Failed to create container",
                    e);
                throw;
            }

            var mongoBlobUri = blobClient.GetContainerReference(containerName).GetPageBlobReference(blobName).Uri.ToString();
            DiagnosticsHelper.TraceInformation("Blob uri obtained {0}", mongoBlobUri);

            // create the cloud drive
            mongoDrive = storageAccount.CreateCloudDrive(mongoBlobUri);

            try
            {
                mongoDrive.CreateIfNotExist(driveSize);
            }
            catch (StorageException e)
            {
                DiagnosticsHelper.TraceErrorException(
                    "Failed to create cloud drive",
                    e);
                throw;
            }

            DiagnosticsHelper.TraceInformation("Initialize cache");
            var localStorage = RoleEnvironment.GetLocalResource(localCachePath);

            CloudDrive.InitializeCache(localStorage.RootPath.TrimEnd('\\'),
                localStorage.MaximumSizeInMegabytes);

            // mount the drive and get the root path of the drive it's mounted as
            try
            {
                DiagnosticsHelper.TraceInformation("Acquiring write lock on azure drive");
                var driveLetter = mongoDrive.Mount(
                    localStorage.MaximumSizeInMegabytes,
                    DriveMountOptions.None);
                DiagnosticsHelper.TraceInformation("Write lock acquired on azure drive, mounted as \"{0}\"",
                    driveLetter);
                return driveLetter;
            }
            catch (CloudDriveException e)
            {
                DiagnosticsHelper.TraceErrorException(
                    "Failed to mount cloud drive",
                    e);
                throw;
            }
        }
    }
}
