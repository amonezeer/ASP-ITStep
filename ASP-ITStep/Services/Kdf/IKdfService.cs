namespace ASP_ITStep.Services.Kdf
{
    public interface IKdfService
    {
        string Dk(string password, string salt);
    }
}
