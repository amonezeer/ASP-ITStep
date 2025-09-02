using ASP_ITStep.Data;
using ASP_ITStep.Filters;
using ASP_ITStep.Models.Api.Product;
using ASP_ITStep.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASP_ITStep.Controllers.Api
{
    [Route("api/product")]
    [ApiController]
    [AutorizationFilter]
    public class ProductController(IStorageService storageSerice, DataAccessor dataAccessor) : ControllerBase
    {
        private readonly IStorageService _storageService = storageSerice;
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpGet]
        public IEnumerable<string> ProductList()
        {
            return ["1", "2", "3"];
        }

        [HttpPost]
        public async Task<object> CreateProduct(ApiProductFormModel formModel)
        {
            if (string.IsNullOrWhiteSpace(formModel.GroupId) || !_dataAccessor.IsGroupExists(formModel.GroupId))
            {
                return new { status = 400, name = "Неправильний або відсутній GroupId" };
            }

            if (string.IsNullOrWhiteSpace(formModel.Name) || formModel.Name.Length < 3 || formModel.Name.Length > 100)
            {
                return new { status = 400, name = "Некоректна назва товару" };
            }

            if (!string.IsNullOrWhiteSpace(formModel.Description) && formModel.Description.Length > 1000)
            {
                return new { status = 400, name = "Опис занадто довгий" };
            }

            if (formModel.Price <= 0)
            {
                return new { status = 400, name = "Ціна повинна бути більшою за 0" };
            }

            if (formModel.Stock < 0)
            {
                return new { status = 400, name = "Кількість не може бути від’ємною" };
            }

            if (formModel.Slug != null)
            {
                if (_dataAccessor.IsProductSlugUsed(formModel.Slug))
                {
                    return new { status = 500, name = "Slug вже використовується іншою групою" };
                }
            }

            string? savedName = null;
            if (formModel.ImageUrl != null)
            {
                try
                {
                    // перевіряємо на дозволеність розширення
                    _storageService.TryGetMineType(formModel.ImageUrl.FileName);
                    savedName = await _storageService.SaveItemAsync(formModel.ImageUrl);
                }
                catch (Exception ex)
                {
                    return new { status = 400, name = ex.Message };
                }
            }

            try
            {
                _dataAccessor.AddProduct(new()
                {
                    GroupId = formModel.GroupId,
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Price = formModel.Price,
                    Stock = formModel.Stock,
                    ImageUrl = savedName,
                    Slug = formModel.Slug,
                });

                return new { status = 201, name = $"Товар {formModel.Name} створено" };
            }
            catch (Exception e) when (e is ArgumentNullException || e is FormatException)
            {
                return new { status = 400, name = e.Message };
            }
            catch
            {
                return new { status = 500, name = "Внутрішня помилка сервера" };
            }
        }
    }
}


/* Відмінності MVC та API контролерів
* MVC: один метод запиту (частіше за все GET) та різні адреси
* GET /home/privacy  -> HomeController::Privacy()
* POST /home/index  -> HomeController::Index()
* 
* API - одна адреса, різні методи запиту
* GET  /api/product -> ProductController::ProductsList()
* POST /api/product -> ProductController::CreateProduct()
* PUT  /api/product
* -----------------------------------------------------------
* MVC - повертають IActionResult
* API - об'єкти довільного типу, які далі ASP переводить до JSON
*/