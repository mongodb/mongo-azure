using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
            var result = new { num = id };
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
