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
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
       
        public ApplicationUserRepository(ApplicationDbContext db) : base(db) //Because the 2 Icategory and Repository that implment the ApplicationDB so we create the constructor and passit to the ihherit class
        {
            _db = db;
        } 

      
    }
}
