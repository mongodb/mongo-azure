using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace MongoDB.WindowsAzure.Manager
{
    public class Util
    {
        /// <summary>
        /// Whether we're running the ASP.NET web app directly instead of via the Azure emulator. (Useful, because running the MVC app directly is faster without the emulator.)
        /// If true, this app generates dummy data instead of trying to connect to the real replica set.
        /// </summary>
        public static bool IsRunningWebAppDirectly
        {
            get
            {
                try
                {
                    return !(RoleEnvironment.IsAvailable || RoleEnvironment.IsEmulated);
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
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