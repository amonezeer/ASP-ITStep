namespace ASP_ITStep.Services.Time
{
    public class SecTimeService : ITimeService
    {
        public long TimeStamp()
        {
           return (DateTime.Now.Ticks - DateTime.UnixEpoch.Ticks) / (long)1e7;
        }
    }
}
