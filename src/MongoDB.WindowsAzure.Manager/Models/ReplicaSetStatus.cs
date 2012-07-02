using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.WindowsAzure.Common;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Globalization;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MongoDB.WindowsAzure.Manager.Models
{
    /// <summary>
    /// Stores the current replica set status as a model so it can easily be shown as a view.
    /// </summary>
    public class ReplicaSetStatus
    {
        public string name;
        public List<ServerStatus> servers = new List<ServerStatus>( );

        private ReplicaSetStatus( string name )
        {
            this.name = name;
        }

        public static ReplicaSetStatus GetReplicaSetStatus( )
        {
            if ( Util.IsRunningWebAppDirectly )
                return GetDummyStatus( );

            var connection = MongoServer.Create( ConnectionUtilities.GetConnectionSettings( true ) );
            try
            {
                return ParseStatus( connection["admin"].RunCommand( "replSetGetStatus" ).Response );
            }
            catch
            {
                return new ReplicaSetStatus( "Replica Set Unavailable" );
            }
        }

        private static ReplicaSetStatus ParseStatus( BsonDocument response )
        {
            // See if starting up...
            BsonValue startupStatus;
            if ( response.TryGetValue( "startupStatus", out startupStatus ) )
            {
                return new ReplicaSetStatus( "Replica Set Initializing" );
            }

            // Otherwise, extract the servers...
            return new ReplicaSetStatus( response.GetValue( "set" ).ToString( ) )
            {
                servers = ServerStatus.Parse( response.GetElement( "members" ).Value.AsBsonArray )
            };
        }

        /// <summary>
        /// Returns dummy server information for when the ASP.NET app is being run directly (without Azure).
        /// </summary>
        /// <returns></returns>
        public static ReplicaSetStatus GetDummyStatus( )
        {
            return new ReplicaSetStatus( "rs-offline-dummy-data" )
            {
                servers = new List<ServerStatus>( new ServerStatus[] {
                    new ServerStatus
                    {
                        id = 0,
                        name = "localhost:27018",
                        health = "UP",
                        state = 2,
                        lastHeartbeat = DateTime.Now.Subtract( new TimeSpan( 0, 0, 1 ) ).ToString( "yyyy-MM-dd HH:mm tt" ),
                        optimeDate = DateTime.Now,
                        pingMS = new Random( ).Next( 20, 600 ).ToString( ),
                        stateStr = "SECONDARY"
                    },
                    new ServerStatus
                    {
                        id = 1,
                        name = "localhost:27019",
                        health = "UP",
                        state = 1,
                        lastHeartbeat = "Not Applicable",
                        optimeDate = DateTime.Now,
                        pingMS = "Not Applicable",
                        stateStr = "PRIMARY"
                    },
                    new ServerStatus
                    {
                        id = 2,
                        name = "localhost:27020",
                        health = "DOWN",
                        state = 8,
                        lastHeartbeat = DateTime.MinValue.ToString( "yyyy-MM-dd HH:mm tt" ),
                        optimeDate = DateTime.MinValue,
                        pingMS = "0",
                        stateStr = "(not reachable/healthy)"
                    } } )
            };
        }
    }
}