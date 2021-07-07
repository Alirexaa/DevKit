namespace DevKit.Web.Services.Jwt
{
    public class JwtSetting
    {
        public string SecretKey { get; set; }
        public string EncryptionKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int ExpirationMinutes { get; set; }
        public int NotBeforeMinutes { get; set; }
    }
}