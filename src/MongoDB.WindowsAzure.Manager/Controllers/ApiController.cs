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
            var mongo = MongoServer.Create("mongodb://" + server.Name + "/?slaveOk=true");
            try
            {
                var result = mongo["admin"]["$cmd"].FindOne(Query.EQ("getLog", "global"));
                return Json(new { log = HtmlizeFromLogArray(result.AsBsonDocument["log"].AsBsonArray) }, JsonRequestBehavior.AllowGet);
            }
            catch (MongoException e)
            {
                return Json(new { log = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fetches the instance log from the Windows Azure Diagnostics (WAD) files in blob storage.
        /// This is slow, expensive, and the logs are out-of-date by up to a minute -- but reliable.
        /// </summary>
        public ActionResult GetServerLogBlob(int id)
        {
            try
            {
                var result = new { log = HtmlizeFromLogFile(LogFetcher.TailLog(id)) };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { log = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Turns an array of log entries into an HTML block.
        /// </summary>
        private string HtmlizeFromLogArray(BsonArray logs)
        {
            StringBuilder str = new StringBuilder();
            foreach (var line in logs)
                str.Append("<div>" + line.AsString + "</div>");

            return str.ToString();
        }

        /// <summary>
        /// Turns a flat log file into an HTML  block.
        /// </summary>
        private string HtmlizeFromLogFile(string log)
        {
            StringBuilder str = new StringBuilder();
            foreach (string line in log.Split('\n'))
                str.Append("<div>" + line + "</div>");

            return str.ToString();
        }
    }
}
