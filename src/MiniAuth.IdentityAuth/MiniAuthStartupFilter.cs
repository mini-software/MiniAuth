using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics;

namespace MiniAuth.Identity
{
    internal class MiniAuthStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                Debug.WriteLine("* start MiniAuthStartupFilter");
                builder.UseMiniAuth<MiniAuthIdentityDbContext, IdentityUser, IdentityRole>();
                //builder.UseMiniAuth();
                next(builder);
            };
        }
    }
    internal class EmptyStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                Debug.WriteLine("* start EmptyStartupFilter");
                next(builder);
            };
        }
    }
}
