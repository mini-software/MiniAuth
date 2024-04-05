public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
        builder.Services.AddControllers();
        var app = builder.Build();
        app.UseCors("AllowAll");
        app.UseMiniAuth();
        app.MapControllers();
        app.MapGet("/miniapi/get", () => "Hello MiniAuth!");
        app.Run();
    }
}