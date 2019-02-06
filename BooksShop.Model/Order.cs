using System;
using System.Collections.Generic;
namespace BooksShop.Model
{
    public class Order
    {
        public Guid PromoCode;
        public IEnumerable<OrderedBook> Books;
        public enum StatusEnum
        {
            Forming,
            Ordered,
            Complete
        }
        public StatusEnum Status;
        public static int MinCost = 2000;
    }
}