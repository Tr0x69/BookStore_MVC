﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModels
{
	public class OrderVM
	{
		public OrderHeader orderHeeader {  get; set; }

		public IEnumerable<OrderDetail> orderDetail { get; set; }
	}
}
