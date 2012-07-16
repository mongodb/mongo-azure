using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using MongoDB.WindowsAzure.Tools;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class BackupController : Controller
    {
        //=========================================================================
        //
        //  AJAX ACTIONS
        //
        //=========================================================================

        public JsonResult Create(string uri)
        {
            var job = new BackupJob(new Uri(uri), "DefaultEndpointsProtocol=http;AccountName=managerstorage3;AccountKey=OMtTMtI5AtLLK8fBrDAUxJBqo9js+4jcd10SmKV2hiZwsUfPJVu5neaAM3OV2d5hgWZeZyaiqM6SP03pzvI7hw==");
            job.Start();
            return Json(new { success = true, jobId = job.Id });
        }

        public JsonResult List()
        {
            var backups = BackupManager.GetBackups("DefaultEndpointsProtocol=http;AccountName=managerstorage3;AccountKey=OMtTMtI5AtLLK8fBrDAUxJBqo9js+4jcd10SmKV2hiZwsUfPJVu5neaAM3OV2d5hgWZeZyaiqM6SP03pzvI7hw==");

            var pairs = backups.Select(blob => new { dateString = SnapshotController.ToString(blob.Attributes.Snapshot), name = blob.Name });
            return Json(new { snapshots = pairs }, JsonRequestBehavior.AllowGet);
        }
    }
}
