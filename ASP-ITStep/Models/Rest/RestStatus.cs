namespace ASP_ITStep.Models.Rest
{
    public class RestStatus
    {
        public bool IsOk { get; set; } = true;
        public int Code { get; set; } = 200;
        public String Phrase { get; set; } = "Ok";

        public static readonly RestStatus RestStatus403 = new()
        {
            IsOk = false,
            Code = 403,
            Phrase = "Forbidden"
        };
        public static readonly RestStatus RestStatus401 = new()
        {
            IsOk = false,
            Code = 403,
            Phrase = "UnAutorized"
        };
        public static readonly RestStatus RestStatus400 = new()
        {
            IsOk = false,
            Code = 400,
            Phrase = "Bad request"
        };
        public static readonly RestStatus RestStatus500 = new()
        {
            IsOk = false,
            Code = 500,
            Phrase = "Internal Error.Details in Server logs"
        };
        public static readonly RestStatus RestStatus201 = new()
        {
            Code = 201,
            IsOk = true,
            Phrase = "Created"
        };
    }
}
