using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Text;

namespace MiniAuth.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        //.AllowCredentials()
                        ;
                    });
            });
            builder.Services.AddMiniAuth(options:options =>
            {
                options.AuthenticationType = AuthType.BearerJwt;
                var key = builder.Configuration["JWTKey"];
                options.JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            });
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseCors("AllowAll");
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();
            app.Run();
        }
    }
}
