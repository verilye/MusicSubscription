using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Http;


namespace CloudComputingAss2.Controllers
{   
    
    [Route("[controller]")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger; 
        }

        public IActionResult Index(IFormCollection fc)
        {   


            var username = Request.Cookies["UserName"];
            
            ViewData["username"] =username;
            return View();
        }
    }
}