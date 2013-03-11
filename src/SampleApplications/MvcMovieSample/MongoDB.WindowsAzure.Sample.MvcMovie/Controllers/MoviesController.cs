/*
 * Copyright 2010-2013 10gen Inc.
 * file : MoviesController.cs
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.WindowsAzure.Common;
using MongoDB.WindowsAzure.Sample.MvcMovie.Models;

namespace MongoDB.WindowsAzure.Sample.MvcMovie.Controllers
{
    public class MoviesController : Controller
    {
        private static MongoClientSettings ConnectionSettings 
        {
            get
            {
                if (clientSettings == null)
                {
                    clientSettings = ConnectionUtilities.GetMongoClientSettings();
                }

                return clientSettings;
            }
        }

        private static MongoClientSettings clientSettings = null;

        private MongoCollection<Movie> GetMoviesCollection( )
        {
            var settings = ConnectionSettings;
            settings.ReadPreference = ReadPreference.SecondaryPreferred;
            var client = new MongoClient(settings);
            var server = client.GetServer();
            var database = server["movies"];
            var movieCollection = database.GetCollection<Movie>( "movies" );
            return movieCollection;
        }

        private MongoCollection<Movie> GetMoviesCollectionForEdit( )
        {
            var settings = ConnectionSettings;
            var client = new MongoClient(settings);
            var server = client.GetServer();
            var database = server["movies"];
            var movieCollection = database.GetCollection<Movie>( "movies" );
            return movieCollection;
        }

        //
        // GET: /Movies/

        public ViewResult Index( )
        {
            var collection = GetMoviesCollection( );
            var cursor = collection.FindAll( );
            try
            {
                var movieList = cursor.ToList<Movie>( );
                return View( movieList );
            }
            catch
            {
                clientSettings = null;
                throw;
            }
        }

        //
        // GET: /Movies/Details/5

        public ViewResult Details( string id )
        {
            var collection = GetMoviesCollection( );
            var query = Query.EQ( "_id", new ObjectId( id ) );
            try
            {
                var movie = collection.FindOne( query );
                return View( movie );
            }
            catch
            {
                clientSettings = null;
                throw;
            }
        }

        //
        // GET: /Movies/Create

        public ActionResult Create( )
        {
            return View( );
        }

        //
        // POST: /Movies/Create

        [HttpPost]
        public ActionResult Create( [Bind( Exclude = "Id" )] Movie movie )
        {
            if ( ModelState.IsValid )
            {
                var collection = GetMoviesCollectionForEdit( );
                try
                {
                    collection.Insert( movie );
                    return RedirectToAction( "Index" );
                }
                catch
                {
                    clientSettings = null;
                    throw;
                }
            }

            return View( movie );
        }

        //
        // GET: /Movies/Edit/5

        public ActionResult Edit( string id )
        {
            var collection = GetMoviesCollectionForEdit( );
            var query = Query.EQ( "_id", new ObjectId( id ) );
            try
            {
                var movie = collection.FindOne( query );
                return View( movie );
            }
            catch
            {
                clientSettings = null;
                throw;
            }
        }

        //
        // POST: /Movies/Edit/5

        [HttpPost]
        public ActionResult Edit( Movie movie )
        {
            if ( ModelState.IsValid )
            {
                var collection = GetMoviesCollectionForEdit( );
                try
                {
                    collection.Save( movie );
                    return RedirectToAction( "Index" );
                }
                catch
                {
                    clientSettings = null;
                    throw;
                }
            }
            return View( movie );
        }

        //
        // GET: /Movies/Delete/5

        public ActionResult Delete( string id )
        {
            var collection = GetMoviesCollectionForEdit( );
            var query = Query.EQ( "_id", new ObjectId( id ) );
            try
            {
                var movie = collection.FindOne( query );
                return View( movie );
            }
            catch
            {
                clientSettings = null;
                throw;
            }
        }

        //
        // POST: /Movies/Delete/5

        [HttpPost, ActionName( "Delete" )]
        public ActionResult DeleteConfirmed( string id )
        {
            var collection = GetMoviesCollectionForEdit( );
            var query = Query.EQ( "_id", new ObjectId( id ) );
            try
            {
                var result = collection.Remove( query );
                return RedirectToAction( "Index" );
            }
            catch
            {
                clientSettings = null;
                throw;
            }
        }

        protected override void Dispose( bool disposing )
        {
            base.Dispose( disposing );
        }

        //
        // GET: /Movies/About

        public ActionResult About( )
        {
            return View( ReplicaSetStatus.GetReplicaSetStatus( ) );
        }

    }
}