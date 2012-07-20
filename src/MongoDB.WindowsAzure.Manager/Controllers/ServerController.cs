/*
 * Copyright 2010-2012 10gen Inc.
 * file : ServerController.cs
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using MongoDB.WindowsAzure.Manager.Models;
    using System.Diagnostics;
    using MongoDB.Driver;
    using System.IO;
    using MongoDB.WindowsAzure.Manager.Src;
    using System.Text;
    using MongoDB.Bson;
    using MongoDB.Driver.Builders;

    /// <summary>
    /// Manages individual servers.
    /// </summary>
    public class ServerController : Controller
    {
        //=========================================================================
        //
        //  REGULAR ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Shows details about one server.
        /// </summary>
        public ActionResult Details(int id)
        {
            var server = ServerStatus.Get(id);
            if (server == null)
            {
                TempData["flashError"] = "That server does not exist.";
                return RedirectToAction("Index", "Dashboard");
            }

            return View(server);
        }

        /// <summary>
        /// Tells the given primary server to step down (elect another primary).
        /// </summary>
        public ActionResult StepDown(int id)
        {
            var server = ServerStatus.Get(id);
            if (server == null)
            {
                TempData["flashError"] = "That server does not exist.";
                return RedirectToAction("Index", "Dashboard");
            }

            var mongo = MongoServer.Create("mongodb://" + server.Name + "/");
            try
            {
                var result = mongo["admin"].RunCommand("replSetStepDown");
                return RedirectToAction("Details", new { id = id });
            }
            catch (EndOfStreamException)
            {
                // [PC] This occurs when the command succeeded - driver bug?
                TempData["flashSuccessTitle"] = "Stepdown succeeded";
                TempData["flashSuccess"] = "It will take a few seconds for the replica set to come back online. Refresh the page manually.";
                return RedirectToAction("Index", "Dashboard");
            }
            catch (MongoException e)
            {
                TempData["flashErrorTitle"] = "Error during stepdown";
                TempData["flashError"] = e.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        /// <summary>
        /// Tells the given server to start a new logfile and archive the old one.
        /// </summary>
        public ActionResult LogRotate(int id)
        {
            var server = ServerStatus.Get(id);
            if (server == null)
            {
                TempData["flashError"] = "That server does not exist.";
                return RedirectToAction("Index", "Dashboard");
            }

            var mongo = MongoServer.Create("mongodb://" + server.Name + "/?slaveOk=true");
            try
            {
                var result = mongo["admin"].RunCommand("logRotate");
            }
            catch (MongoException e)
            {
                TempData["flashErrorTitle"] = "Error during log rotation";
                TempData["flashError"] = e.Message;
                return RedirectToAction("Index", "Dashboard");
            }

            TempData["flashSuccess"] = "Logs rotated on " + server.Name + ".";
            return RedirectToAction("Details", new { id = id });
        }


        /// <summary>
        /// Downloads the Windows Azure Diagnostics (WAD) log of the given instance's mongod.
        /// This is slow and expensive (it requires a full blob fetch), and the logs are out-of-date by up to a minute -- but it works when the instance is down.
        /// </summary>
        public ActionResult DownloadAzureLog(int id)
        {
            SetFileDownloadHeaders("instance" + id + ".log", "text/plain");
            LogFetcher.WriteEntireLog(Response, id);
            Response.Close();
            return null;
        }

        /// <summary>
        /// Sends the given text as a file to the client to be downloaded.
        /// </summary>
        private void SetFileDownloadHeaders(string fileName, string mimeType)
        {
            Response.Clear();
            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentType = mimeType;
        }

        //=========================================================================
        //
        //  AJAX ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Fetches the instance log by connecting to its mongod server.
        /// This is fast and cheap, but won't work if the instance is down.
        /// </summary>
        public JsonResult GetServerLog(int id)
        {
            var server = ServerStatus.Get(id);
            var mongo = MongoServer.Create(new MongoServerSettings { ConnectTimeout = new TimeSpan(0, 0, 3), Server = MongoServerAddress.Parse(server.Name), SlaveOk = true });
            try
            {
                var result = mongo["admin"]["$cmd"].FindOne(Query.EQ("getLog", "global"));
                return Json(new { log = HtmlizeFromLogArray(result.AsBsonDocument["log"].AsBsonArray) }, JsonRequestBehavior.AllowGet);
            }
            catch (MongoException e)
            {
                return Json(new { error = e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Turns an array of log entries into an HTML block.
        /// </summary>
        private string HtmlizeFromLogArray(BsonArray logs)
        {
            StringBuilder str = new StringBuilder();
            foreach (var line in logs)
                str.Append("<div class='logLine'>" + line.AsString + "</div>");

            return str.ToString();
        }
    }
}
