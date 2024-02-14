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

        public static TBuilder RequireXAuthorization<TBuilder>(this TBuilder builder) where TBuilder : IEndpointConventionBuilder
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.RequireAuthorization(new XAuthorizeAttribute());
        }
    }

    public class XAuthorizeAttribute : Attribute, IAuthorizeData
    {
        public XAuthorizeAttribute() { }
        public XAuthorizeAttribute(string policy) { }

        public string Policy { get; set; }
        public string Roles { get; set; }
        public string AuthenticationSchemes { get; set; }
    }
}
