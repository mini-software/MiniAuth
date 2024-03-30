using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using System;

namespace MiniAuth
{
    public static class MiniAuthBuilderExtensions
    {
        public static IApplicationBuilder UseMiniAuth(this IApplicationBuilder builder)
        {
            _ = builder ?? throw new ArgumentNullException(nameof(builder));
            return builder.UseMiddleware<MiniAuthMiddleware>();
        }
    }
}
