﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
                builder.UseMiniAuth();
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
