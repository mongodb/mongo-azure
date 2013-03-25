/*
 * Copyright 2010-2013 10gen Inc.
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

namespace MongoDB.WindowsAzure.MongoDBRole
{

    using System;

    using Microsoft.WindowsAzure.ServiceRuntime;
    
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.WindowsAzure.Common;

    internal static class DatabaseHelper
    {

        private static string currentRoleName = null;

        static DatabaseHelper()
        {
            currentRoleName = RoleEnvironment.CurrentRoleInstance.Role.Name;
        }

        internal static void RunInitializeCommandLocally(
            string rsName,
            int port)
        {
            var replicaSetRoleCount = RoleEnvironment.Roles[currentRoleName].Instances.Count;
            var membersDocument = new BsonArray();
            for (int i = 0; i < replicaSetRoleCount; i++)
            {
                EnsureMongodIsListening(rsName, i, port);
                membersDocument.Add(new BsonDocument {
                    {"_id", i},
                    {"host", RoleEnvironment.IsEmulated 
                        ? string.Format(Settings.LocalHostString, (port+i))
                        : ConnectionUtilities.GetNodeAlias(rsName, i)}
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
            var result = server["admin"].RunCommand(initCommand);

        }

        internal static bool IsReplicaSetInitialized(int port)
        {
            try
            {
                var server = GetLocalSlaveOkConnection(port);
                var result = server["admin"].RunCommand("replSetGetStatus");
                BsonValue startupStatus;
                result.Response.TryGetValue("startupStatus", out startupStatus);
                if (startupStatus != null)
                {
                    if (startupStatus == 3)
                    {
                        DiagnosticsHelper.TraceInformation(
                            "Status 3 in IsReplicaSetInitialized");
                        return false;
                    }
                }
                return true;
            }
            catch (MongoCommandException mce)
            {
                DiagnosticsHelper.TraceInformation(
                    string.Format("Exception in {0}IsReplicaSetInitialized",
                    mce.Message));
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

                server["admin"].RunCommand(stepDownCommand);
            }
        }

        internal static MongoServer GetLocalSlaveOkConnection(int port)
        {
            return GetSlaveOkConnection("localhost", port);
        }

        private static MongoServer GetSlaveOkConnection(string hostAlias, int port)
        {
            var connectionString = "mongodb://{0}:{1}/?slaveOk=true";
            var client = new MongoClient(string.Format(connectionString, hostAlias, port));
            var server = client.GetServer();
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
            var result = server["admin"].RunCommand(setLogLevelCommand);
        }

        internal static void EnsureMongodIsListening(string rsName, int instanceId, int mongodPort)
        {
            var alias = ConnectionUtilities.GetNodeAlias(rsName, instanceId);
            var commandSucceeded = false;
            while (!commandSucceeded)
            {
                try
                {
                    MongoServer conn;
                    if (RoleEnvironment.IsEmulated)
                    {
                        conn = DatabaseHelper.GetLocalSlaveOkConnection(mongodPort + instanceId);
                    }
                    else
                    {
                        conn = DatabaseHelper.GetSlaveOkConnection(alias, mongodPort);
                    }
                    conn.Connect(new TimeSpan(0, 0, 5));
                    commandSucceeded = true;
                }
                catch (MongoConnectionException mce)
                {
                    DiagnosticsHelper.TraceInformation(mce.Message);
                }
            }

        }

    }
}
