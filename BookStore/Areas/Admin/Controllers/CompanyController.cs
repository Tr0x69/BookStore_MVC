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
    public class CompanyController : Controller
    {
        private readonly ICompanyRepository _company;
        
        public CompanyController(ICompanyRepository company)
        {
            _company = company;
        }

        public IActionResult Index()
        {
            List<Company> list = _company.GetAll().ToList();
           
            return View(list);
        }

        public IActionResult Upsert(int? id)
        {

           
            if (id == null || id == 0) 
            {
                return View(new Company());
            }
            else
            {
                Company company = _company.Get(u => u.Id == id);
                return View(company);
            }
            
        }
        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
               

                if(company.Id == 0)
                {
                    _company.Add(company);
                }else
                {
                    _company.Update(company);
                }
                
                _company.Save();
                TempData["success"] = "Product Created succesfully";
                return RedirectToAction("Index");
            } else
            {
                
                    
              
                return View(company);

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
            List<Company> list = _company.GetAll().ToList();
            return Json(new { data = list });
        }



        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var companyDelete = _company.Get(c => c.Id == id);
            if (companyDelete == null)
            {
                return Json(new {success = false, message = "Error while deleting"});
            }


            _company.Remove(companyDelete);
            _company.Save();

            return Json(new { success = true, message = "Delete Successfull" });
        }

        #endregion
    }
}
