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
    public class BackupController : Controller
    {
        private static Dictionary<int, BackupJob> Jobs = new Dictionary<int, BackupJob>();

        //=========================================================================
        //
        //  REGULAR ACTIONS
        //
        //=========================================================================

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowJob(int id)
        {
            BackupJob job;
            lock (Jobs)
            {
                job = Jobs[id];
            }

            return View(job);
        }

        //=========================================================================
        //
        //  AJAX ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Starts a backup job on the contents of the VHD with the given URI.
        /// </summary>
        public JsonResult Start(string uri)
        {
            var job = new BackupJob(new Uri(uri), "DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==");
            lock (Jobs)
            {
                Jobs.Add(job.Id, job);
            }
            job.Start();
            return Json(new { success = true, jobId = job.Id });
        }

        /// <summary>
        /// Returns all the completed backups that are stored as TARS.
        /// </summary>
        public JsonResult ListCompleted()
        {
            var backups = BackupManager.GetBackups("DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==");

            var data = backups.Select(blob => new { name = blob.Name, uri = blob.Uri }); // Extract certain properties.
            return Json(new { backups = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all the completed backups that are stored as TARS.
        /// </summary>
        public JsonResult ListJobs()
        {
            IEnumerable data;
            lock (Jobs)
            {
                data = Jobs.Values.Select(job => job.ToJson()); // Extract certain properties.
            }
            return Json(new { jobs = data }, JsonRequestBehavior.AllowGet);
        }
    }
}
