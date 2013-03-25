/*
 * Copyright 2010-2013 10gen Inc.
 * file : SnapshotController.cs
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
    using System.Linq;
    using System.Web.Mvc;

    using MongoDB.WindowsAzure.Backup;
    using MongoDB.WindowsAzure.Common;
    using MongoDB.WindowsAzure.Manager.Models;

    /// <summary>
    /// Manages server snapshots.
    /// </summary>
    public class SnapshotController : Controller
    {
        //=========================================================================
        //
        //  REGULAR ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Creates a new snapshot based on the current primary.
        /// </summary>
        /// <returns></returns>
        public ActionResult New()
        {
            var uri = SnapshotManager.MakeSnapshot(ServerStatus.Primary.Id, 
                RoleSettings.StorageCredentials, RoleSettings.ReplicaSetName);

            TempData["flashSuccess"] = "Snapshot created!";
            return RedirectToAction("Index", "Dashboard");
        }

        //=========================================================================
        //
        //  AJAX ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Returns a list of all snapshots, including their URLs, blob names, and dates.
        /// </summary>
        /// <returns></returns>
        public JsonResult List()
        {
            var snapshots = SnapshotManager.GetSnapshots(RoleSettings.StorageCredentials, 
                RoleSettings.ReplicaSetName);

            var data = snapshots.Select(blob => new { 
                dateString = ToString(blob.Attributes.Snapshot), 
                blob = blob.Name, uri = SnapshotManager.GetSnapshotUri(blob) });
            return Json(new { snapshots = data }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the snapshot with the given URI.
        /// </summary>
        /// <returns></returns>
        public JsonResult Delete(string uri)
        {
            SnapshotManager.DeleteBlob(uri, RoleSettings.StorageCredentials);
            return Json(new { success = true });
        }

        /// <summary>
        /// Turns the date into a string presentation.
        /// </summary>
        public static string ToString(DateTime? snapshotTime)
        {
            return snapshotTime.Value.ToShortDateString() + " " + 
                snapshotTime.Value.ToShortTimeString();
        }
    }
}
