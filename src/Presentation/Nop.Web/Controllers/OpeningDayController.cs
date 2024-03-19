using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Nop.Web.Controllers
{
    public class OpeningDayController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}