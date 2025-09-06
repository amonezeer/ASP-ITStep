using ASP_ITStep.Data;
using ASP_ITStep.Data.Entities;
using ASP_ITStep.Models.Api.Group;
using ASP_ITStep.Models.Rest;
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
        public RestResponse ExecuteGET()
        {
            var groups = _dataAccessor.GetProductGroups();
            RestResponse response = new();
            response.Meta.ResourceName = "ProductGroups";
            response.Meta.ResourceUrl = "/api/product-group";
            response.Meta.Method = "GET";
            response.Meta.DataType = nameof(ProductGroup);

            response.Status = new RestStatus
            {
                Code = 200,
                IsOk = true,
                Phrase = "OK"
            };
            return response;
        }

        [HttpPost]
        public RestResponse ExecutePOST([FromForm] ApiGroupFormModel formModel)
        {
            RestResponse response = new();

            if (string.IsNullOrEmpty(formModel.Slug))
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Slug is required";
            }
            if(_dataAccessor.IsGroupSlugUsed(formModel.Slug))
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Slug is already used";
            }

            if (!_dataAccessor.IsGroupExists(formModel.ParentId))
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "ParentId does not exist";
            }

            string? savedName = null;
            try
            {
                _storageService.TryGetMineType(formModel.ImageUrl.FileName);
                savedName = _storageService.SaveItem(formModel.ImageUrl);
            }
            catch (Exception ex)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Image upload failed: " + ex.Message;
            }
            try
            {
                if (savedName != null)
                {
                    _dataAccessor.AddProductGroup(new()
                    {
                        Name = formModel.Name,
                        Description = formModel.Description,
                        Slug = formModel.Slug,
                        ParentId = formModel.ParentId,
                        ImageUrl = savedName
                    });
                }

                response.Status = RestStatus.RestStatus201;
            }    
             catch { response.Status = RestStatus.RestStatus500; } 
            return response; 
        }
        
        private RestMeta CreateMeta(string method)
        {
            return new RestMeta
            {
                ResourceName = "ProductGroups",
                ResourceUrl = "/api/product-group",
                Method = method,
                DataType = nameof(ProductGroup)
            };
        }
    }
}
