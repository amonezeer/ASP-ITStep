using ASP_ITStep.Data;
using ASP_ITStep.Data.Entities;
using ASP_ITStep.Models.Api.Group;
using ASP_ITStep.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_ITStep.Controllers.Api
{
    [Route("api/product-group")]
    [ApiController]
    public class ProductGroupController(DataAccessor dataAccessor, IStorageService storageService) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IStorageService _storageService = storageService;

        public object AnyRequest()
        {
            string methodName = "Execute" + HttpContext.Request.Method;
            var type = GetType();
            var action = type.GetMethod(methodName,
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            if (action == null)
            {
                return new
                {
                    status = 405,
                    message = "Method not Implemented"
                };
            }

            if (HttpContext.Request.Method == "GET")
            {
                return action.Invoke(this, null);
            }
            bool isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return new
                {
                    status = 401,
                    message = "UnAuthorized"
                };
            }

            return action.Invoke(this, null);
        }


        [HttpGet]
        public IEnumerable<ProductGroup> ExecuteGET()
        {
            return _dataAccessor.GetProductGroups();
        }

        [HttpPost]
        public object ExecutePOST(ApiGroupFormModel formModel)
        {
            if(string.IsNullOrEmpty(formModel.Slug))
            {
                return new { status = 400, name = "Slug could not be empty" };
            }
            if(_dataAccessor.IsGroupSlugUsed(formModel.Slug))
            {
                return new { status = 409, name = "Slug is used by other group" };
            }

            string savedName;
            try
            {
                _storageService.TryGetMineType(formModel.ImageUrl.FileName);
                savedName = _storageService.SaveItem(formModel.ImageUrl);
            }
            catch (Exception ex)
            {
                return new { status = 400, name = ex.Message };
            }
            try
            {
                _dataAccessor.AddProductGroup(new()
                {
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Slug = formModel.Slug,
                    ParentId = formModel.ParentId,
                    ImageUrl = savedName
                });
               return new { status = 201, name = "Created" };
            }
             catch { return new { status = 500, name = "Error" }; }    
        }
    }
}
