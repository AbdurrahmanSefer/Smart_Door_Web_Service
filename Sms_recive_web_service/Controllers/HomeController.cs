using Sms_recive_web_service.Models;
using System;
using System.Web.Mvc;

namespace Sms_recive_web_service.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            LoadDataAsync();
            return View();
           // return Redirect("https://smsreceive.site");
        }

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            try
            {
                PushNotification pushNotification = new PushNotification("cRhfh2BXS9SSWcnyatq24s:APA91bE2F4Bg_1GggOPii8qtQajPfBGQqABRkPijZyPTT0MIaf2ZhMPJ9nEcOO3SVvvx1euLOiNbkb5gsttp3N9rEnnUYTJzQNGiCUBZQUqcARKIhwYkpMTXNmBY1_iMQQxZ6Zf9qw71");
             
            }
            catch(Exception ex)
            {
                String hat = ex.ToString();
            }
        
        }
    }
}
