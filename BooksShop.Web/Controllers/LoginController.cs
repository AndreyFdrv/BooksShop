using System;
using BooksShop.DataLayer;
using System.Web.Mvc;
using System.Configuration;
using System.Net.Http;
using System.Net;

namespace BooksShop.Web.Controllers
{
    public class LoginController : Controller
    {
        public static string ConnectionString;
        OrdersRepository OrdersRepository;
        public LoginController()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ToString();
            OrdersRepository = new OrdersRepository(ConnectionString);
        }
        [HttpGet]
        public ActionResult Login()
        {
            ViewBag.ErrorMessage = "";
            return View();
        }
        [HttpPost]
        public ActionResult Login(string promoCodeStr)
        {
            Guid promoCode;
            try
            {
                promoCode = Guid.Parse(promoCodeStr);
            }
            catch (FormatException)
            {
                ViewBag.ErrorMessage = "Неверный формат промокода";
                return View();
            }
            if (!OrdersRepository.IsOrderExist(promoCode))
            {
                ViewBag.ErrorMessage = "Заказ с данным промокодом не был найден";
                return View();
            }
            Session["PromoCode"] = promoCode;
            return RedirectToAction("Index", "Home");
        }
        public ActionResult CreatePromoCode()
        {
            var promoCode = OrdersRepository.CreateOrder();
            Session["PromoCode"] = promoCode;
            return RedirectToAction("Index", "Home");
        }
    }
}