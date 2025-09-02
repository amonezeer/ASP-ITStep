using ASP_ITStep.Services.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ASP_ITStep.Controllers
{
    public class StorageController(IStorageService storageService) : Controller
    {
        private readonly IStorageService _storageService = storageService;
        [HttpGet]
        public IActionResult Item(String id)
        {
            try
            {
                return File(_storageService.GetItemBytes(id), _storageService.TryGetMineType(id));
            }
            catch 
            {
                return NotFound();
            }
        }
    }
}
