using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepositorpy
    {
        private ApplicationDbContext _db;
       
        public OrderHeaderRepository(ApplicationDbContext db) : base(db) //Because the 2 Icategory and Repository that implment the ApplicationDB so we create the constructor and passit to the ihherit class
        {
            _db = db;
        } 

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(OrderHeader obj)
        {
            _db.orderHeaders.Update(obj);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var order = _db.orderHeaders.FirstOrDefault(u=> u.Id == id);
            if (order != null)
            {
                order.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    order.PaymentStatus = paymentStatus;
                }
            }
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntendId) // paymentIntendId only assigned if the payment was successfull
		{
			var order = _db.orderHeaders.FirstOrDefault(u => u.Id == id);
            if (!string.IsNullOrEmpty(sessionId))
            {
               order.SessionId= sessionId;
            }
			if (!string.IsNullOrEmpty(paymentIntendId))
			{
				order.PaymentIntentId = paymentIntendId;
                order.PaymentDate = DateTime.Now;
			}


		}
	}
}
