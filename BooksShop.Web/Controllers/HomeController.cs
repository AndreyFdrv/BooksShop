using BooksShop.DataLayer;
using System;
using System.Web.Mvc;
using System.Configuration;

namespace BooksShop.Web.Controllers
{
    public class HomeController : Controller
    {
        public static string ConnectionString;
        BooksRepository BooksRepository;
        OrdersRepository OrdersRepository;
        public HomeController()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            BooksRepository = new BooksRepository(ConnectionString);
            OrdersRepository = new OrdersRepository(ConnectionString);
        }
        public ActionResult Index()
        {
            Update();
            return View();
        }
        private void Update()
        {
            var promoCode = (Guid)Session["PromoCode"];
            ViewBag.PromoCode = promoCode;
            ViewBag.Books = BooksRepository.GetBooks();
            ViewBag.OrderedBooks = OrdersRepository.GetBooks(promoCode);
        }
        public ActionResult AddToOrder(string ISBNCode)
        {
            OrdersRepository.AddBooks((Guid)Session["PromoCode"], ISBNCode);
            Update();
            return View("Index");
        }
    }
}