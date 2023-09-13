using MarketWeb.Models;

namespace MarketWeb.ModelViews
{
    public class HomeViewVM
    {
        public List<TinDang> TinTuc { get; set; }
        public List<ProductHomeVM> Products { get; set; }
        public QuangCao quangcao { get; set; }
    }
}
