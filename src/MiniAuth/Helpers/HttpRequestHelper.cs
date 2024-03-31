using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using MiniAuth.Managers;
using MiniAuth.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace MiniAuth.Helpers
{
    public static class HttpRequestHelper
    {
        public static MiniAuthUser GetMiniAuthUser(this Microsoft.AspNetCore.Mvc.ControllerBase controller)
        {
            return controller.Request.GetMiniAuthUser();
        }
        public static MiniAuthUser GetMiniAuthUser(this Microsoft.AspNetCore.Http.HttpRequest request)
        {
            var jwtManager = new JWTManager("miniauth", "miniauth", "miniauth.pfx");
            var token = string.Empty;
            if (request.Headers.ContainsKey("X-MiniAuth-Token"))
            {
                token = request.Headers["X-MiniAuth-Token"];
            }
            else if (request.Cookies.ContainsKey("X-MiniAuth-Token"))
            {
                token = request.Cookies["X-MiniAuth-Token"];
            }
            if (token != null)
            {
                var json = jwtManager.DecodeToken(token);
                if (json != null)
                {
                    var obj = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string,object>>(json);
                    var roles = ((JsonElement)obj["roles"]).Deserialize<string[]>();
                    var user = new MiniAuthUser
                    {
                        Id = obj["jti"].ToString(),
                        Username = obj["sub"].ToString(),
                        Roles = roles,
                        First_name = obj.ContainsKey("first_name") ? obj["first_name"]?.ToString() : null,
                        Last_name = obj.ContainsKey("last_name") ? obj["last_name"]?.ToString() : null,
                        Emp_no = obj.ContainsKey("emp_no") ? obj["emp_no"]?.ToString() : null,
                        Mail = obj.ContainsKey("mail") ? obj["mail"]?.ToString() : null,
                        Enable = true
                    };
                    return user;  
                }
            }

            return null;
        }
    }
}
