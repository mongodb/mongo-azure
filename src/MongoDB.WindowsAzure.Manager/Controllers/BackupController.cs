using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading;
using MongoDB.WindowsAzure.Tools;
using System.Collections;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    /// <summary>
    /// Manages backups and backup jobs.
    /// </summary>
    public class BackupController : Controller
    {
        //=========================================================================
        //
        //  REGULAR ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Shows details about the job with the given ID.
        /// </summary>
        public ActionResult ShowJob(int id)
        {
            BackupJob job;
            lock (BackupJobs.Jobs)
            {
                job = BackupJobs.Jobs[id];
            }

            return View(job);
        }

        //=========================================================================
        //
        //  AJAX ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Starts a backup job on the contents of the blob with the given URI.
        /// </summary>
        public JsonResult Start(string uri)
        {
            var job = new BackupJob(new Uri(uri), "DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==");
            lock (BackupJobs.Jobs)
            {
                BackupJobs.Jobs.Add(job.Id, job);
            }
            job.Start();
            return Json(new { success = true, jobId = job.Id });
        }

        /// <summary>
        /// Returns all the completed backups.
        /// </summary>
        public JsonResult ListCompleted()
        {
            var backups = BackupManager.GetBackups("DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==");

            var data = backups.Select(blob => new { name = blob.Name, uri = blob.Uri }); // Extract certain properties.
            return Json(new { backups = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all the in-progress backup jobs.
        /// </summary>
        public JsonResult ListJobs()
        {
            IEnumerable data;
            lock (BackupJobs.Jobs)
            {
                data = BackupJobs.Jobs.Values.Select(job => job.ToJson()); // Extract certain properties.
            }
            return Json(new { jobs = data }, JsonRequestBehavior.AllowGet);
        }
    }

    /// <summary>
    /// Wraps the static jobs collection so it is persistant across sessions.
    /// See http://stackoverflow.com/questions/8919095/lifetime-of-asp-net-static-variable
    /// </summary>
    static class BackupJobs
    {
        public static Dictionary<int, BackupJob> Jobs = new Dictionary<int, BackupJob>();
    }
}
