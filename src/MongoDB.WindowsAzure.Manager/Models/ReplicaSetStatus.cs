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

            ReplicaSetStatus status;
            var settings = ConnectionUtilities.GetConnectionSettings( );
            settings.SlaveOk = true;
            var server = MongoServer.Create( settings );
            try
            {
                var result = server["admin"].RunCommand( "replSetGetStatus" );
                var response = result.Response;

                BsonValue startupStatus;
                if ( response.TryGetValue( "startupStatus", out startupStatus ) )
                {
                    status = new ReplicaSetStatus( "Replica Set Initializing" );
                    return status;
                }

                status = new ReplicaSetStatus( (string) response.GetValue( "set" ) );
                var value = response.GetElement( "members" );
                var members = value.Value.AsBsonArray;
                foreach ( BsonDocument member in members )
                {
                    var node = new ServerStatus( );
                    foreach ( BsonElement bsonElement in member.Elements )
                    {
                        switch ( bsonElement.Name )
                        {
                            case "_id":
                                node.id = bsonElement.Value.ToInt32( );
                                break;
                            case "name":
                                node.name = bsonElement.Value.ToString( );
                                break;
                            case "health":
                                node.health = bsonElement.Value.ToInt32( ) == 0 ? "DOWN" : "UP";
                                break;
                            case "state":
                                node.state = bsonElement.Value.ToInt32( );
                                if ( node.state == 1 )
                                {
                                    node.lastHeartbeat = "Not Applicable";
                                    node.pingMS = "Not Applicable";
                                }
                                break;
                            case "stateStr":
                                node.stateStr = bsonElement.Value.ToString( );
                                break;
                            case "uptime":
                                break;
                            case "lastHeartbeat":
                                var hearbeat = bsonElement.Value.AsDateTime;
                                if ( hearbeat != null )
                                {
                                    node.lastHeartbeat = hearbeat.ToString( "yyyy-MM-dd HH:mm tt" );
                                }
                                break;
                            case "optimeDate":
                                node.optimeDate = bsonElement.Value.AsDateTime;
                                break;
                            case "pingMs":
                                Double pingTime = bsonElement.Value.AsInt32;
                                node.pingMS = pingTime.ToString( CultureInfo.InvariantCulture );
                                break;
                        }
                    }
                    status.servers.Add( node );
                }
            }
            catch
            {
                status = new ReplicaSetStatus( "Replica Set Unavailable" );
            }
            return status;
        }

        /// <summary>
        /// Returns dummy server information for when the ASP.NET app is being run directly (without Azure).
        /// </summary>
        /// <returns></returns>
        public static ReplicaSetStatus GetDummyStatus( )
        {
            return new ReplicaSetStatus( "rs-offline-dummy-data" )
            {
                servers = new List<ServerStatus>( new ServerStatus[] { new ServerStatus
                {
                    id = 0,
                    name = "localhost:27018",
                    health = "UP",
                    state = 2,
                    lastHeartbeat = DateTime.Now.Subtract( new TimeSpan( 0, 0, 1 ) ).ToString( "yyyy-MM-dd HH:mm tt" ),
                    optimeDate = DateTime.Now,
                    pingMS = new Random( ).Next( 20, 600 ).ToString( ),
                    stateStr = "SECONDARY"
                }, new ServerStatus
                {
                    id = 1,
                    name = "localhost:27019",
                    health = "UP",
                    state = 1,
                    lastHeartbeat = "Not Applicable",
                    optimeDate = DateTime.Now,
                    pingMS = "Not Applicable",
                    stateStr = "PRIMARY"
                }, new ServerStatus
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