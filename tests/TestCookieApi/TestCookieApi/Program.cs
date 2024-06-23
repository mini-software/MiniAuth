using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;

namespace TestCookieApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMiniAuth();
            var app = builder.Build();


            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
