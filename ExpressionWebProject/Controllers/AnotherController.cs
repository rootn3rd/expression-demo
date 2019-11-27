using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExpressionWebProject.Infrastructure;

namespace ExpressionWebProject.Controllers
{
    public class AnotherController : Controller
    {
        //public IActionResult Index()
        //{
        //    return RedirectToAction("Index", "Home");
        //}

        public IActionResult About()
        {
            var id = 32;
            return this.RedirectTo<HomeController>(c => c.Index(id, "Some tested string"));

            //return RedirectToAction("Index", "Home", new { id = 19, query = "Some interesting value"});
        }
    }
}