using BooksShop.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace BooksShop.DataLayer.Tests
{
    [TestClass]
    public class BooksRepositoryTest
    {
        private readonly List<string> TempBooks = new List<string>();
        private const string ConnectionString = "Data Source=localhost;Database=BooksShop;Integrated Security=True";
        private readonly BooksRepository BooksRepository = new BooksRepository(ConnectionString);
        [TestMethod]
        public void GetBooksList()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "insert into Books (ISBN, Name, Author, PublishingYear, Price, Count) "+
                        "values('test', 'test', 'test', 1, 1, 1)";
                    command.ExecuteNonQuery();
                }
            }
            TempBooks.Add("test");
            var booksList = new List<Book>(BooksRepository.GetBooks());
            var book = booksList.Find(x => x.ISBNCode == "test");
            Assert.AreEqual(book.ISBNCode, "test");
            Assert.AreEqual(book.Name, "test");
            Assert.AreEqual(book.Author, "test");
            Assert.AreEqual(book.PublishingYear, 1);
            Assert.AreEqual(book.Price, 1);
            Assert.AreEqual(book.Count, 1);
        }
        [TestCleanup]
        public void Clean()
        {
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