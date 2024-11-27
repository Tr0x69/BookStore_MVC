using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepositorpy : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);


        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

		void UpdateStripePaymentID(int id, string sessionId, string paymentIntendId);


		void Save();
    }
}
