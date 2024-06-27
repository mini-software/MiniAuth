using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MiniAuth;
using System.Text;

namespace TestCustomOptions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            //builder.Services.AddMiniAuth(options: (options) =>
            //{
            //    options.JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6ee3edbf-488e-4484-9c2c-e3ffa6dcbc09"));
            //    options.LoginPath = "/Identity/Account/Login";
            //    options.DisableMiniAuthLogin = true;
            //});

            builder.Services.AddMiniAuth(options: (options) =>
            {
                options.RoutePrefix = "MiniAuth";
                options.LoginPath = $"/{options.RoutePrefix}/login.html";
                options.DisableMiniAuthLogin = false;
                options.AuthenticationType = AuthType.BearerJwt;
                options.JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("This is a secret key"));
                options.TokenExpiresIn = 3600;
                options.Issuer = "MiniAuth";
                options.SqliteConnectionString = "Data Source=MiniAuth.db";
            });

            var app = builder.Build();
            
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
