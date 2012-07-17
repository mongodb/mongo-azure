using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Manager.Src;
using System.Text;
using MongoDB.WindowsAzure.Manager.Models;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    /// <summary>
    /// Serves up API endpoints that power the asynchronous front-end.
    /// TODO: Fold this class into ServerController.
    /// </summary>
    public class ApiController : Controller
    {
        /// <summary>
        /// Fetches the instance log by connecting to its mongod server.
        /// This is fast and cheap, but won't work if the instance is down.
        /// </summary>
        public ActionResult GetServerLogDirect(int id)
        {
            var server = ServerStatus.Get(id);
            var mongo = MongoServer.Create(new MongoServerSettings { ConnectTimeout = new TimeSpan(0, 0, 3), Server = MongoServerAddress.Parse(server.Name), SlaveOk = true });
            try
            {
                var result = mongo["admin"]["$cmd"].FindOne(Query.EQ("getLog", "global"));
                return Json(new { log = HtmlizeFromLogArray(result.AsBsonDocument["log"].AsBsonArray) }, JsonRequestBehavior.AllowGet);
            }
            catch (MongoException e)
            {
                return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Turns an array of log entries into an HTML block.
        /// </summary>
        private string HtmlizeFromLogArray(BsonArray logs)
        {
            StringBuilder str = new StringBuilder();
            foreach (var line in logs)
                str.Append("<div class='logLine'>" + line.AsString + "</div>");

            return str.ToString();
        }
    }
}
