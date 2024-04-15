namespace MiniAuth.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddMiniIdentityAuth(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Password.RequiredLength = 3;
            });
            var app = builder.Build();
            app.UseMiniIdentityAuth();
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
