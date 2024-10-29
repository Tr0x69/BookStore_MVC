using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductRepository _product;
        private readonly ICategoryRepository _category;
        private readonly IWebHostEnvironment _webHostEnvironment; // use to access the wwwroot path folder to store static file. (We store image upload by user)
        public ProductController(IProductRepository product, ICategoryRepository category, IWebHostEnvironment webHostEnvironment)
        {
            _product = product;
            _category = category;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> list = _product.GetAll(includeProperties: "Category").ToList();
           
            return View(list);
        }

        public IActionResult Upsert(int? id)
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
            if (id == null || id == 0) 
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _product.Get(u => u.Id == id);
                return View(productVM);
            }
            
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file) //IFormFile use to handle file upload by user
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                { 
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName); //rename the file upload by user
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))// if the image is already there
                    {
                        //delete the old image
                        var oldIamgePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\')); // get the old path image

                        if (System.IO.File.Exists(oldIamgePath))
                        {
                            System.IO.File.Delete(oldIamgePath);
                        }
                    }
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName; //set the product image that a new file has been created
                }

                if(productVM.Product.Id == 0)
                {
                    _product.Add(productVM.Product);
                }else
                {
                    _product.Update(productVM.Product);
                }
                
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

        //public IActionResult Edit(int? id)
        //{
        //    if(id == null || id ==0)
        //    {
        //        return NotFound();
        //    }
        //    Product product = _product.Get(u=>u.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}


        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        _product.Update(obj);
        //        _product.Save();
        //        TempData["success"] = "Category updated succesfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product product = _product.Get(c => c.Id == id);
        //    if (product == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(product);
        //}
        //[HttpPost]
        //[ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    Product product = _product.Get(c => c.Id == id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    if (!string.IsNullOrEmpty(product.ImageUrl))// if the image is already there
        //    {
        //        //delete the old image
        //        var oldIamgePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\')); // get the old path image

        //        if (System.IO.File.Exists(oldIamgePath))
        //        {
        //            System.IO.File.Delete(oldIamgePath);
        //        }
        //    }


        //    _product.Remove(product);
        //    _product.Save();
        //    TempData["success"] = "Category deleted succesfully";
        //    return RedirectToAction("Index");
        //}


        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> list = _product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = list });
        }



        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var productDelete = _product.Get(c => c.Id == id);
            if (productDelete == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }


            if (!string.IsNullOrEmpty(productDelete.ImageUrl))// if the image is already there
            {
                //delete the old image
                var oldIamgePath = Path.Combine(_webHostEnvironment.WebRootPath, productDelete.ImageUrl.TrimStart('\\')); // get the old path image

                if (System.IO.File.Exists(oldIamgePath))
                {
                    System.IO.File.Delete(oldIamgePath);
                }
            }
            _product.Remove(productDelete);
            _product.Save();

            return Json(new { success = true, message = "Delete Successfull" });
        }

        #endregion
    }
}
