using BooksShop.Model;
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
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.ErrorMessage = "";
            var status = OrdersRepository.GetStatus((Guid)Session["PromoCode"]);
            if (status==Order.StatusEnum.Ordered)
                ViewBag.InfoMessage = "Заказ оформлен";
            else if (status == Order.StatusEnum.Complete)
                ViewBag.InfoMessage = "Заказ выполнен";
            Update();
            return View();
        }
        private void Update()
        {
            var promoCode = (Guid)Session["PromoCode"];
            ViewBag.PromoCode = promoCode;
            ViewBag.Books = BooksRepository.GetBooks();
            ViewBag.OrderedBooks = OrdersRepository.GetBooks(promoCode);
            ViewBag.Cost = OrdersRepository.Cost(promoCode);
        }
        [HttpPost]
        public ActionResult AddToOrder(string ISBNCode)
        {
            try
            {
                OrdersRepository.AddBooks((Guid)Session["PromoCode"], ISBNCode);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                Update();
                return View("Index");
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult DeleteFromOrder(string ISBNCode)
        {
            try
            {
                OrdersRepository.DeleteBooks((Guid)Session["PromoCode"], ISBNCode);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                Update();
                return View("Index");
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult MakeOrder()
        {
            try
            {
                OrdersRepository.MakeOrder((Guid)Session["PromoCode"]);
            }
            catch (ArgumentException ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                Update();
                return View("Index");
            }
            return RedirectToAction("Index", "Home");
        }
    }
}