using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MiniAuth
{
    public class MiniAuthOptions
    {
        public string RoutePrefix { get; set; } = "MiniAuth";
        public static string LoginPath = "/miniauth/login.html";
        public static bool DisableMiniAuthLogin = false;
        public enum AuthType
        {
            Cookie,
            BearerJwt
        }
        public static AuthType AuthenticationType = AuthType.Cookie;
        public static SecurityKey IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is miniauth key for demo"));
        /// <summary>
        /// Seconds
        /// </summary>
        public static int TokenExpiresIn = 15*60;
    }
}
