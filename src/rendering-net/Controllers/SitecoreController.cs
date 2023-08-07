using Microsoft.AspNetCore.Mvc;
using Sitecore.AspNet.RenderingEngine.Binding.Attributes;
using Sitecore.LayoutService.Client.Response.Model;

namespace PlayWebsite.Controllers
{
    public class SitecoreController : Controller
    {
        public IActionResult Index([SitecoreRouteField] Field<string> pageTitle)
        {
            ViewBag.Title = pageTitle.Value;

            return View();
        }

    }
}
