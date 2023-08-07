using Microsoft.AspNetCore.Mvc;

namespace PlayWebsite.ViewComponents.Navigation
{
    public class HeaderContentViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
