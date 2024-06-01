using MiniAuth;

namespace TestAspNetCoreEmpty
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            MiniAuthOptions.AuthenticationType = MiniAuthOptions.AuthType.Jwt;
            builder.Services.AddMiniAuth();
            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
