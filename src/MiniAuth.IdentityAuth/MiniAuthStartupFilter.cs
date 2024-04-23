using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace MiniAuth.Identity
{
    public class MiniAuthStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiniIdentityAuth();
                next(builder);
            };
        }
    }
}
