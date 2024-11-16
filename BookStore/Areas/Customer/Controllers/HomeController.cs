using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using System.Diagnostics;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IProductRepository _product;
        private readonly ICategoryRepository _category;
        private readonly IshoppingCartRepository _shoppingCart;

        public HomeController( IProductRepository product, ICategoryRepository category, IshoppingCartRepository shoppingCart)
        {

            _product = product;
            _category = category;
            _shoppingCart = shoppingCart;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _product.GetAll(includeProperties: "Category");
            return View(productList);
        }


        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _product.Get(u => u.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProdictId = productId
            };
            
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimIdenity = (ClaimsIdentity)User.Identity;
            var userId = claimIdenity.FindFirst(ClaimTypes.NameIdentifier).Value;
            
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _shoppingCart.Get(u => u.ApplicationUserId == shoppingCart.ApplicationUserId && u.ProdictId == shoppingCart.ProdictId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _shoppingCart.Update(cartFromDb);
            } else
            {
                _shoppingCart.Add(shoppingCart);
            }


            TempData["success"] = "Cart updated successfully";

            //var uss = User.FindFirstValue(ClaimTypes.NameIdentifier); alternative

          
            _shoppingCart.Save();
            return RedirectToAction("Index");
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
