using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Manager.Src;
using System.Text;

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

        //
        // GET: /Orders/{orderID}
        public ActionResult GetServerLog(int id)
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

        private string Htmlize(string log)
        {
            StringBuilder str = new StringBuilder();
            foreach (string line in log.Split('\n'))
                str.Append("<div>" + line + "</div>");

            return str.ToString();
        }
    }
}
