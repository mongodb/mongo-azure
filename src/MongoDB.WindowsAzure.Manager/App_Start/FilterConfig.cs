using System.Web;
using System.Web.Mvc;

namespace MongoDB.WindowsAzure.Manager
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters( GlobalFilterCollection filters )
        {
            filters.Add( new HandleErrorAttribute( ) );
        }
    }
}