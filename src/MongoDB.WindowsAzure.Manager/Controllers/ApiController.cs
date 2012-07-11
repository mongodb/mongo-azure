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
    public class ApiController : Controller
    {
        // GET: /Orders/
        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Index()
        {
            string result = "list of orders";
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Fetches the instance log directly from the mongod server.
        /// </summary>
        public ActionResult GetServerLogDirect(int id)
        {
            var server = ServerStatus.Get(id);
            var mongo = MongoServer.Create("mongodb://" + server.Name + "/?slaveOk=true");
            try
            {
                var result = mongo["admin"]["$cmd"].FindOne(Query.EQ("getLog", "global"));
                return Json(new { log = Htmlize(result.AsBsonDocument["log"].AsBsonArray) }, JsonRequestBehavior.AllowGet);
            }
            catch (MongoException e)
            {
                return Json(new { log = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Fetches the instance log from the Azure WAD files in blob storage.
        /// </summary>
        public ActionResult GetServerLogBlob(int id)
        {
            try
            {
                var result = new { log = Htmlize(LogFetcher.TailLog(id)) };
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { log = "Error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string Htmlize(BsonArray logs)
        {
            StringBuilder str = new StringBuilder();
            foreach (var line in logs)
                str.Append("<div>" + line.AsString + "</div>");

            return str.ToString();
        }

        private string Htmlize(string log)
        {
            StringBuilder str = new StringBuilder();
            foreach (string line in log.Split('\n'))
                str.Append("<div>" + line + "</div>");

            return str.ToString();
        }
    }
}
