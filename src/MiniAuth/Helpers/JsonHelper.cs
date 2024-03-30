using MiniAuth.Models;

namespace MiniAuth.Helpers
{
    internal static class IdHelper
    {
        public static string NewId()
        {
            return System.Guid.NewGuid().ToString();
        }
    }
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
    }
}
