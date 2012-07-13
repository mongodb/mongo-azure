using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Tools;
using MongoDB.WindowsAzure.Manager.Models;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class BackupController : Controller
    {
        public ActionResult NewSnapshot()
        {
            // Snapshot the primary.
            var uri = SnapshotManager.MakeSnapshot(ServerStatus.Primary.Id);
            TempData["flashSuccess"] = "Snapshot created: " + uri;
            return RedirectToAction("Index", "Dashboard");
        }

    }
}
