using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Manager.Models;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Deployment = Util.IsRunningWebAppDirectly ? "Demo" : RoleEnvironment.DeploymentId;
            return View(ReplicaSetStatus.GetReplicaSetStatus());
        }
    }
}
