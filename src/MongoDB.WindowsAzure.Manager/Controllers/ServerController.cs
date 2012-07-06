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
    public class ServerController : Controller
    {
        //
        // GET: /Server/
        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Server/Details/5
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
                return RedirectToAction("Index", "Dashboard");
            }
            catch (MongoException e)
            {
                TempData["flashErrorTitle"] = "Error during stepdown";
                TempData["flashError"] = e.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

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
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
