using System;
using System.Threading;

namespace ASP_ITStep.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private static readonly object _lock = new object();
        private static long _lastTimestamp = 0;
        private static int _counter = 0;
        private const int MAX_COUNTER = 999;

        public long GenerateId()
        {
            lock (_lock)
            {
                long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                if (currentTimestamp == _lastTimestamp)
                {
                    _counter++;
                    if (_counter > MAX_COUNTER)
                    {
                        while (currentTimestamp <= _lastTimestamp)
                        {
                            currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        }
                        _counter = 0;
                    }
                }
                else
                {
                    _counter = 0;
                }

                _lastTimestamp = currentTimestamp;
                string timestampStr = currentTimestamp.ToString();
                string reversedTimestamp = ReverseString(timestampStr);
                string idStr = reversedTimestamp + _counter.ToString("D3");

                return long.Parse(idStr);
            }
        }

        public string GetIdInfo(long id)
        {
            string idStr = id.ToString();
            if (idStr.Length > 3)
            {
                string counterPart = idStr.Substring(idStr.Length - 3);
                string timestampPart = idStr.Substring(0, idStr.Length - 3);
                string originalTimestamp = ReverseString(timestampPart);

                if (long.TryParse(originalTimestamp, out long timestamp))
                {
                    var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
                    return $"ID: {id}, Час: {dateTime:yyyy-MM-dd HH:mm:ss.fff}, Лічильник: {counterPart}";
                }
            }
            return $"ID: {id} (неможливо розпарсити)";
        }

        private string ReverseString(string input)
        {
            char[] chars = input.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }
    }
}