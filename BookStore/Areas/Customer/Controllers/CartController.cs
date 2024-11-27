using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BookStore.Areas.Customer.Controllers
{
	[Area("customer")]
	[Authorize]
	public class CartController : Controller
	{
		private readonly IshoppingCartRepository _ishoppingCartRepository;
		private readonly IApplicationUserRepository _userRepository;
		private readonly IOrderDetailRepository _orderDetailRepository;
		private readonly IOrderHeaderRepositorpy _orderHeaderRepository;


		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public CartController(IshoppingCartRepository context, IApplicationUserRepository user, IOrderHeaderRepositorpy orderHeader, IOrderDetailRepository orderDetail)
		{
			_ishoppingCartRepository = context;
			_userRepository = user;
			_orderHeaderRepository = orderHeader;
			_orderDetailRepository = orderDetail;
		}
		public IActionResult Index()
		{
			var claimIdenity = (ClaimsIdentity)User.Identity;
			var userId = claimIdenity.FindFirst(ClaimTypes.NameIdentifier).Value;


			ShoppingCartVM = new()
			{
				ShoppingCartList = _ishoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()
			};

			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuanity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}


			return View(ShoppingCartVM);
		}



		public IActionResult Summary()
		{
			var claimIdenity = (ClaimsIdentity)User.Identity;
			var userId = claimIdenity.FindFirst(ClaimTypes.NameIdentifier).Value;


			ShoppingCartVM = new()
			{
				ShoppingCartList = _ishoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVM.OrderHeader.ApplicationUser = _userRepository.Get(u => u.Id == userId);

			ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
			ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
			ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
			ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;



			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuanity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}



			return View(ShoppingCartVM);

		}



		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPost()
		{

			var claimIdenity = (ClaimsIdentity)User.Identity;
			var userId = claimIdenity.FindFirst(ClaimTypes.NameIdentifier).Value;


			ShoppingCartVM.ShoppingCartList = _ishoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
			ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
			ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


			ApplicationUser applicationUser = _userRepository.Get(u => u.Id == userId);





			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuanity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
			ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;


			_orderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
			_orderHeaderRepository.Save();



			//this is a regular customer account
			//capture payment
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{

				foreach (var cart in ShoppingCartVM.ShoppingCartList)
				{
					OrderDetail orderDetail = new()
					{
						ProductId = cart.ProdictId,
						OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
						Price = cart.Price,
						Count = cart.Count,
					};
					_orderDetailRepository.Add(orderDetail);
					_orderDetailRepository.Save();
				}

				var domain = "https://localhost:7221/";
				var options = new Stripe.Checkout.SessionCreateOptions
				{
					SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
					CancelUrl = domain + "customer/cart/index",
					LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
					Mode = "payment",
				};

				foreach (var item in ShoppingCartVM.ShoppingCartList)
				{
					var sessionLineItem = new SessionLineItemOptions
					{
						PriceData = new SessionLineItemPriceDataOptions
						{
							UnitAmount = (long)(item.Price * 100),
							Currency = "usd",
							ProductData = new SessionLineItemPriceDataProductDataOptions
							{
								Name = item.Product.Title
							}
						},
						Quantity = item.Count
					};
					options.LineItems.Add(sessionLineItem);
				}

				var service = new Stripe.Checkout.SessionService();
				Session session = service.Create(options);

				_orderHeaderRepository.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_orderHeaderRepository.Save();
				Response.Headers.Add("Location", session.Url);
				return new StatusCodeResult(303);

			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });

		}


		public IActionResult OrderConfirmation(int id)
		{
			OrderHeader order = _orderHeaderRepository.Get(u => u.Id == id, includeProperties: "ApplicationUser");

			var service = new SessionService();
			Session session = service.Get(order.SessionId);

			if (session.PaymentStatus.ToLower() == "paid") //if succesfull, stripe will assign a paymentIntentId
			{
				_orderHeaderRepository.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
				_orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PatmentStatusApproved);
				_orderHeaderRepository.Save();

			}

			List<ShoppingCart> shoppingCarts = _ishoppingCartRepository.GetAll(u => u.ApplicationUserId == order.ApplicationUserId).ToList();
			_ishoppingCartRepository.RemoveRange(shoppingCarts);
			_ishoppingCartRepository.Save();

			return View(id);
		}





		public IActionResult Plus(int cartId)
		{
			var cartFromDb = _ishoppingCartRepository.Get(u => u.Id == cartId);

			cartFromDb.Count += 1;
			_ishoppingCartRepository.Update(cartFromDb);
			_ishoppingCartRepository.Save();
			return RedirectToAction(nameof(Index));
		}


		public IActionResult Minus(int cartId)
		{
			var cartFromDb = _ishoppingCartRepository.Get(u => u.Id == cartId);
			if (cartFromDb.Count <= 1)
			{
				_ishoppingCartRepository.Remove(cartFromDb);

			}
			else
			{
				cartFromDb.Count -= 1;
				_ishoppingCartRepository.Update(cartFromDb);
			}



			_ishoppingCartRepository.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cartFromDb = _ishoppingCartRepository.Get(u => u.Id == cartId);


			_ishoppingCartRepository.Remove(cartFromDb);
			_ishoppingCartRepository.Save();
			return RedirectToAction(nameof(Index));
		}






		private double GetPriceBasedOnQuanity(ShoppingCart shoppingCart)
		{
			if (shoppingCart.Count <= 50)
			{

				return shoppingCart.Product.Price;
			}
			else
			{
				if (shoppingCart.Count <= 100)
				{
					return shoppingCart.Product.Price50;
				}
				else
				{
					return shoppingCart.Product.Price100;
				}
			}
		}




	}
}
