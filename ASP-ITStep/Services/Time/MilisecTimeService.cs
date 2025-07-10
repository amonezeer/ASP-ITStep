namespace ASP_ITStep.Services.Time
{
    public class MilisecTimeService : ITimeService
    {
        public long TimeStamp()
        {
            return (DateTime.Now.Ticks - DateTime.UnixEpoch.Ticks) / (long)1e4;
        }
    }
}
