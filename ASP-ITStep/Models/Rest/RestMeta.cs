namespace ASP_ITStep.Models.Rest
{
    public class RestMeta
    {
        public long ServerTime { get; set; } = DateTime.Now.Ticks;
        public String ResourceName { get; set; } = null!;
        public String ResourceUrl { get; set; } = null!;
        public String DataType { get; set; } = null!;
        public String Method { get; set; } = "GET";
        public long Cash { get; set; }
        public String[] Manipulations { get; set; } = ["GET"];
        public Dictionary<String, String> Links { get; set; } = []; 
    }
}
