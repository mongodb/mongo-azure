using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}