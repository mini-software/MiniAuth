using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MiniAuth
{
    public class MiniAuthOptions
    {
        public static string RoutePrefix = "MiniAuth";
        public static string LoginPath = $"/{RoutePrefix}/login.html";
        public static bool DisableMiniAuthLogin = false;
        public enum AuthType
        {
            Cookie,
            BearerJwt
        }
        public static AuthType AuthenticationType = AuthType.Cookie;
        public static SecurityKey JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is miniauth key for demo"));
        /// <summary>
        /// Seconds
        /// </summary>
        public static int TokenExpiresIn = 30*60;
        public static string Issuer = $"{RoutePrefix}";
        public static string SqliteConnectionString = "Data Source=miniauth_identity.db";
    }
}
