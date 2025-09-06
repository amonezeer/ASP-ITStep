using ASP_ITStep.Data;
using ASP_ITStep.Models.Shop;
using Microsoft.AspNetCore.Mvc;

namespace ASP_ITStep.Controllers
{
    public class ShopController(DataAccessor dataAccessor) : Controller
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        public IActionResult Index()
        {
            ShopIndexPageModel model = new()
            {
                ProductGroup = _dataAccessor.GetProductGroups(),
            };
            return View(model);
        }

        public IActionResult Group([FromRoute] String Id)
        {
            ShopGroupPageModel model = new()
            {
                ProductGroup = _dataAccessor.GetProductGroupBySlug(Id),
                ProductGroups = _dataAccessor.GetProductGroups().ToList()
            };
            return View(model);
        }

        public IActionResult Item([FromRoute] String Id)
        {
            ShopItemPageModel model = new()
            {
                Product = _dataAccessor.GetProductBySlug(Id),
            };
            return View(model);
        }

        public IActionResult Cart()
        {
            return View();
        }
        public IActionResult Admin()
        {
            ShopAdminPageModel model = new()
            {
                ProductGroups = _dataAccessor.GetProductGroups().Select(g => new Models.OptionModel
                {
                    Value = g.Id.ToString(),
                    Content = $"{g.Name} ({g.Description})"
                }),
            };
            return View(model);
        }
    }
}
