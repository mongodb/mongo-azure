using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Bson;
using System.Globalization;

namespace MongoDB.WindowsAzure.Manager.Models
{
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
        /// Time of the last succeeded heartbeat.
        /// </summary>
        public DateTime? LastHeartBeat { get; set; }

        /// <summary>
        /// The time of the last operation run on this node.
        /// </summary>
        public DateTime OptimeDate { get; set; }

        /// <summary>
        /// The round-trip ping time to the primary, in MS.
        /// </summary>
        public int PingTime { get; set; }

        /// <summary>
        /// Parses this server status from one document in the "members" set of a replSetGetStatus call.
        /// </summary>
        public static ServerStatus Parse( BsonDocument document )
        {
            return new ServerStatus
            {
                Id = document["_id"].AsInt32,
                Name = document["name"].AsString,
                Health = (HealthTypes) document["health"].AsDouble,
                CurrentState = (State) document["state"].AsInt32,
                LastHeartBeat = document.Contains( "lastHeartBeat" ) ? document["lastHeartBeat"].AsDateTime : (DateTime?) null,
                OptimeDate = document["optimeDate"].AsDateTime,
                PingTime = document.Contains( "pingMs" ) ? (int) document["pingMs"].AsInt32 : 0
            };
        }

        /// <summary>
        /// Parses the "members" set of a replSetGetStatus call.
        /// </summary>        
        public static List<ServerStatus> Parse( BsonArray documents )
        {
            List<ServerStatus> servers = new List<ServerStatus>( );

            foreach ( BsonDocument member in documents )
            {
                servers.Add( ServerStatus.Parse( member ) );
            }

            return servers;
        }
    }
}