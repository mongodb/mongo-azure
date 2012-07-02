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
        /// Whether we're running the manager ASP.NET directly (NOT via the Azure emulator).
        /// (This is done to speed development run time.)
        /// If true, dummy data is used, instead of trying to connect to the real replica set.
        /// </summary>
        public static bool IsRunningWebAppDirectly
        {
            get
            {
                try
                {
                    return !( RoleEnvironment.IsAvailable || RoleEnvironment.IsEmulated );
                }
                catch ( InvalidOperationException )
                {
                    return true;
                }
            }
        }
    }
}