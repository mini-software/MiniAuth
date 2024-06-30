using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Authorization;

namespace TestCookieApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMiniAuth();
            var app = builder.Build();


            app.MapGet("/", () => "Hello World!")
                .RequireAuthorization()
            ;
            app.MapGet("/test/admin", () => "Is miniauth-admin!")
            .RequireAuthorization(
                new AuthorizeAttribute() { Roles = "miniauth-admin" })
            ;
            app.Run();
        }
    }
}
