using MiniAuth.Identity;

namespace TestApiWithoutIdentity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMiniAuth();

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();
            app.UseMiniAuth();


            app.MapControllers();

            app.Run();
        }
    }
}
