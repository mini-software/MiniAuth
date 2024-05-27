namespace MiniAuth
{
    public class MiniAuthOptions
    {
        public string RoutePrefix { get; set; } = "MiniAuth";
        public static string LoginPath = "/miniauth/login.html";
        public static bool DisableMiniAuthLogin = false;
    }
}
