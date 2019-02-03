using BooksShop.Model;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace BooksShop.DataLayer
{
    public class BooksRepository
    {
        private readonly string ConnectionString;
        public BooksRepository(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public IEnumerable<Book> GetBooks()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select ISBN, Name, Author, PublishingYear, Price, Count from Books "+
                        "where Count>0";
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            yield return new Book
                            {
                                ISBNCode = reader.GetString(reader.GetOrdinal("ISBN")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Author = reader.GetString(reader.GetOrdinal("Author")),
                                PublishingYear = reader.GetInt32(reader.GetOrdinal("PublishingYear")),
                                Price = reader.GetInt32(reader.GetOrdinal("Price")),
                                Count = reader.GetInt32(reader.GetOrdinal("Count"))
                            };
                        }
                    }
                }
            }
        }
    }
}