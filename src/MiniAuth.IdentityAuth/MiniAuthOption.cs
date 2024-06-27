using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MiniAuth
{
    public class MiniAuthOptions
    {
        public string RoutePrefix
        {
            set { MiniAuthOption.RoutePrefix = value; }
            get { return MiniAuthOption.RoutePrefix; }
        }
        public string LoginPath
        {
            set { MiniAuthOption.LoginPath = value; }
            get { return MiniAuthOption.LoginPath; }
        }
        public bool DisableMiniAuthLogin
        {
            set { MiniAuthOption.DisableMiniAuthLogin = value; }
            get { return MiniAuthOption.DisableMiniAuthLogin; }
        }
        public AuthType AuthenticationType
        {
            set { MiniAuthOption.AuthenticationType = value; }
            get { return MiniAuthOption.AuthenticationType; }
        }
        public SecurityKey JWTKey
        {
            set { MiniAuthOption.JWTKey = value; }
            get { return MiniAuthOption.JWTKey; }
        }

        /// <summary>
        /// Token expires in seconds, default is 1 hour
        /// </summary>
        public int TokenExpiresIn
        {
            set { MiniAuthOption.TokenExpiresIn = value; }
            get { return MiniAuthOption.TokenExpiresIn; }
        }
        public string Issuer
        {
            set { MiniAuthOption.Issuer = value; }
            get { return MiniAuthOption.Issuer; }
        }
        public string SqliteConnectionString
        {
            set { MiniAuthOption.SqliteConnectionString = value; }
            get { return MiniAuthOption.SqliteConnectionString; }
        }
    }
    public enum AuthType
    {
        Cookie,
        BearerJwt
    }
    internal class MiniAuthOption

    {
        public static string RoutePrefix = "MiniAuth";
        public static string LoginPath = $"/{RoutePrefix}/login.html";
        public static bool DisableMiniAuthLogin = false;

        public static AuthType AuthenticationType = AuthType.Cookie;
        public static SecurityKey JWTKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is miniauth key for demo"));
        /// <summary>
        /// Token expires in seconds, default is 1 hour
        /// </summary>
        public static int TokenExpiresIn = 60 * 60;
        public static string Issuer = $"{RoutePrefix}";
        public static string SqliteConnectionString = "Data Source=miniauth_identity.db";
    }
}
