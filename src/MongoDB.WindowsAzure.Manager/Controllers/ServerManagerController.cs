using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Manager.Models;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class ServerManagerController : Controller
    {
        //
        // GET: /ServerManager/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /ServerManager/Details/5

        public ActionResult Details(int id)
        {
            var status = ReplicaSetStatus.GetReplicaSetStatus( );
            var server = status.servers.Find( delegate( ServerStatus s ) { return ( s.Id == id ); } );
            return View( server );
        }

        //
        // GET: /ServerManager/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ServerManager/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /ServerManager/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /ServerManager/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /ServerManager/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /ServerManager/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
