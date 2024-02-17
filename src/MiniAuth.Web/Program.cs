using Microsoft.AspNetCore.Mvc;

namespace MiniAuth.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var service = builder.Services;
            service.AddControllers();
            var app = builder.Build();
            app.UseMiniAuth();
            app.MapControllers();
            app.MapGet("/", () => "Hello MiniAuth!");
            app.Run();
        }
    }

    [ApiController]
    [Route("[controller]/[action]")]
    public class DemoController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<dynamic> Api1()
        {
            return new[] { new { id = 1 } };
        }
        [HttpPost]
        public ActionResult Api2() => Content("Api2");
    }

    [ApiController]
    [Route("[controller]")]
    public class Demo2Controller : ControllerBase
    {
        [HttpGet("Api1")]
        public ActionResult Api1() => Content("Api1");
        public ActionResult Api2() => Content("Api2");
    }
}
