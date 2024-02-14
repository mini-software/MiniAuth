using System;
using System.IO;
using System.Reflection;

namespace MiniAuth
{
    public class MiniAuthOptions
    {
        public string RoutePrefix { get; set; } = "MiniAuth";
        public int ExpirationMinuteTime { get; set; } = 24 * 60;
        public Func<Stream> IndexStream { get; set; } = () => typeof(MiniAuthOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("MiniAuth.wwwroot.login.html");
    }
}
