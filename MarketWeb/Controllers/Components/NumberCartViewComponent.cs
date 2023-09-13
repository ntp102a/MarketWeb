using MarketWeb.Extension;
using MarketWeb.ModelViews;
using Microsoft.AspNetCore.Mvc;

namespace MarketWeb.Controllers.Components
{
    public class NumberCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>("GioHang");
            
            return View(cart);
        }
    }
}
