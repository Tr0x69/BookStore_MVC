using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class OrderController : Controller
	{
		private readonly IOrderHeaderRepositorpy _orderHeaderRepositorpy;
		private readonly IOrderDetailRepository _orderDetailRepository;
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
			OrderVM orderVM = new()
			{
                orderHeeader = _orderHeaderRepositorpy.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
				orderDetail = _orderDetailRepository.GetAll(u=>u.OrderHeaderId==orderId, includeProperties: "Product")

            };

			return View(orderVM);
		}



		#region

		[HttpGet]
		public IActionResult GetAll()
		{
			List<OrderHeader> orderHeaders = _orderHeaderRepositorpy.GetAll(includeProperties: "ApplicationUser").ToList();
			return Json(new { data = orderHeaders });
		}

		#endregion
	}
}
