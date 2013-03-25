/*
 * Copyright 2010-2013 10gen Inc.
 * file : RoleSettings.cs
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
    /// A shorthand class to fetch role environmental variables.
    /// </summary>
    public class RoleSettings
    {
        /// <summary>
        /// The credentials provided to access the MongoDB storage account.
        /// </summary>
        public static string StorageCredentials
        {
            get
            {
                return RoleEnvironment.GetConfigurationSettingValue(
                    Constants.MongoDataCredentialSetting);
            }
        }

        /// <summary>
        /// The name of the replica set MongoDB is configured to use. Defaults to "rs".
        /// </summary>
        public static string ReplicaSetName
        {
            get
            {
                return RoleEnvironment.GetConfigurationSettingValue(
                    Constants.ReplicaSetNameSetting);
            }
        }
    }
}
