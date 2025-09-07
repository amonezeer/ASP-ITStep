using ASP_ITStep.Data;
using ASP_ITStep.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP_ITStep.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataAccessor dataAccessor, ILogger<CartController> logger) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<CartController> _logger = logger;

        [HttpPost("{id}")]
        public RestResponse AddToCart([FromRoute] String id)
        {
            RestResponse restResponse = new();
            restResponse.Meta.ResourceName = "Shop API 'user cart'";
            restResponse.Meta.ResourceUrl = $"/api/cart/{id}";
            restResponse.Meta.Method = "POST";
            restResponse.Meta.Manipulations = ["POST", "PATCH", "DELETE"];

            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String? userId = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
                if (userId == null)
                {
                   restResponse.Status = RestStatus.RestStatus403;
                   restResponse.Data = "PrimarySid not found";
                   restResponse.Meta.DataType = "string";
                }
                else
                try
                {
                    _dataAccessor.AddToCart(userId, id);
                }
                catch (Exception e) when (e is ArgumentException || e is ArgumentNullException || e is FormatException)
                {
                    restResponse.Status = RestStatus.RestStatus400;
                    restResponse.Data = e.Message ;
                    restResponse.Meta.DataType = "string";
                }
                catch(Exception e)
                {
                    restResponse.Status = RestStatus.RestStatus500;
                        _logger.LogError("AddToCart {ex}", e.Message);
                }
            }
            else
            {
                restResponse.Status = RestStatus.RestStatus401;
            }
            return restResponse;
        }
    }
}
