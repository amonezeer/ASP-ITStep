namespace ASP_ITStep.Services.Identity
{
    public interface IIdentityService
    {
        long GenerateId();
        string GetIdInfo(long id);
    }
}
