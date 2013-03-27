/*
 * Copyright 2010-2013 10gen Inc.
 * file : ServerStatus.cs
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

    using MongoDB.Bson;

    /// <summary>
    /// Represents the status of one MongoDB server.
    /// </summary>
    public class ServerStatus
    {
        public enum HealthTypes
        {
            Down = 0,
            Up = 1
        }

        public enum State
        {
            StartingUp = 0, // Parsing configuration
            Primary = 1,
            Secondary = 2,
            Recovering = 3, // Initial syncing, post-rollback, stale members
            FatalError = 4,
            StartingUpPhase2 = 5, // Forking threads
            Unknown = 6, // Member has never been reached
            Arbiter = 7,
            Down = 8,
            Rollback = 9,
            Removed = 10
        }

        /// <summary>
        /// Returns all the servers that we're aware of in the current replica
        /// set.
        /// </summary>
        public static List<ServerStatus> List
        {
            get
            {
                return ReplicaSetStatus.GetStatus().Servers;
            }
        }

        /// <summary>
        /// Returns the server that is currently the primary.
        /// </summary>
        public static ServerStatus Primary
        {
            get
            {
                return List.Find(delegate(ServerStatus s) 
                { 
                    return (s.CurrentState == State.Primary); 
                });
            }
        }

        /// <summary>
        /// The node's ID in the replica set.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The node's name (often its address, e.g "localhost:27017").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Is the node up or down?
        /// </summary>
        public HealthTypes Health { get; set; }

        /// <summary>
        /// The current state of the node.
        /// </summary>
        public State CurrentState { get; set; }

        /// <summary>
        /// The last time a heartbeat from this node was received by the primary.
        /// </summary>
        public DateTime LastHeartBeat { get; set; }

        /// <summary>
        /// The last time a database operation ran on this node.
        /// </summary>
        public DateTime LastOperationTime { get; set; }

        /// <summary>
        /// The round-trip ping time to the primary, in MS.
        /// </summary>
        public int PingTime { get; set; }

        /// <summary>
        /// Parses this server status from one document in the "members" set
        /// of a replSetGetStatus call.
        /// </summary>
        public static ServerStatus Parse(BsonDocument document)
        {
            var status = new ServerStatus
            {
                Id = document["_id"].AsInt32,
                Name = document["name"].AsString,
                Health = (HealthTypes) document["health"].AsDouble,
                CurrentState = (State) document["state"].AsInt32,
                LastHeartBeat = document.Contains("lastHeartbeat") ? 
                    document["lastHeartbeat"].ToUniversalTime() : 
                    DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc),
                LastOperationTime = document["optimeDate"].ToUniversalTime(),
                PingTime = document.Contains("pingMs") ? 
                    (int) document["pingMs"].AsInt32 : 0
            };

            status.Repair();
            return status;
        }

        /// <summary>
        /// Returns the server with the given ID.
        /// </summary>
        public static ServerStatus Get(int id)
        {
            return List.Find(delegate(ServerStatus s) { return (s.Id == id); });
        }

        /// <summary>
        /// Parses the "members" set of a replSetGetStatus call.
        /// </summary>        
        public static List<ServerStatus> Parse(BsonArray documents)
        {
            List<ServerStatus> servers = new List<ServerStatus>();

            foreach (BsonDocument member in documents)
            {
                servers.Add(ServerStatus.Parse(member));
            }

            return servers;
        }

        /// <summary>
        /// Corrects any conflicting or redundant data in the server's status.
        /// </summary>
        private void Repair()
        {
            // mongod returns the Unix epoch for down instances -- convert
            // these to DateTime.MinValue, the .NET epoch.
            LastHeartBeat = (LastHeartBeat == BsonConstants.UnixEpoch) ? 
                DateTime.MinValue : LastHeartBeat;
            LastOperationTime = (LastOperationTime == 
                BsonConstants.UnixEpoch) ? 
                DateTime.MinValue : LastOperationTime;
        }
    }
}