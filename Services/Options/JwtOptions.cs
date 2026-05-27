namespace Services.Options
{
    public class JwtOptions
    {
        public string? SecretKey { get; set; }
        public double ExpiresInHours { get; set; }
    }
}
