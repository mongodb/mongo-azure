/*
 * Copyright 2010-2013 10gen Inc.
 * file : BackupController.cs
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using MongoDB.WindowsAzure.Backup;
    using MongoDB.WindowsAzure.Common;

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
            var job = new BackupJob(new Uri(uri), RoleSettings.StorageCredentials);
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
            var backups = BackupManager.GetBackups(RoleSettings.StorageCredentials, 
                RoleSettings.ReplicaSetName);
            var data = backups.Select(blob => new { name = blob.Name, uri = blob.Uri }); 
            return Json(new { backups = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns all the in-progress backup jobs.
        /// </summary>
        public JsonResult ListJobs()
        {
            // Use this opportunity to remove older jobs.
            BackupJobs.RemoveOldJobs();

            IEnumerable data;
            lock (BackupJobs.Jobs)
            {               
                data = BackupJobs.Jobs.Values.Select(job => job.ToAjaxObject());
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

        /// <summary>
        /// Removes jobs that finished over an hour ago.
        /// </summary>
        public static void RemoveOldJobs()
        {
            lock (Jobs)
            {
                foreach (BackupJob job in Jobs.Values.Where(
                    p => p.DateFinished.HasValue && 
                        DateTime.Now.Subtract(p.DateFinished.Value).TotalHours >= 1.0).ToList())
                    Jobs.Remove(job.Id);
            }
        }
    }
}
