using Microsoft.AspNetCore.Builder;
using MiniAuth;
using System;

public static class MiniAuthBuilderExtensions
{
    public static IApplicationBuilder UseMiniAuth(this IApplicationBuilder builder)
    {
        _ = builder ?? throw new ArgumentNullException(nameof(builder));
        return builder.UseMiddleware<MiniAuthMiddleware>();
    }
}

