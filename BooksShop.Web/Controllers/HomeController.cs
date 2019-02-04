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
        public HomeController()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            BooksRepository = new BooksRepository(ConnectionString);
        }
        public ActionResult Index()
        {
            ViewBag.PromoCode = Session["PromoCode"];
            var books = BooksRepository.GetBooks();
            ViewBag.Books = books;
            return View();
        }
    }
}