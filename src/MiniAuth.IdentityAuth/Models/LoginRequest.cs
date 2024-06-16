namespace MiniAuth.IdentityAuth.Models
{
    public sealed class LoginRequest
    {
        public string username { get; init; }

        public string password { get; init; }
        public bool remember { get; init; }
    }
}
