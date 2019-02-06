using BooksShop.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace BooksShop.DataLayer.Tests
{
    [TestClass]
    public class OrdersRepositoryTests
    {
        private readonly List<string> TempBooks = new List<string>();
        private readonly List<Guid> TempOrders = new List<Guid>();
        private const string ConnectionString = "Data Source=localhost;Database=BooksShop;Integrated Security=True";
        private readonly OrdersRepository OrdersRepository = new OrdersRepository(ConnectionString);
        private void DeleteOrder(Guid promoCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "delete from OrderToBook where PromoCode=@PromoCode";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    command.ExecuteNonQuery();
                }
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "delete from Orders where PromoCode=@PromoCode";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    command.ExecuteNonQuery();
                }
            }
        }
        [TestMethod]
        public void IsOrderExist()
        {
            var order = OrdersRepository.CreateOrder();
            Assert.IsTrue(OrdersRepository.IsOrderExist(order));
            DeleteOrder(order);
            Assert.IsFalse(OrdersRepository.IsOrderExist(order));
        }
        private void AddBookToShop(string ISBNCode, int price)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "insert into Books (ISBN, Name, Author, PublishingYear, Price, Count) " +
                        "values(@ISBN, 'test', 'test', 1, @Price, 1)";
                    command.Parameters.AddWithValue("@ISBN", ISBNCode);
                    command.Parameters.AddWithValue("@Price", price);
                    command.ExecuteNonQuery();
                }
            }
        }
        [TestMethod]
        public void Cost()
        {
            var order = OrdersRepository.CreateOrder();
            TempOrders.Add(order);
            AddBookToShop("test1", 1);
            TempBooks.Add("test1");
            AddBookToShop("test2", 2);
            TempBooks.Add("test2");
            OrdersRepository.AddBooks(order, "test1");
            OrdersRepository.AddBooks(order, "test2");
            Assert.AreEqual(OrdersRepository.Cost(order), 3);
        }
        [TestMethod]
        public void ChangeStatus()
        {
            var order = OrdersRepository.CreateOrder();
            TempOrders.Add(order);
            Assert.AreEqual(OrdersRepository.GetStatus(order), Order.StatusEnum.Forming);
            AddBookToShop("test", Order.MinCost);
            TempBooks.Add("test");
            OrdersRepository.AddBooks(order, "test");
            OrdersRepository.MakeOrder(order);
            Assert.AreEqual(OrdersRepository.GetStatus(order), Order.StatusEnum.Ordered);
            OrdersRepository.CompleteOrder(order);
            Assert.AreEqual(OrdersRepository.GetStatus(order), Order.StatusEnum.Complete);
        }
        [TestMethod]
        public void GetBooks()
        {
            var order = OrdersRepository.CreateOrder();
            TempOrders.Add(order);
            AddBookToShop("test", 1);
            TempBooks.Add("test");
            OrdersRepository.AddBooks(order, "test");
            Assert.AreEqual(OrdersRepository.GetBooks(order).Single().ISBNCode, "test");
        }
        [TestMethod]
        public void AddAndDeleteBookToOrder()
        {
            var order = OrdersRepository.CreateOrder();
            TempOrders.Add(order);
            AddBookToShop("test", 1);
            TempBooks.Add("test");
            Assert.AreEqual(OrdersRepository.GetShopBookCount("test"), 1);
            Assert.AreEqual(OrdersRepository.GetOrderBookCount(order, "test"), 0);
            OrdersRepository.AddBooks(order, "test");
            Assert.AreEqual(OrdersRepository.GetShopBookCount("test"), 0);
            Assert.AreEqual(OrdersRepository.GetOrderBookCount(order, "test"), 1);
            OrdersRepository.DeleteBooks(order, "test");
            Assert.AreEqual(OrdersRepository.GetShopBookCount("test"), 1);
            Assert.AreEqual(OrdersRepository.GetOrderBookCount(order, "test"), 0);
        }
        [TestMethod]
        public void IsBookExistInOrder()
        {
            var order = OrdersRepository.CreateOrder();
            TempOrders.Add(order);
            AddBookToShop("test", 1);
            TempBooks.Add("test");
            Assert.IsFalse(OrdersRepository.IsBookExistInOrder(order, "test"));
            OrdersRepository.AddBooks(order, "test");
            Assert.IsTrue(OrdersRepository.IsBookExistInOrder(order, "test"));
        }
        [TestCleanup]
        public void Clean()
        {
            foreach (var order in TempOrders)
                DeleteOrder(order);
            foreach (var book in TempBooks)
            {
                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "delete from Books where ISBN=@ISBN";
                        command.Parameters.AddWithValue("@ISBN", book);
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}