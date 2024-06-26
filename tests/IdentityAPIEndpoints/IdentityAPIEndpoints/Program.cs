﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace IdentityAPIEndpoints
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // http://localhost:5135/swagger/index.html
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthorization();
            builder.Services.AddDbContext<IdentityDbContext>(options =>
                options.UseSqlite("Data Source=identity.db"));
            builder.Services
                .AddIdentityApiEndpoints<IdentityUser>()
                .AddEntityFrameworkStores<IdentityDbContext>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            using (var scope = builder.Services.BuildServiceProvider().CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
                if (ctx.Database.EnsureCreated())
                {

                }
            }


            var app = builder.Build();
            app.MapGet("/", () => "Hello World!").RequireAuthorization()
                ;
            app.MapGroup("/account").MapIdentityApi<IdentityUser>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization(); 
            app.Run();
        }
    }
}
