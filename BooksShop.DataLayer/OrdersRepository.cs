using BooksShop.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace BooksShop.DataLayer
{
    public class OrdersRepository
    {
        private readonly string ConnectionString;
        public OrdersRepository(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public Guid CreateOrder()
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "insert into Orders (PromoCode, StatusID) values(@PromoCode, @StatusID)";
                    var promoCode = Guid.NewGuid();
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    command.Parameters.AddWithValue("@StatusID", (int)Order.StatusEnum.Forming);
                    command.ExecuteNonQuery();
                    return promoCode;
                }
            }
        }
        public bool IsOrderExist(Guid promoCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select count(*) from Orders where PromoCode=@PromoCode";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    return (int)command.ExecuteScalar()>0;
                }
            }
        }
        public int Cost(Guid promoCode)
        {
            if (!IsOrderExist(promoCode))
                throw new ArgumentException($"Заказ {promoCode} не существует");
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select sum(OrderToBook.Count*Books.Price) " +
                        "from OrderToBook inner join Books on OrderToBook.ISBN=Books.ISBN where PromoCode=@PromoCode";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    var result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : (int)result;
                }
            };
        }
        public Order.StatusEnum GetStatus(Guid promoCode)
        {
            if (!IsOrderExist(promoCode))
                throw new ArgumentException($"Заказ {promoCode} не существует");
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select top(1) StatusID from Orders where PromoCode=@PromoCode";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    return (Order.StatusEnum)command.ExecuteScalar();
                }
            }
        }
        private void SetStatus(Guid promoCode, Order.StatusEnum status)
        {
            if (!IsOrderExist(promoCode))
                throw new ArgumentException($"Заказ {promoCode} не существует");
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "update Orders set StatusID=@StatusID where PromoCode=@PromoCode";
                    command.Parameters.AddWithValue("@StatusID", status);
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    command.ExecuteNonQuery();
                }
            }
        }
        public void MakeOrder(Guid promoCode)
        {
            if (GetStatus(promoCode) != Order.StatusEnum.Forming)
                throw new ArgumentException($"Заказ {promoCode} уже был оформлен");
            if (Cost(promoCode)<Order.MinCost)
                throw new ArgumentException($"Минимальная стоимость заказа: {Order.MinCost} рублей");
            SetStatus(promoCode, Order.StatusEnum.Ordered);
        }
        public void CompleteOrder(Guid promoCode)
        {
            if (GetStatus(promoCode) == Order.StatusEnum.Forming)
                throw new ArgumentException($"Заказ {promoCode} не может быть выполнен, так как не был оформлен");
            else if (GetStatus(promoCode) == Order.StatusEnum.Complete)
                throw new ArgumentException($"Заказ {promoCode} уже был выполнен");
            else if (GetStatus(promoCode) != Order.StatusEnum.Ordered)
                throw new ArgumentException($"Заказ {promoCode} обладает неизвестным статусом");
            SetStatus(promoCode, Order.StatusEnum.Complete);
        }
        public IEnumerable<Book> GetBooks(Guid promoCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select Books.ISBN, Books.Name, Books.Author, Books.PublishingYear, "+
                        "Books.Price, OrderToBook.Count "+
                        "from Books inner join OrderToBook on Books.ISBN=OrderToBook.ISBN "+
                        "where PromoCode=@PromoCode and OrderToBook.Count>0";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
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
        public int GetShopBookCount(string ISBNCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select Count from Books where ISBN=@ISBN";
                    command.Parameters.AddWithValue("@ISBN", ISBNCode);
                    if (command.ExecuteScalar() == null)
                        throw new ArgumentException($"Не найдено книги с ISBN кодом {ISBNCode}");
                    return (int)command.ExecuteScalar();
                }
            }
        }
        public int GetOrderBookCount(Guid promoCode, string ISBNCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select Count from OrderToBook where PromoCode=@PromoCode and ISBN=@ISBN";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    command.Parameters.AddWithValue("@ISBN", ISBNCode);
                    var result = command.ExecuteScalar();
                    return result == null ? 0 : (int)result;
                }
            }
        }
        public bool IsBookExistInOrder(Guid promoCode, string ISBNCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select count(*) FROM OrderToBook where PromoCode=@PromoCode and ISBN=@ISBN";
                    command.Parameters.AddWithValue("@PromoCode", promoCode);
                    command.Parameters.AddWithValue("@ISBN", ISBNCode);
                    return (int)command.ExecuteScalar()>0;
                }
            }
        }
        public bool IsBookExistInShop(string ISBNCode)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select count(*) FROM Books where ISBN=@ISBN";
                    command.Parameters.AddWithValue("@ISBN", ISBNCode);
                    return (int)command.ExecuteScalar() > 0;
                }
            }
        }
        public void AddBooks(Guid promoCode, string ISBNCode, int count=1)
        {
            if (!IsOrderExist(promoCode))
                throw new ArgumentException($"Заказ {promoCode} не существует");
            if (GetStatus(promoCode) != Order.StatusEnum.Forming)
                throw new ArgumentException($"Заказ {promoCode} невозможно изменить, так как он уже был оформлен");
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    int shopBookCount = GetShopBookCount(ISBNCode);
                    if (shopBookCount < count)
                        throw new ArgumentException($"Недостаточно книг с ISBN кодом {ISBNCode}");
                    if(GetOrderBookCount(promoCode, ISBNCode)>=1)
                        throw new ArgumentException("В корзину можно положить только один экземпдяр " +
                            "одной и той же книги");
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "update Books set Count = Count - @Count where ISBN=@ISBN";
                        command.Parameters.AddWithValue("@Count", count);
                        command.Parameters.AddWithValue("@ISBN", ISBNCode);
                        if(command.ExecuteNonQuery()==0)
                            throw new ArgumentException($"Не найдено книги с ISBN кодом {ISBNCode}");
                    }
                    bool isBookExistInOrder = IsBookExistInOrder(promoCode, ISBNCode);
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        if(isBookExistInOrder)
                            command.CommandText = "update OrderToBook set Count = Count + @Count "+
                                "where PromoCode=@PromoCode and ISBN=@ISBN";
                        else
                            command.CommandText = "insert into OrderToBook (PromoCode, ISBN, Count) "+
                                "values (@PromoCode, @ISBN, @Count)";
                        command.Parameters.AddWithValue("@Count", count);
                        command.Parameters.AddWithValue("@PromoCode", promoCode);
                        command.Parameters.AddWithValue("@ISBN", ISBNCode);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
        public void DeleteBooks(Guid promoCode, string ISBNCode, int count = 1)
        {
            if (!IsOrderExist(promoCode))
                throw new ArgumentException($"Заказ {promoCode} не существует");
            if (GetStatus(promoCode) != Order.StatusEnum.Forming)
                throw new ArgumentException($"Заказ {promoCode} невозможно изменить, так как он уже был оформлен");
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    int orderBookCount = GetOrderBookCount(promoCode, ISBNCode);
                    if (orderBookCount < count)
                        throw new ArgumentException($"Недостаточно книг с ISBN кодом {ISBNCode} в заказе {promoCode}");
                    else if (orderBookCount == count)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "delete from OrderToBook where PromoCode=@PromoCode and ISBN=@ISBN";
                            command.Parameters.AddWithValue("@PromoCode", promoCode);
                            command.Parameters.AddWithValue("@ISBN", ISBNCode);
                            if (command.ExecuteNonQuery() == 0)
                                throw new ArgumentException($"Не найдено книги с ISBN кодом {ISBNCode} в заказе {promoCode}");
                        }
                    }
                    else
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = "update OrderToBook set Count = Count - @Count " +
                                "where PromoCode=@PromoCode and ISBN=@ISBN";
                            command.Parameters.AddWithValue("@Count", count);
                            command.Parameters.AddWithValue("@PromoCode", promoCode);
                            command.Parameters.AddWithValue("@ISBN", ISBNCode);
                            if (command.ExecuteNonQuery() == 0)
                                throw new ArgumentException($"Не найдено книги с ISBN кодом {ISBNCode} в заказе {promoCode}");
                        }
                    }
                    if(!IsBookExistInShop(ISBNCode))
                        throw new ArgumentException($"Книги с ISBN кодом {ISBNCode} не существует");
                    using (var command = connection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = "update Books set Count = Count + @Count where ISBN=@ISBN";
                        command.Parameters.AddWithValue("@Count", count);
                        command.Parameters.AddWithValue("@ISBN", ISBNCode);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
            }
        }
    }
}