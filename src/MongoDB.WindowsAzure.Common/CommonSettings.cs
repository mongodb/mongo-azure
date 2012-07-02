/*
 * Copyright 2010-2012 10gen Inc.
 * file : CommonSettings.cs
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

namespace MongoDB.WindowsAzure.Common
{
    using System;
    using Microsoft.WindowsAzure.ServiceRuntime;

    /// <summary>
    /// Store constants that are used across multiple roles here.
    /// </summary>
    public static class CommonSettings
    {
        /// <summary>
        /// The name of the worker role that runs MongoDB. (This should be the name of the actual Azure Worker Role project in this solution.)
        /// </summary>
        public const string MongoDBWorkerRoleName = "MongoDB.WindowsAzure.MongoDBRole";

        /// <summary>
        /// The name of the container that will store the VHD images for the MongoDB data drives.
        /// The {0} parameter refers to the replica set name. (default "rs")
        /// </summary>
        public const string MongoDataContainerName = "mongoddatadrive{0}";

        //=============================================================================================
        //
        // CONFIGURATION SETTING NAMES
        //
        // The following are the names of settings that are defined for each role.
        // These settings are then configured by the user in the CloudDeploy project before deploying.
        // If you rename those settings, don't forget to update the references here!
        //
        //=============================================================================================

        /// <summary>
        /// The name of the setting that specifies the MongoDB host port.
        /// </summary>
        public const string MongodPortSetting = "MongodPort";

        /// <summary>
        /// The name of the setting that specifies the MongoDB replica set name.
        /// </summary>
        public const string ReplicaSetNameSetting = "ReplicaSetName";

        /// <summary>
        /// The name of the setting that specifies the connection string for the cloud storage that MongoDB uses for its data.
        /// </summary>
        public const string MongoDataCredentialSetting = "MongoDBDataDir";

        /// <summary>
        /// The name of the local storage used for BlobBackup's drive mounting.
        /// </summary>
        public const string BackupLocalStorageName = "BackupDriveCache";

        public static string GetReplicaSetName( )
        {
            return RoleEnvironment.GetConfigurationSettingValue( ReplicaSetNameSetting );
        }
    }
}
