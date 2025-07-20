using System.Text.Json.Serialization;

namespace ASP_ITStep.Data.Entities
{
    public class AccessToken  // організований за стандартом JWT
    {
        public String Jti { get; set; } = null!;
        public Guid Sub { get; set; } // UserAccessId
        public String? Iat { get; set; } // 
        public String? Exp { get; set; } //
        public String? Nbf { get; set; } //
        public String? Aud { get; set; }  // Role / RoleId
        public String? iss { get; set; }  // Видавник токена

        // ---------------------------
        [JsonIgnore]
        public UserAccess userAccess { get; set; } = null!;
    }
}
