/*
 * Copyright 2010-2011 10gen Inc.
 * file : ReplicaSetHelper.cs
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

namespace MongoDB.Azure.ReplicaSets.ReplicaSetRole
{

    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Azure.ReplicaSets;

    using System;
    using System.Text;

    internal static class DatabaseHelper
    {

        private static int replicaSetRoleCount = 0;
        private static string currentRoleName = null;
        private static string nodeName = "#d{0}";
        private static string nodeAddress = "{0}:{1}";

        static DatabaseHelper()
        {
            // Note : This does not account for replica set member changes
            currentRoleName = RoleEnvironment.CurrentRoleInstance.Role.Name;
            replicaSetRoleCount = RoleEnvironment.Roles[currentRoleName].Instances.Count;
        }

        private static CommandResult ReplicaSetGetStatus(int port)
        {
            var server = GetLocalSlaveOkConnection(port);
            var result = server.RunAdminCommand("replSetGetStatus");
            return result;
        }

        internal static void RunInitializeCommandLocally(string rsName, int port)
        {
            var membersDocument = new BsonArray();
            for (int i = 0; i < replicaSetRoleCount; i++)
            {
                membersDocument.Add(new BsonDocument {
                    {"_id", i},
                    {"host", string.Format(nodeName, i)}
                });
            }
            var cfg = new BsonDocument {
                {"_id", rsName},
                {"members", membersDocument}
            };
            var initCommand = new CommandDocument {
                {"replSetInitiate", cfg}
            };
            var server = GetLocalSlaveOkConnection(port);
            try
            {
                var result = server.RunAdminCommand(initCommand);
            }
            catch
            {
                // TODO - need to do the right thing here
                // for now do nothing to assume init went through
            }
        }

        internal static void RunCloudCommandLocally(int myId, int port)
        {
            var nodeDocument = new BsonDocument();
            foreach (var instance in RoleEnvironment.Roles[currentRoleName].Instances)
            {
                var endpoint = instance.InstanceEndpoints[MongoDBAzureHelper.MongodPortSetting].IPEndpoint;
                int instanceId = ParseNodeInstanceId(instance.Id);
                nodeDocument.Add(
                        string.Format(nodeName, instanceId),
                        string.Format(nodeAddress, endpoint.Address,
                        RoleEnvironment.IsEmulated ? endpoint.Port + instanceId : endpoint.Port)
                );
            }

            var commandDocument = new BsonDocument {
                {"cloud", 1},
                {"nodes", nodeDocument},
                {"me", string.Format(nodeName, myId)}
            };

            var cloudCommand = new CommandDocument(commandDocument);

            var server = GetLocalSlaveOkConnection(port);
            var result = server.RunAdminCommand(cloudCommand);

        }

        internal static bool IsReplicaSetInitialized(int port)
        {
            try
            {
                var result = ReplicaSetGetStatus(port);
                BsonValue startupStatus;
                result.Response.TryGetValue("startupStatus", out startupStatus);
                if (startupStatus != null)
                {
                    if (startupStatus == 3)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        internal static void StepdownIfNeeded(int port)
        {
            var server = GetLocalSlaveOkConnection(port);
            if (server.State == MongoServerState.Disconnected)
            {
                server.Connect();
            }
            
            if (server.Instance.IsPrimary)
            {
                var stepDownCommand = new CommandDocument {
                    {"replSetStepDown", 1}
                };

                server.RunAdminCommand(stepDownCommand);
            }
        }

        internal static int ParseNodeInstanceId(string id)
        {
            int instanceId = int.Parse(id.Substring(id.LastIndexOf("_") + 1));
            return instanceId;
        }

        internal static MongoServer GetLocalSlaveOkConnection(int port)
        {
            var connectionString = "mongodb://localhost:{0}/?slaveOk=true";
            var server = MongoServer.Create(string.Format(connectionString, port));
            return server;
        }

        internal static void SetLogLevel(int port, string logLevelString)
        {
            int logLevel = logLevelString.Length;

            var commandDocument = new BsonDocument {
                {"setParameter", 1},
                {"logLevel", (logLevel-1)<6?(logLevel-1):5}
            };

            var setLogLevelCommand = new CommandDocument(commandDocument);
            var server = GetLocalSlaveOkConnection(port);
            var result = server.RunAdminCommand(setLogLevelCommand);
        }
    }
}
