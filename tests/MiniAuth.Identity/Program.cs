using Microsoft.AspNetCore.Identity;
using MiniAuth.IdentityAuth.Models;
using System.Diagnostics;

namespace MiniAuth.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Debug.WriteLine("* start Services add");
            builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            //builder.Services.AddControllers();
            builder.Services.AddMiniIdentityAuth();
#if DEBUG
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //builder.Services.AddIdentityApiEndpoints<IdentityUser>();
#endif

            Debug.WriteLine("* start builder build");   
            var app = builder.Build();
            app.UseCors("AllowAll");
            app.MapGet("/", () => "Hello World!");
            //app.MapGroup("/api").MapIdentityApi<IdentityUser>();
            //app.MapControllers();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseMiniIdentityAuth();
            app.Run();
        }
    }
}
