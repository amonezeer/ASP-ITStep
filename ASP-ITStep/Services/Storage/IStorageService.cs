namespace ASP_ITStep.Services.Storage
{
    public interface IStorageService
    {
        byte[] GetItemBytes(String itemName);
        String SaveItem(IFormFile formFile);
        String TryGetMineType(String itemName);

        Task<String> SaveItemAsync(IFormFile formFile);
    }
}
