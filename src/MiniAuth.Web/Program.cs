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
        public Demo2Controller()
        {
            var t = this;
        }
        [HttpGet("Api1")]
        public IEnumerable<dynamic> Api1()
        {
            return Enumerable.Range(1, 5).Select(index => new
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                //Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
.ToArray();
        }
        public ActionResult Api2() => Content("Api2");
    }
}
