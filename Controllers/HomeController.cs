using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicSubscription.Models;
using Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using System.Net;

namespace CloudComputingAss2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IConfiguration _config;

        public HomeController(IAmazonDynamoDB dynamoDB, ILogger<HomeController> logger, IConfiguration configuration)
        {
            _config = configuration;
            _logger = logger;
            _dynamoDb = dynamoDB;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IndexAsync(string email, string username, string password)
        {
            //Query for unique ID, scan for attributes

            string tableName = "login";
            Table ThreadTable = Table.LoadTable(_dynamoDb, tableName);

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("email", ScanOperator.Equal, email);
            scanFilter.AddCondition("user_name", ScanOperator.Equal, username);
            scanFilter.AddCondition("password", ScanOperator.Equal, password);
            Search search = ThreadTable.Scan(scanFilter);

            var result = search.Count;

            if(result == 1){
                
                var cookieOptions = new CookieOptions{Secure = true,HttpOnly = true,SameSite = SameSiteMode.None};
                Response.Cookies.Append("UserName", username, cookieOptions);
                Response.Cookies.Append("Email", email, cookieOptions);
                return RedirectToAction("Index", "Main");

            }else{

                ViewData["error"] = "email or password is invalid";
                return View();

            }

            
        }
            

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
