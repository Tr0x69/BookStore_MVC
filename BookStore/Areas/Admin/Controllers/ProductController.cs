using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IProductRepository _product;
        private readonly ICategoryRepository _category;
        public ProductController(IProductRepository product, ICategoryRepository category)
        {
            _product = product;
            _category = category;
        }

        public IActionResult Index()
        {
            List<Product> list = _product.GetAll().ToList();
           
            return View(list);
        }

        public IActionResult Create()
        {

            //Left CategoryList is a key and we can name whatever we want. Left CategoryList is the value
            //ViewBag.categoryList = categoryList;
            ProductVM productVM = new()
            {
                CategoryList = _category.GetAll().Select(u => new SelectListItem //because we use the SelectListItem type so
                                                                                 //we have to convert each category to selectlist type
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Create(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                _product.Add(productVM.Product);
                _product.Save();
                TempData["success"] = "Product Created succesfully";
                return RedirectToAction("Index");
            } else
            {
                productVM.CategoryList = _category.GetAll().Select(u => new SelectListItem //because we use the SelectListItem type so
                                                                                     //we have to convert each category to selectlist type
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                    
              
                return View(productVM);

            }
            
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id ==0)
            {
                return NotFound();
            }
            Product product = _product.Get(u=>u.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }


        [HttpPost]
        public IActionResult Edit(Product obj)
        {
            if (ModelState.IsValid)
            {

                _product.Update(obj);
                _product.Save();
                TempData["success"] = "Category updated succesfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _product.Get(c => c.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            Product product = _product.Get(c => c.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _product.Remove(product);
            _product.Save();
            TempData["success"] = "Category deleted succesfully";
            return RedirectToAction("Index");
        }
    }
}
