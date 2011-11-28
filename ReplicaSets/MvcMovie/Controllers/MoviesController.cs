/* Copyright 2010-2011 10gen Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcMovie.Models;

using MongoDB.Azure.ReplicaSets.MongoDBHelper;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private MongoCollection<Movie> GetMoviesCollection()
        {
            var server = MongoDBHelper.GetSlaveOkReplicaSetConnection();
            var database = server["movies"];
            var movieCollection = database.GetCollection<Movie>("movies");
            return movieCollection;
        }

        private MongoCollection<Movie> GetMoviesCollectionForEdit()
        {
            var server = MongoDBHelper.GetReplicaSetConnection();
            var database = server["movies"];
            var movieCollection = database.GetCollection<Movie>("movies");
            return movieCollection;
        }

        //
        // GET: /Movies/

        public ViewResult Index()
        {
            var collection = GetMoviesCollection();
            var cursor = collection.FindAll();
            return View(cursor.ToList<Movie>());
        }

        //
        // GET: /Movies/Details/5

        public ViewResult Details(string id)
        {
            var collection = GetMoviesCollection();
            var query = Query.EQ("_id", new ObjectId(id));
            var movie = collection.FindOne(query);
            return View(movie);
        }

        //
        // GET: /Movies/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Movies/Create

        [HttpPost]
        public ActionResult Create(Movie movie)
        {
            if (ModelState.IsValid)
            {
                var collection = GetMoviesCollectionForEdit();
                collection.Insert(movie);
                return RedirectToAction("Index");
            }

            return View(movie);
        }

        //
        // GET: /Movies/Edit/5

        public ActionResult Edit(string id)
        {
            var collection = GetMoviesCollectionForEdit();
            var query = Query.EQ("_id", new ObjectId(id));
            var movie = collection.FindOne(query);
            return View(movie);
        }

        //
        // POST: /Movies/Edit/5

        [HttpPost]
        public ActionResult Edit(Movie movie)
        {
            if (ModelState.IsValid)
            {
                var collection = GetMoviesCollectionForEdit();
                collection.Save(movie);
                return RedirectToAction("Index");
            }
            return View(movie);
        }

        //
        // GET: /Movies/Delete/5

        public ActionResult Delete(string id)
        {
            var collection = GetMoviesCollectionForEdit();
            var query = Query.EQ("_id", new ObjectId(id));
            var movie = collection.FindOne(query);
            return View(movie);
        }

        //
        // POST: /Movies/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            var collection = GetMoviesCollectionForEdit();
            var query = Query.EQ("_id", new ObjectId(id));
            var result = collection.Remove(query);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        //
        // GET: /Movies/About

        public ActionResult About()
        {
            return View();
        }

    }
}