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
                        Id = 0,
                        Name = "localhost:27018",
                        Health = ServerStatus.HealthTypes.Up,
                        CurrentState = ServerStatus.State.Secondary,
                        LastHeartBeat = DateTime.Now.Subtract( new TimeSpan( 0, 0, 1 ) ),
                        OptimeDate = DateTime.Now,
                        PingTime = new Random( ).Next( 20, 600 )
                    },
                    new ServerStatus
                    {
                        Id = 1,
                        Name = "localhost:27019",
                        Health = ServerStatus.HealthTypes.Up,
                        CurrentState = ServerStatus.State.Primary,
                        LastHeartBeat = DateTime.MinValue,
                        OptimeDate = DateTime.Now,
                        PingTime = 0
                    },
                    new ServerStatus
                    {
                        Id = 2,
                        Name = "localhost:27020",
                        Health = ServerStatus.HealthTypes.Down,
                        CurrentState = ServerStatus.State.Down,
                        LastHeartBeat = DateTime.MinValue,
                        OptimeDate = DateTime.MinValue,
                        PingTime = 0,
                    } } )
            };
        }
    }
}