using System.Web.Mvc;
using System.Web.Routing;

namespace StudentManagementSystem
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            // Enable attribute routing (optional but recommended)
            routes.MapMvcAttributeRoutes();

            // Ignore axd files (default)
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Default route: directs to Account/Login if no route specified
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
