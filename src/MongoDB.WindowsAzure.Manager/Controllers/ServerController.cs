using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Manager.Models;
using System.Diagnostics;
using MongoDB.Driver;
using System.IO;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    /// <summary>
    /// Controls individual servers.
    /// </summary>
    public class ServerController : Controller
    {
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
                TempData["flashSuccess"] = "It might take a few seconds for the replica set to come back online.";
                return RedirectToAction("Details", new { id = id });
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
            return RedirectToAction("Details", new { id = id } );
        }
    }
}
