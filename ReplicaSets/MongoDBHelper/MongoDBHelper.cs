/*
 * Copyright 2010-2011 10gen Inc.
 * file : MongoDBHelper.cs
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

namespace MongoDB.Azure.ReplicaSets.MongoDBHelper
{

    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.Driver;

    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;

    public static class MongoDBHelper
    {

        public const string MongodPortKey = "MongodPort";
        public const string MongoRoleName = "ReplicaSetRole";
        public const string ReplicaSetNameSetting = "ReplicaSetName";

        public static MongoServer GetLocalConnection(int port)
        {
            var connectionString = new StringBuilder();
            connectionString.Append("mongodb://");
            connectionString.Append(string.Format("localhost:{0}", port));
            var server = MongoServer.Create(connectionString.ToString());
            return server;
        }

        public static MongoServer GetLocalSlaveOkConnection(int port)
        {
            var connectionString = new StringBuilder();
            connectionString.Append("mongodb://");
            connectionString.Append(string.Format("localhost:{0}", port));
            connectionString.Append("/?slaveOk=true");
            var server = MongoServer.Create(connectionString.ToString());
            return server;
        }

        public static MongoUrlBuilder GetReplicaSetConnectionUri()
        {
            var connection = new MongoUrlBuilder();
            // TODO - Should only have 1 setting across both roles
            var replicaSetName = RoleEnvironment.GetConfigurationSettingValue(ReplicaSetNameSetting);
            connection.ReplicaSetName = replicaSetName;
            int replicaSetRoleCount = RoleEnvironment.Roles[MongoDBHelper.MongoRoleName].Instances.Count;
            var servers = new List<MongoServerAddress>();
            foreach (var instance in RoleEnvironment.Roles[MongoDBHelper.MongoRoleName].Instances)
            {
                var endpoint = instance.InstanceEndpoints[MongoDBHelper.MongodPortKey].IPEndpoint;
                int instanceId = ParseNodeInstanceId(instance.Id);
                var server = new MongoServerAddress(
                    endpoint.Address.ToString(),
                    (RoleEnvironment.IsEmulated ? endpoint.Port + instanceId : endpoint.Port)
                    );
                servers.Add(server);
            }
            connection.Servers = servers;
            return connection;
        }

        public static MongoServer GetReplicaSetConnection()
        {
            return MongoServer.Create(GetReplicaSetConnectionUri().ToServerSettings());
        }

        public static MongoUrlBuilder GetSlaveOkReplicaSetConnectionUri()
        {
            var url = GetReplicaSetConnectionUri();
            url.SlaveOk = true;
            return url;
        }

        public static MongoServer GetSlaveOkReplicaSetConnection()
        {
            return MongoServer.Create(GetSlaveOkReplicaSetConnectionUri().ToServerSettings());
        }

        public static int ParseNodeInstanceId(string id)
        {
            int instanceIndex = 0;
            int.TryParse(id.Substring(id.LastIndexOf("_") + 1), out instanceIndex);
            return instanceIndex;
        }

    }
}
