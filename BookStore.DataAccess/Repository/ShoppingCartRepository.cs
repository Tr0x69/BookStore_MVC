﻿using BookStore.DataAccess.Data;
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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IshoppingCartRepository
    {
        private ApplicationDbContext _db;
       
        public ShoppingCartRepository(ApplicationDbContext db) : base(db) //Because the 2 Icategory and Repository that implment the ApplicationDB so we create the constructor and passit to the ihherit class
        {
            _db = db;
        } 

        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
    }
}
