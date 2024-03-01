using System;
using System.IO;
using System.Reflection;

namespace MiniAuth.Configs
{
    public class MiniAuthOptions
    {
        internal string RoutePrefix { get; set; } = "MiniAuth"; //TODO: public option
        public bool AuthAllRoutes { get; set; } = true;
        public int ExpirationMinuteTime { get; set; } = 24 * 60;
        public Func<Stream> LoginHtmlStream { get; set; } = () => typeof(MiniAuthOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("MiniAuth.wwwroot.login.html");
    }
}
