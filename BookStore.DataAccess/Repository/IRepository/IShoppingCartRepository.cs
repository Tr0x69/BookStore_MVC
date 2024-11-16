﻿using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IshoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart obj);

        void Save();
    }
}
