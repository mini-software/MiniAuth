using Microsoft.AspNetCore.Authentication.Cookies;

namespace MiniAuth.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.UseMiniAuth();
            app.MapGet("/", () => "Hello MiniAuth!");
            app.Run();
        }
    }
}
