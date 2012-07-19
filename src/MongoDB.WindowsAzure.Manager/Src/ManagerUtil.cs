/*
 * Copyright 2010-2012 10gen Inc.
 * file : ManagerUtil.cs
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

namespace MongoDB.WindowsAzure.Manager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Microsoft.WindowsAzure.ServiceRuntime;

    public class ManagerUtil
    {
        /// <summary>
        /// Whether we're running the ASP.NET web app directly instead of via the Azure emulator. (Useful, because running the MVC app directly is faster without the emulator.)
        /// If true, this app generates dummy data instead of trying to connect to the real replica set.
        /// </summary>
        public static bool IsRunningWebAppDirectly
        {
            get
            {
                return !RoleEnvironment.IsAvailable;
            }
        }

        /// <summary>
        /// The Unix time epoch. 
        /// </summary>
        public static DateTime UnixEpoch { get { return new DateTime(1970, 1, 1, 0, 0, 0); } }

        /// <summary>
        /// If the specified date is equal to the Unix epoch, converts it to the .NET-style DateTime.MinValue. Otherwise, no change is made.        
        /// </summary>
        public static DateTime RemoveUnixEpoch(DateTime date)
        {
            return (date == UnixEpoch) ? DateTime.MinValue : date;
        }
    }
}