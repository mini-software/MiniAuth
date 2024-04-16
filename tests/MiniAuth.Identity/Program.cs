namespace MiniAuth.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            //builder.Services.AddControllers();
            builder.Services.AddMiniIdentityAuth();
            var app = builder.Build();
            app.UseCors("AllowAll");
            app.MapGet("/", () => "Hello World!");
            //app.MapControllers();
            app.UseMiniIdentityAuth();
            app.Run();
        }
    }
}
