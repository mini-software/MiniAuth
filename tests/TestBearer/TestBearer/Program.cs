using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using MiniAuth;

namespace TestBearer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            MiniAuthOptions.AuthenticationType = MiniAuthOptions.AuthType.BearerJwt;
            MiniAuthOptions.TokenExpiresIn = 30;
            builder.Services.AddMiniAuth();

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!")
                .RequireAuthorization();    
            ;

            app.Run();
        }
    }
}
