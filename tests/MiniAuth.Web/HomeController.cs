using Microsoft.AspNetCore.Mvc;

namespace MiniAuth.Web
{
    public class HomeController : Controller
    {
        [Route("/")]
        public ActionResult Home() => Content("This's homepage");
        [HttpGet]
        [Route("/About")]
        public ActionResult About() => Content("This's About");
        [Route("/UserInfo")]
        public ActionResult UserInfo()
        {
            var user = MiniAuth.Helpers.HttpRequestHelper.GetMiniAuthUser(this);
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
    public class ProductStockController : ControllerBase
    {
        private static List<ProductStock> products = new List<ProductStock>
        {
            new ProductStock { Id = 1, Name = "Apple", Stock = 10 },
            new ProductStock { Id = 2, Name = "Orange", Stock = 5 }
        };

        [HttpGet]
        public ActionResult<IEnumerable<ProductStock>> GetAll()
        {
            return Ok(products);
        }

        [HttpGet("{id}")]
        public ActionResult<ProductStock> GetById(int id)
        {
            var product = products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public ActionResult<ProductStock> Create(ProductStock product)
        {
            product.Id = products.Count + 1;
            products.Add(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, ProductStock updatedProduct)
        {
            var product = products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            product.Name = updatedProduct.Name;
            product.Stock = updatedProduct.Stock;
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

        public class ProductStock
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int Stock { get; set; }
        }
    }
}
