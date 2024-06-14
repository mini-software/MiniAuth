using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MiniAuth;
using System.Text;

namespace TestBearer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            MiniAuthOptions.AuthenticationType = MiniAuthOptions.AuthType.BearerJwt;
            MiniAuthOptions.JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6ee3edbf-488e-4484-9c2c-e3ffa6dcbc09"));
            builder.Services.AddMiniAuth();

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!")
                .RequireAuthorization();    
            ;

            app.Run();
        }
    }
}
