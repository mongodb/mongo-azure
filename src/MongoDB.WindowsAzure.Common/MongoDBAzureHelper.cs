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

namespace MongoDB.WindowsAzure.Common
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using MongoDB.Driver;
    using System.Net;

    /// <summary>
    /// Helper class used to obtain connection settings to connect to the replica set worker role instances.
    /// </summary>
    public static class MongoDBAzureHelper
    {
        /// <summary>
        /// Returns the connection settings that can be used to connect to the MongoDB installation in the curent deployment.
        /// Use this to connect to MongoDB in your application.
        /// 
        /// You should cache these connection settings and re-obtain them only if there is a connection exception.
        /// </summary>
        /// <example>var server = MongoServer.Create(MongoDBAzureHelper.GetReplicaSetSettings());</example>
        /// <example>var setting = MongoDBAzureHelper.GetReplicaSetSettings();
        /// setting.SlaveOk = true;
        /// var server = MongoServer.Create(setting);</example>
        public static MongoServerSettings GetReplicaSetSettings( )
        {
            return new MongoServerSettings
            {
                ReplicaSetName = CommonSettings.GetReplicaSetName( ),
                Servers = GetServerAddresses( ),
                ConnectionMode = ConnectionMode.ReplicaSet
            };
        }

        /// <summary>
        /// Returns a list of all MongoDB server addresses running in the current deployment.
        /// </summary>
        public static IList<MongoServerAddress> GetServerAddresses( )
        {
            var servers = new List<MongoServerAddress>( );

            foreach ( var instance in GetDatabaseWorkerRoles( ) )
            {
                servers.Add( GetServerAddress( instance ) );
            }

            return servers;
        }

        /// <summary>
        /// Determines the server address of the MongoDB instance running on the given instance.
        /// </summary>
        public static MongoServerAddress GetServerAddress( RoleInstance instance )
        {
            int instanceId = CommonUtilities.ParseNodeInstanceId( instance.Id );
            IPEndPoint endpoint = instance.InstanceEndpoints[CommonSettings.MongodPortSetting].IPEndpoint;

            if ( RoleEnvironment.IsEmulated )
            {
                // When running in the Azure emulator, the mongod instances are all running on localhost, with sequentially increasing port numbers.
                return new MongoServerAddress( "localhost", endpoint.Port + instanceId );
            }
            else
            {
                return new MongoServerAddress( CommonUtilities.GetNodeAlias( CommonSettings.GetReplicaSetName( ), instanceId ), endpoint.Port );
            }
        }

        /// <summary>
        /// Returns the set of all worker roles in the current deployment that are hosting MongoDB.
        /// Throws a ReplicaSetEnvironmentException if they could not be retrieved.
        /// </summary>
        public static ReadOnlyCollection<RoleInstance> GetDatabaseWorkerRoles( )
        {
            try
            {
                return RoleEnvironment.Roles[CommonSettings.MongoDBWorkerRoleName].Instances;
            }
            catch ( KeyNotFoundException e )
            {
                throw new ReplicaSetEnvironmentException( string.Format( "The MongoDB worker role should be called {0}", CommonSettings.MongoDBWorkerRoleName ), e );
            }
            catch ( Exception e )
            {
                throw new ReplicaSetEnvironmentException( "Exception when trying to obtain worker role instances", e );
            }
        }
    }
}
