using Microsoft.AspNetCore.Mvc;
using MiniAuth.Configs;
using MiniAuth.Helpers;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace MiniAuth.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
            builder.Services.AddControllers();
            //builder.Services.AddSingleton<MiniAuthOptions>(new MiniAuthOptions {ExpirationMinuteTime=12*24*60 });
            //builder.Services.AddSingleton<IMiniAuthDB>(
            //     new MiniAuthDB<System.Data.SqlClient.SqlConnection>("Data Source=(localdb)\\MSSQLLocalDB;Integrated Security=SSPI;Initial Catalog=miniauth;app=MiniAuth")
            //);
            var app = builder.Build();
            app.UseCors("AllowAll");
            app.UseStaticFiles();
            app.UseMiniAuth();
            app.MapControllers();
            app.MapGet("/miniapi/get", () => "Hello MiniAuth!");



            app.Run();
        }
    }

    public class HomeController : Controller
    {
        [HttpGet]
        [HttpPost]
        [Route("/")]
        public ActionResult Home() => Content("This's homepage");
        [HttpGet]
        [Route("/About")]
        public ActionResult About() => Content("This's About");
        [Route("/UserInfo")]
        public ActionResult UserInfo()
        {
            var user = this.GetMiniAuthUser();
            return Json(user);
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private static List<Order> orders = new List<Order>
        {
            new Order { Id = 1, Product = "Apple", Quantity = 2 },
            new Order { Id = 2, Product = "Orange", Quantity = 3 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Order>> GetAll()
        {
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public ActionResult<Order> GetById(int id)
        {
            var order = orders.Find(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(order);
        }

        [HttpPost]
        public ActionResult<Order> Create(Order order)
        {
            order.Id = orders.Count + 1;
            orders.Add(order);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Order updatedOrder)
        {
            var order = orders.Find(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            order.Product = updatedOrder.Product;
            order.Quantity = updatedOrder.Quantity;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var order = orders.Find(o => o.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            orders.Remove(order);
            return NoContent();
        }

        public class Order
        {
            public int Id { get; set; }
            public string Product { get; set; }
            public int Quantity { get; set; }
        }
    }



    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private static List<Product> products = new List<Product>
        {
            new Product { Id = 1, Name = "Apple", Price = 1.99 },
            new Product { Id = 2, Name = "Orange", Price = 0.99 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Product>> GetAll()
        {
            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<Product> GetById(int id)
        {
            var product = products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public ActionResult<Product> Create(Product product)
        {
            product.Id = products.Count + 1;
            products.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Product updatedProduct)
        {
            var product = products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            product.Name = updatedProduct.Name;
            product.Price = updatedProduct.Price;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            products.Remove(product);
            return NoContent();
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private static List<Stock> stocks = new List<Stock>
        {
            new Stock { Id = 1, Symbol = "AAPL", Price = 150.50 },
            new Stock { Id = 2, Symbol = "GOOGL", Price = 2500.75 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<Stock>> GetAll()
        {
            return Ok(stocks);
        }

        [HttpGet("{id}")]
        public ActionResult<Stock> GetById(int id)
        {
            var stock = stocks.Find(s => s.Id == id);
            if (stock == null)
            {
                return NotFound();
            }
            return Ok(stock);
        }

        [HttpPost]
        public ActionResult<Stock> Create(Stock stock)
        {
            stock.Id = stocks.Count + 1;
            stocks.Add(stock);
            return CreatedAtAction(nameof(GetById), new { id = stock.Id }, stock);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, Stock updatedStock)
        {
            var stock = stocks.Find(s => s.Id == id);
            if (stock == null)
            {
                return NotFound();
            }
            stock.Symbol = updatedStock.Symbol;
            stock.Price = updatedStock.Price;
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var stock = stocks.Find(s => s.Id == id);
            if (stock == null)
            {
                return NotFound();
            }
            stocks.Remove(stock);
            return NoContent();
        }

        public class Stock
        {
            public int Id { get; set; }
            public string Symbol { get; set; }
            public double Price { get; set; }
        }
    }
}
