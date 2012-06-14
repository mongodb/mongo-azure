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

namespace MongoDB.Azure.ReplicaSets.ReplicaSetRole
{

    using Microsoft.WindowsAzure.ServiceRuntime;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Azure.Common;

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
            var server = GetLocalSlaveOkConnection(port);
            var result = server.RunAdminCommand("replSetGetStatus");
            return result;
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
                        : CommonUtilities.GetNodeAlias(rsName, i)}
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
            var result = server.RunAdminCommand(initCommand);

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
            var result = server.RunAdminCommand(setLogLevelCommand);
        }

        internal static void EnsureMongodIsListening(string rsName, int instanceId, int mongodPort)
        {
            var alias = CommonUtilities.GetNodeAlias(rsName, instanceId);
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
                catch (Exception e)
                {
                    DiagnosticsHelper.TraceInformation(e.Message);
                    commandSucceeded = false;
                }
            }

        }

    }
}
