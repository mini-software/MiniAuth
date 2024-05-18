using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;

namespace MiniAuth.Identity
{
    public class MiniAuthStartupFilter : IStartupFilter
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
    public class EmptyStartupFilter : IStartupFilter
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
