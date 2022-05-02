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
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Models;


namespace CloudComputingAss2.Controllers
{   
    
    [Route("[controller]")]
    public class MainController : Controller
    {
        private readonly ILogger<MainController> _logger;
        public readonly IAmazonDynamoDB _dynamoDb;

        public MainController(IAmazonDynamoDB dynamoDb, ILogger<MainController> logger)
        {
            _dynamoDb = dynamoDb;
            _logger = logger; 
        }

        public async Task<IActionResult> Index()
        {   

            //Display username at top
            var username = Request.Cookies["UserName"];
            ViewData["username"] =username;

            string tableName = "music";
            Table musicTable = Table.LoadTable(_dynamoDb, tableName);
            ScanFilter scanFilter = new ScanFilter();
            Search search = musicTable.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            List<music> musicList = new List<music>();
            do
            {
                documentList = await search.GetNextSetAsync();
                foreach (var document in documentList)
                {
                    music song = new music();

                    song.title = document["title"];
                    song.artist =document["artist"];
                    song.year =document["year"];
                    song.web_url =document["web_url"];
                    song.img_url =document["img_url"];

                    musicList.Add(song);
                }

                
            } while (!search.IsDone);


            string token = search.PaginationToken;
            var cookieOptions = new CookieOptions{Secure = true,HttpOnly = true,SameSite = SameSiteMode.None};
            Response.Cookies.Append("pagination", token, cookieOptions);
            
            //Display and paginate subscribed data 
            //turn search results into a list that the view can read

            return View(musicList);
        }        

        [HttpPost]
        public IActionResult queryMusic(string title, string year, string artist)
        {

            //Query based on input
            //Return all relevant music in the view

            var username = Request.Cookies["UserName"];
            ViewData["username"] = username;

            string tableName = "music";
            Table ThreadTable = Table.LoadTable(_dynamoDb, tableName);

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("title", ScanOperator.Equal, title);
            scanFilter.AddCondition("year", ScanOperator.Equal, year);
            scanFilter.AddCondition("artist", ScanOperator.Equal, artist);
            Search search = ThreadTable.Scan(scanFilter);



            return View();
        }



    }
}