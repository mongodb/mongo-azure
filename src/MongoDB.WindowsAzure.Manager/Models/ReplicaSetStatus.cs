/*
 * Copyright 2010-2013 10gen Inc.
 * file : ReplicaSetStatus.cs
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

namespace MongoDB.WindowsAzure.Manager.Models
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Microsoft.WindowsAzure.ServiceRuntime;
    
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.WindowsAzure.Common;

    /// <summary>
    /// Stores the status of the replica set.
    /// </summary>
    public class ReplicaSetStatus
    {
        public enum State
        {
            Initializing,
            OK,
            Error
        }

        /// <summary>
        /// The state of the replica set.
        /// </summary>
        public State Status { get; private set; }

        /// <summary>
        /// The name of the replica set, if Status is OK.
        /// </summary>
        public string ReplicaSetName { get; private set; }

        /// <summary>
        /// The error we received while fetching the status, if Status is Error.
        /// </summary>
        public Exception Error { get; private set; }

        /// <summary>
        /// The actual servers in the replica set.
        /// </summary>
        public List<ServerStatus> Servers { get; set; }

        private ReplicaSetStatus()
        {
            Status = State.Initializing;
            Servers = new List<ServerStatus>();
        }

        /// <summary>
        /// Fetches the current status.
        /// </summary>
        public static ReplicaSetStatus GetStatus()
        {
            if (!RoleEnvironment.IsAvailable)
            {
                return GetDummyStatus();
            }
            var settings = ConnectionUtilities.GetMongoClientSettings(
                ReadPreference.SecondaryPreferred);
            var client = new MongoClient(settings);
            var server = client.GetServer();
            try
            {
                var result = server.GetDatabase("admin").RunCommand("replSetGetStatus");
                return ParseStatus(result.Response);
            }
            catch (IOException ie)
            {
                return new ReplicaSetStatus { Status = State.Error, Error = ie };
            }
            catch (MongoException e)
            {
                return new ReplicaSetStatus { Status = State.Error, Error = e };
            }
        }

        /// <summary>
        /// Parses and returns the status from a replSetGetStatus result.
        /// </summary>
        private static ReplicaSetStatus ParseStatus(BsonDocument response)
        {
            // See if starting up...
            BsonValue startupStatus;
            if (response.TryGetValue("startupStatus", out startupStatus))
            {
                return new ReplicaSetStatus { Status = State.Initializing };
            }

            // Otherwise, extract the servers...
            return new ReplicaSetStatus
            {
                Status = State.OK,
                ReplicaSetName = response.GetValue("set").AsString,
                Servers = ServerStatus.Parse(response.GetElement("members").Value.AsBsonArray)
            };
        }

        /// <summary>
        /// Returns dummy server information for when the ASP.NET app is being run directly (without Azure).
        /// </summary>
        /// <returns></returns>
        public static ReplicaSetStatus GetDummyStatus()
        {
            return new ReplicaSetStatus
            {
                Status = State.OK,
                ReplicaSetName = "rs-offline-dummy-data",
                Servers = new List<ServerStatus>(new ServerStatus[] {
                    new ServerStatus
                    {
                        Id = 0,
                        Name = "localhost:27018",
                        Health = ServerStatus.HealthTypes.Up,
                        CurrentState = ServerStatus.State.Secondary,
                        LastHeartBeat = DateTime.Now.Subtract(new TimeSpan(0, 0, 1)),
                        LastOperationTime = DateTime.Now,
                        PingTime = new Random().Next(20, 600)
                    },
                    new ServerStatus
                    {
                        Id = 1,
                        Name = "localhost:27019",
                        Health = ServerStatus.HealthTypes.Up,
                        CurrentState = ServerStatus.State.Primary,
                        LastHeartBeat = DateTime.MinValue,
                        LastOperationTime = DateTime.Now,
                        PingTime = 0
                    },
                    new ServerStatus
                    {
                        Id = 2,
                        Name = "localhost:27020",
                        Health = ServerStatus.HealthTypes.Down,
                        CurrentState = ServerStatus.State.Down,
                        LastHeartBeat = DateTime.MinValue,
                        LastOperationTime = DateTime.MinValue,
                        PingTime = 0,
                    } })
            };
        }
    }
}