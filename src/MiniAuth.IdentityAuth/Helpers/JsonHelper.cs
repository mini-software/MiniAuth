using System.Text.Json;
using MiniAuth.Identity;

namespace MiniAuth.IdentityAuth.Helpers
{
    internal static class JsonHelper
    {
        public static string ToJson(this object data, int code = 200, string message = null)
        {
            return JsonSerializer.Serialize(new ResponseVo
            {
                code = code,
                message = message,
                data = data
            });
        }
        public static T GetProperty<T>(this JsonElement data, string key)
        {
            var obj = default(object);
            if (data.TryGetProperty(key, out JsonElement j))
                obj = j.Deserialize<T>();
            if (obj == null)
                return default;
            return (T)obj;
        }
    }
}
