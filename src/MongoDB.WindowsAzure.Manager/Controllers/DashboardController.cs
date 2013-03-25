/*
 * Copyright 2010-2013 10gen Inc.
 * file : DashboardController.cs
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
    using System.Web.Mvc;
    using MongoDB.WindowsAzure.Manager.Models;

    /// <summary>
    /// The front page of the application; shows the server list and commands
    /// to manage them.
    /// </summary>
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            return View(ReplicaSetStatus.GetStatus());
        }
    }
}
