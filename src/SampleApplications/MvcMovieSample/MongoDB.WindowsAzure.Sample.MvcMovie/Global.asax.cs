﻿/*
 * Copyright 2010-2013 10gen Inc.
 * file : Global.asax.cs
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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace MongoDB.WindowsAzure.Sample.MvcMovie
{

    using MongoDB.Bson;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Movies", action = "About", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            ModelBinders.Binders.Add(typeof(ObjectId), new ObjectIdBinder());
            RegisterRoutes(RouteTable.Routes);
        }


        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute
            {
                ExceptionType = typeof(MongoDB.Driver.MongoConnectionException),
                View = "ConnectError",
                Order = 2
            });
            filters.Add(new HandleErrorAttribute
            {
                ExceptionType = typeof(System.Net.Sockets.SocketException),
                View = "ConnectError",
                Order = 2
            });
            filters.Add(new HandleErrorAttribute());
        }
    }
}