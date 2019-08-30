using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Marvel.Models;
using System.Web.Mvc;
using System.Collections.Generic;

namespace Marvel.Controllers
{
    public class HomeController : Controller
    {        
        public ActionResult Index()
        {
            return View();
        }       
       
    }
}
