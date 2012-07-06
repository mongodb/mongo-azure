using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Manager.Models;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View(ReplicaSetStatus.GetReplicaSetStatus());
        }
    }
}
