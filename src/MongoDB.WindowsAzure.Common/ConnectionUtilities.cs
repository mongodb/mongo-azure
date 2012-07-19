/*
 * Copyright 2010-2012 10gen Inc.
 * file : ConnectionUtilities.cs
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
    using System.Collections.Generic;
    using MongoDB.Driver;
    using System.Collections.ObjectModel;
    using System.Net;

    /// <summary>
    /// Provides utility methods to easily connect to the MongoDB servers in your deployment.
    /// </summary>
    public static class ConnectionUtilities
    {
        /// <summary>
        /// Returns the connection settings to the MongoDB installation in the curent deployment.
        /// Use this to connect to MongoDB in your application!
        /// 
        /// You should cache these connection settings and re-obtain them only if there is a connection exception.
        /// </summary>
        /// <example>var server = MongoServer.Create(MongoDBAzureHelper.GetReplicaSetSettings());</example>
        /// <example>var setting = MongoDBAzureHelper.GetReplicaSetSettings();
        /// setting.SlaveOk = true;
        /// var server = MongoServer.Create(setting);</example>
        public static MongoServerSettings GetConnectionSettings( bool slaveOk = false )
        {
            return new MongoServerSettings
            {
                ReplicaSetName = GetReplicaSetName(),
                Servers = GetServerAddresses(),
                ConnectionMode = ConnectionMode.ReplicaSet,
                SlaveOk = slaveOk
            };
        }

        /// <summary>
        /// Returns the name of the blob container that holds the MongoDB data drives.
        /// </summary>
        /// <param name="replicaSetName">The name of the replica set used</param>
        public static string GetDataContainerName(string replicaSetName)
        {
            return string.Format(Constants.MongoDataContainerName, replicaSetName);
        }

        /// <summary>
        /// Returns a list of all MongoDB server addresses running in the current deployment.
        /// </summary>
        public static IList<MongoServerAddress> GetServerAddresses()
        {
            var servers = new List<MongoServerAddress>();

            foreach (var instance in GetDatabaseWorkerRoles())
            {
                servers.Add(GetServerAddress(instance));
            }

            return servers;
        }

        /// <summary>
        /// Determines the server address of the MongoDB instance running on the given instance.
        /// </summary>
        public static MongoServerAddress GetServerAddress(RoleInstance instance)
        {
            int instanceId = ParseNodeInstanceId(instance.Id);
            IPEndPoint endpoint = instance.InstanceEndpoints[Constants.MongodPortSetting].IPEndpoint;

            if (RoleEnvironment.IsEmulated)
            {
                // When running in the Azure emulator, the mongod instances are all running on localhost, with sequentially increasing port numbers.
                return new MongoServerAddress("localhost", endpoint.Port + instanceId);
            }
            else
            {
                return new MongoServerAddress(ConnectionUtilities.GetNodeAlias(ConnectionUtilities.GetReplicaSetName(), instanceId), endpoint.Port);
            }
        }

        /// <summary>
        /// Returns the set of all worker roles in the current deployment that are hosting MongoDB.
        /// Throws a ReplicaSetEnvironmentException if they could not be retrieved.
        /// </summary>
        public static ReadOnlyCollection<RoleInstance> GetDatabaseWorkerRoles()
        {
            try
            {
                return RoleEnvironment.Roles[Constants.MongoDBWorkerRoleName].Instances;
            }
            catch (KeyNotFoundException e)
            {
                throw new ReplicaSetEnvironmentException(string.Format("The MongoDB worker role should be called {0}", Constants.MongoDBWorkerRoleName), e);
            }
            catch (Exception e)
            {
                throw new ReplicaSetEnvironmentException("Exception when trying to obtain worker role instances", e);
            }
        }

        /// <summary>
        /// Returns the name of the MongoDB replica set in our current deployment.
        /// </summary>
        public static string GetReplicaSetName()
        {
            return RoleEnvironment.GetConfigurationSettingValue(Constants.ReplicaSetNameSetting);
        }

        /// <summary>
        /// Extracts the instance number from the instance's ID string.
        /// </summary>
        /// <param name="id">The instance's string ID (eg, deployment17(48).MongoDBReplicaSet.MongoDB.WindowsAzure.MongoDBRole_IN_2)</param>
        /// <returns>The instance numer (eg 2)</returns>
        public static int ParseNodeInstanceId(string id)
        {
            return int.Parse(id.Substring(id.LastIndexOf("_") + 1));
        }

        public static string GetNodeAlias(string replicaSetName, int instanceId)
        {
            var alias = string.Format("{0}_{1}", replicaSetName, instanceId);
            return alias;
        }

        /// <summary>
        /// Exception indicating configuration issues (such as that the worker role name have changed).
        /// </summary>
        public class ReplicaSetEnvironmentException : Exception
        {
            /// <summary>
            /// Creates a new instance of ReplicaSetEnvironmentException.
            /// </summary>
            /// <param name="message">User visible error message.</param>
            /// <param name="innerException">Inner exception that caused this.</param>
            public ReplicaSetEnvironmentException(string message, Exception innerException)
                : base(message, innerException) { }
        }
    }
}
