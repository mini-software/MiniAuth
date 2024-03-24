using System;
using System.IO;
using System.Reflection;

namespace MiniAuth.Configs
{
    public class MiniAuthOptions
    {
        internal string RoutePrefix { get; set; } = "MiniAuth"; //TODO: public option
        public int ExpirationMinuteTime { get; set; } = 24 * 60;
        public Func<Stream> LoginHtmlStream { get; set; } = () => typeof(MiniAuthOptions).GetTypeInfo().Assembly
            .GetManifestResourceStream("MiniAuth.wwwroot.login.html");
        internal string SubjectName { get; set; } = "miniauth";
        internal string Password { get; set; } = "miniauth";
        internal string CerPath { get; set; } = "miniauth.pfx";
    }
}
