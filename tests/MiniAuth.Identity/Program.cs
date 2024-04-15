namespace MiniAuth.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();
            app.UseMiniIdentityAuth();
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
