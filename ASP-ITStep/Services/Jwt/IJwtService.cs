namespace ASP_ITStep.Services.Jwt
{
    public interface IJwtService
    {
        String EncodeJwt(Object payload, Object? header = null, String? secret = null);
        (Object, Object) DecodeJwt(String jwt, String? secret = null);
    }
}
