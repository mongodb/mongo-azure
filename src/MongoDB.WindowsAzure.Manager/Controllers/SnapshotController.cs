using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MongoDB.WindowsAzure.Tools;
using MongoDB.WindowsAzure.Manager.Models;

namespace MongoDB.WindowsAzure.Manager.Controllers
{
    public class SnapshotController : Controller
    {
        //=========================================================================
        //
        //  REGULAR ACTIONS
        //
        //=========================================================================

        /// <summary>
        /// Cretes a new snapshot based on the current primary.
        /// </summary>
        /// <returns></returns>
        public ActionResult New()
        {
            var uri = SnapshotManager.MakeSnapshot("DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==", ServerStatus.Primary.Id);

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
        public JsonResult GetAll()
        {
            var snapshots = SnapshotManager.GetSnapshots("DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==");

            var pairs = snapshots.Select(blob => new { dateString = ToString(blob.Attributes.Snapshot), blob = blob.Name, uri = SnapshotManager.ToSnapshotUri(blob) });
            return Json(new { snapshots = pairs }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Deletes the snapshot with the given URI.
        /// </summary>
        /// <returns></returns>
        public JsonResult Delete(string uri)
        {
            SnapshotManager.DeleteBlob(uri, "DefaultEndpointsProtocol=http;AccountName=managerstorage4;AccountKey=zJrhOZSDVLod52wsdtx4j3nPku57EQlVmjkACSW3cwUv3oo9bz+8n+sbzlfXpnjfxshLsx8jfTmm99BTkC1Img==");
            return Json(new { success = true });
        }

        /// <summary>
        /// Turns the date into a string presentation.
        /// </summary>
        public static string ToString(DateTime? snapshotTime)
        {
            return snapshotTime.Value.ToShortDateString() + " " + snapshotTime.Value.ToShortTimeString();
        }
    }
}
