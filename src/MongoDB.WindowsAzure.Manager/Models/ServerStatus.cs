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
        public Int32 id { get; set; }
        public string name { get; set; }
        public string health { get; set; }
        public Int32 state { get; set; }
        public string stateStr { get; set; }
        public string lastHeartbeat { get; set; }
        public DateTime optimeDate { get; set; }
        public string pingMS { get; set; }

        public static ServerStatus Parse( IEnumerable<BsonElement> properties )
        {
            var server = new ServerStatus();
            foreach ( BsonElement bsonElement in properties )
            {
                switch ( bsonElement.Name )
                {
                    case "_id":
                        server.id = bsonElement.Value.ToInt32( );
                        break;
                    case "name":
                        server.name = bsonElement.Value.ToString( );
                        break;
                    case "health":
                        server.health = bsonElement.Value.ToInt32( ) == 0 ? "DOWN" : "UP";
                        break;
                    case "state":
                        server.state = bsonElement.Value.ToInt32( );
                        if ( server.state == 1 )
                        {
                            server.lastHeartbeat = "Not Applicable";
                            server.pingMS = "Not Applicable";
                        }
                        break;
                    case "stateStr":
                        server.stateStr = bsonElement.Value.ToString( );
                        break;
                    case "uptime":
                        break;
                    case "lastHeartbeat":
                        var hearbeat = bsonElement.Value.AsDateTime;
                        if ( hearbeat != null )
                        {
                            server.lastHeartbeat = hearbeat.ToString( "yyyy-MM-dd HH:mm tt" );
                        }
                        break;
                    case "optimeDate":
                        server.optimeDate = bsonElement.Value.AsDateTime;
                        break;
                    case "pingMs":
                        Double pingTime = bsonElement.Value.AsInt32;
                        server.pingMS = pingTime.ToString( CultureInfo.InvariantCulture );
                        break;
                }
            }

            return server;
        }
        public static List<ServerStatus> Parse( BsonArray documents )
        {
            List<ServerStatus> servers = new List<ServerStatus>( );

            foreach ( BsonDocument member in documents )
            {
                servers.Add( ServerStatus.Parse( member.Elements ) );
            }

            return servers;
        }
    }
}