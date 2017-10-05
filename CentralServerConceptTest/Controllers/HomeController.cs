using CentralServerConceptTest.Bussiness;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CentralServerConceptTest.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<string> TestStatus()
        {
            CustomHttpClient client = new CustomHttpClient(true);
            return await client.InvokeGet("localhost:8080", "api/Action/Status");
        }
    }
}