using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MusicSubscription.Models;
using Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace MusicSubscription.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IAmazonDynamoDB _dynamoDb;

    public HomeController(IAmazonDynamoDB dynamoDB, ILogger<HomeController> logger)
    {
        _logger = logger;
        _dynamoDb = dynamoDB;
    }
    
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> IndexAsync(string email, string password)
    {
        //Query for unique ID, scan for attributes

        string tableName = "login";
        Table ThreadTable = Table.LoadTable(_dynamoDb, tableName);

        ScanFilter scanFilter = new ScanFilter();
        scanFilter.AddCondition("email", ScanOperator.Equal, email);
        scanFilter.AddCondition("password", ScanOperator.Equal, password);
        Search search = ThreadTable.Scan(scanFilter);

        var result = search.Count;

        if(result == 1){

            return View();

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
