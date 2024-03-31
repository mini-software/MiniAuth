using MiniAuth.Models;
using System.Text.Json;

namespace MiniAuth.Helpers
{
    internal static class JsonHelper
    {
        public static string ToJson(this object data, int code = 200, string message = null)
        {
            return System.Text.Json.JsonSerializer.Serialize(new ResponseVo
            {
                code = code,
                message = message,
                data = data
            });
        }
        public static T GetProperty<T>(this JsonElement data,string key) 
        {
            var obj= default(object);
            if(data.TryGetProperty(key,out JsonElement j))
                obj = j.Deserialize<T>();
            if(obj == null)
                return default(T);
            return (T)obj;
        }
    }
}
