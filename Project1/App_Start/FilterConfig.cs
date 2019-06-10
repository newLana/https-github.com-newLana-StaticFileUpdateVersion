using Project1.Filters;
using System.Web;
using System.Web.Mvc;

namespace Project1
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new StaticFilesVersionAttribute());
        }
    }
}
