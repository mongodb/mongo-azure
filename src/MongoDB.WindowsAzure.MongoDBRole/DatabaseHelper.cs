/*
 * Copyright 2010-2012 10gen Inc.
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

    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.WindowsAzure.Common;

    using System;
    using System.Text;

    internal static class DatabaseHelper
    {
        private static string currentRoleName = null;

        static DatabaseHelper()
        {
            currentRoleName = RoleEnvironment.CurrentRoleInstance.Role.Name;
        }

        private static CommandResult ReplicaSetGetStatus(int port)
        {
            try
            {
                var server = GetLocalSlaveOkConnection(port);
                return server["admin"].RunCommand("replSetGetStatus");
            }
            catch (MongoCommandException mongoCommandException)
            {
                return mongoCommandException.CommandResult;
            }
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
            var result = ReplicaSetGetStatus(port);
            if (!result.Ok)
            {
                return false;
            }

            BsonValue startupStatus;
            if (result.Response.TryGetValue("startupStatus", out startupStatus))
            {
                if (startupStatus == 3)
                {
                    return false;
                }
            }
            return true;
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

        internal static MongoServer GetSlaveOkConnection(string hostAlias, int port)
        {
            var connectionString = "mongodb://{0}:{1}/?slaveOk=true";
            var server = MongoServer.Create(string.Format(connectionString, hostAlias, port));
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
            for (;;)
            {
                // TODO: Prevent denial-of-service against the the local
                // machine if GetConnection keeps failing.
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
                    break;
                }
                catch (MongoConnectionException e)
                {
                    DiagnosticsHelper.TraceInformation(e.Message);
                }
            }
        }

    }
}
