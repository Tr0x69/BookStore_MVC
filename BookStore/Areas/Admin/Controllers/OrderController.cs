using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Climate;
using System.Security.Claims;

namespace BookStore.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IOrderHeaderRepositorpy _orderHeaderRepositorpy;
		private readonly IOrderDetailRepository _orderDetailRepository;
		[BindProperty]
		public OrderVM OrderVM { get; set; }

        public OrderController(IOrderHeaderRepositorpy ctx, IOrderDetailRepository orderDetail)
        {
			_orderHeaderRepositorpy = ctx;
			_orderDetailRepository = orderDetail;
        }
        public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int orderId)
		{
            var orderHeader = _orderHeaderRepositorpy.Get(u => u.Id == orderId, includeProperties: "ApplicationUser");

            if (orderHeader == null)
            {
           
                return NotFound(); 
            }

            OrderVM = new()
			{
                orderHeeader = orderHeader,
				orderDetail = _orderDetailRepository.GetAll(u=>u.OrderHeaderId==orderId, includeProperties: "Product")

            };

			return View(OrderVM);
		}

		[Authorize(Roles =SD.Role_Admin)]
		[HttpPost]
        public IActionResult UpdateOrderDetail()
        {
			var orderHeader = _orderHeaderRepositorpy.Get(u => u.Id == OrderVM.orderHeeader.Id);
            orderHeader.Name = OrderVM.orderHeeader.Name;
			orderHeader.PhoneNumber = OrderVM.orderHeeader.PhoneNumber;
			orderHeader.StreetAddress = OrderVM.orderHeeader.StreetAddress;
			orderHeader.City = OrderVM.orderHeeader.City;
			orderHeader.State = OrderVM.orderHeeader.State;
			orderHeader.PostalCode = OrderVM.orderHeeader.PostalCode;


			_orderHeaderRepositorpy.Update(orderHeader);
			_orderHeaderRepositorpy.Save();
			TempData["Success"] = "Order Details Updated Successfully.";

            return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id});
        }



		public IActionResult CancelOrder()
		{
			var orderHeader = _orderHeaderRepositorpy.Get(u => u.Id == OrderVM.orderHeeader.Id);

			if (orderHeader.PaymentStatus == SD.StatusApproved) {
				var options = new RefundCreateOptions 
					{
						Reason = RefundReasons.RequestedByCustomer,
						PaymentIntent = orderHeader.PaymentIntentId
					};


                var service = new RefundService();
                Refund refund = service.Create(options);

				_orderHeaderRepositorpy.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            } else
			{
                _orderHeaderRepositorpy.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
			_orderHeaderRepositorpy.Save();
            TempData["Success"] = "Order Cancelled Successfully.";
			return RedirectToAction(nameof(Details), new { orderId = orderHeader.Id});


        

		}




        #region

        [HttpGet]
		public IActionResult GetAll()
		{
			IEnumerable<OrderHeader> orderHeaders; 

			if(User.IsInRole(SD.Role_Admin))
			{
				orderHeaders = _orderHeaderRepositorpy.GetAll(includeProperties: "ApplicationUser").ToList();
            } else
			{
                var claimIdenity = (ClaimsIdentity)User.Identity;
                var userId = claimIdenity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = _orderHeaderRepositorpy.GetAll(u => u.ApplicationUserId == userId,includeProperties: "ApplicationUser").ToList();
            }

			return Json(new { data = orderHeaders });
		}

		#endregion
	}
}
