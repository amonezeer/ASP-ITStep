namespace ASP_ITStep.Services.Email
{
    public interface IEmailService
    {
        void Send(String to, String subject, String content);
    }
}
