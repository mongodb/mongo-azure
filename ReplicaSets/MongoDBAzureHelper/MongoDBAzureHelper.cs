/*
 * Copyright 2010-2012 10gen Inc.
 * file : MongoDBAzureHelper.cs
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

namespace MongoDB.Azure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.Driver;

    /// <summary>
    /// Helper class used to obtain connection settings to connect to the replica set worker role instances.
    /// </summary>
    public static class MongoDBAzureHelper
    {

        /// <summary>
        /// Get a MongoServerSettings object that can then be used to create a connection to the
        /// MongoDB Replica Set. Note this assumes the name of the replica ser role is ReplicaSetRole.
        /// The connection settings should be cached on the client side and should only be obtained
        /// if there is a connect exception.
        /// </summary>
        /// <returns>A MongoServerSettings object that corresponds to the replica set instances</returns>
        /// <example>var server = MongoServer.Create(MongoDBAzureHelper.GetReplicaSetSettings());</example>
        /// <example>var setting = MongoDBAzureHelper.GetReplicaSetSettings();
        /// setting.SlaveOk = true;
        /// var server = MongoServer.Create(setting);</example>
        public static MongoServerSettings GetReplicaSetSettings()
        {
            var settings = new MongoServerSettings();
            // TODO - Should only have 1 setting across both roles
            var replicaSetName = RoleEnvironment.GetConfigurationSettingValue(CommonSettings.ReplicaSetNameSetting);
            settings.ReplicaSetName = replicaSetName;
            
            ReadOnlyCollection<RoleInstance> workerRoleInstances = null;
            try
            {
                workerRoleInstances = RoleEnvironment.Roles[CommonSettings.MongoDBWorkerRoleName].Instances;
            }
            catch (KeyNotFoundException ke)
            {
                throw new ReplicaSetEnvironmentException(
                    string.Format("The MongoDB worker role should be called {0}", CommonSettings.MongoDBWorkerRoleName), 
                    ke);
            }
            catch (Exception e)
            {
                throw new ReplicaSetEnvironmentException(
                    "Exception when trying to obtain worker role instances",
                    e);
            }

            int replicaSetRoleCount = workerRoleInstances.Count;
            var servers = new List<MongoServerAddress>();
            foreach (var instance in workerRoleInstances)
            {
                var endpoint = instance.InstanceEndpoints[CommonSettings.MongodPortSetting].IPEndpoint;
                int instanceId = CommonUtilities.ParseNodeInstanceId(instance.Id);
                MongoServerAddress server = null;
                if (RoleEnvironment.IsEmulated)
                {
                    server = new MongoServerAddress("localhost", endpoint.Port + instanceId);
                }
                else
                {
                    server = new MongoServerAddress(
                        CommonUtilities.GetNodeAlias(replicaSetName, instanceId),
                        endpoint.Port
                        );
                }

                servers.Add(server);
            }
            settings.Servers = servers;
            settings.ConnectionMode = ConnectionMode.ReplicaSet;
            return settings;
        }
    }
}
