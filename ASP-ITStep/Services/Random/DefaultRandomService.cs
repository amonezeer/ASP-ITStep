using ASP_ITStep.Services.Time;
using System.Security.Cryptography.X509Certificates;

namespace ASP_ITStep.Services.Random
{
    public class DefaultRandomService : IRandomService
    {
        private readonly System.Random random;
        public DefaultRandomService(ITimeService timeService) 
        { 
            random = new System.Random((int)timeService.TimeStamp());
        }

        public string Otp(int lenth)
        {
           return String.Join("", 
               Enumerable.Range(0, lenth).Select(_ => random.Next(10))
          );
        }
    }
}
