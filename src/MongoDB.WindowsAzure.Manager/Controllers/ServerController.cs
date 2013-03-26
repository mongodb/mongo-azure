/*
 * Copyright 2010-2013 10gen Inc.
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
    using System.IO;
    using System.Text;
    using System.Web.Mvc;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.WindowsAzure.Manager.Models;
    using MongoDB.WindowsAzure.Manager.Src;

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

            var client = new MongoClient(string.Format("mongodb://{0}/", server.Name));
            var conn = client.GetServer();
            try
            {
                var result = conn.GetDatabase("admin").RunCommand("replSetStepDown");
                return RedirectToAction("Details", new { id = id });
            }
            catch (EndOfStreamException)
            {
                // [PC] This occurs when the command succeeded - driver bug?
                TempData["flashSuccessTitle"] = "Stepdown succeeded";
                TempData["flashSuccess"] = 
                    "It will take a few seconds for the replica set to come back online. Refresh the page manually.";
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

            var client = new MongoClient(string.Format(
                "mongodb://{0}/?slaveOk=true", server.Name));
            var conn = client.GetServer();
            try
            {
                var result = conn.GetDatabase("admin").RunCommand("logRotate");
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
        /// Downloads the Windows Azure Diagnostics (WAD) log of the given 
        /// instance's mongod. This is slow and expensive (it requires a
        /// full blob fetch), and the logs are out-of-date by up to a minute
        /// but it works when the instance is down.
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
            var urlBuilder = new MongoUrlBuilder();
            urlBuilder.ConnectTimeout = new TimeSpan(0, 0, 3);
            urlBuilder.Server = MongoServerAddress.Parse(server.Name);
            urlBuilder.ReadPreference = ReadPreference.SecondaryPreferred;
            var client = new MongoClient(urlBuilder.ToMongoUrl());
            var conn = client.GetServer();
            try
            {
                var command = new CommandDocument
                {
                    { "getLog", "global" }
                };
                var result = conn.GetDatabase("admin").RunCommand(command);
                return Json(new { log = HtmlizeFromLogArray(result.
                    Response["log"].AsBsonArray) }, 
                    JsonRequestBehavior.AllowGet);
            }
            catch (MongoException e)
            {
                return Json(new { error = e.Message }, 
                    JsonRequestBehavior.AllowGet);
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
