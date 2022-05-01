using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace CloudComputingAss2.Controllers
{
    [Route("[controller]")]
    public class RegisterController : Controller
    {
        private readonly ILogger<RegisterController> _logger;
        private readonly IAmazonDynamoDB _dynamoDb;

        public RegisterController(IAmazonDynamoDB dynamoDb, ILogger<RegisterController> logger)
        {
            _dynamoDb = dynamoDb;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreateUser(string email, string username, string password)
        {
            string tableName = "login";
            Table ThreadTable = Table.LoadTable(_dynamoDb, tableName);

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("email", ScanOperator.Equal, email);
            Search search = ThreadTable.Scan(scanFilter);

            var result = search.Count;

            if(result > 0){
                
                ViewData["error"] = "The email already exists";
                return View();
                
            }

            try{
                var request = new PutItemRequest
                {
                    TableName = tableName,
                    Item = new Dictionary<string, AttributeValue>()
                    {
                        { "email", new AttributeValue { S = email }},
                        { "user_name", new AttributeValue { S = username }},
                        { "password", new AttributeValue { S = password }}
                    }
                };

                _dynamoDb.PutItemAsync(request);
            }catch(Exception e){

                Console.Write(e);
            }

            TempData["error"] = "User created Successfully!";
            return RedirectToAction("Index", "Home");

        } 

    }
}