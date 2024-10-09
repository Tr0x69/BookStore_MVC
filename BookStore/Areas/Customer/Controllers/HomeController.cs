using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IProductRepository _product;
        private readonly ICategoryRepository _category;

        public HomeController( IProductRepository product, ICategoryRepository category)
        {

            _product = product;
            _category = category;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _product.GetAll(includeProperties: "Category");
            return View(productList);
        }


        public IActionResult Details(int? id)
        {
            Product product = _product.Get(u => u.Id == id,includeProperties: "Category");
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
