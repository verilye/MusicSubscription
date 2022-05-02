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

            //turn search results into a list that the view can read

            return View(musicList);
        }        

        [HttpPost]
        public async Task<IActionResult> Index(string title, string year, string artist)
        {
            
           //Display username at top
            var username = Request.Cookies["UserName"];
            ViewData["username"] =username;

            string tableName = "music";
            Table musicTable = Table.LoadTable(_dynamoDb, tableName);
            ScanFilter scanFilter = new ScanFilter();

            if(title!=null)scanFilter.AddCondition("title", ScanOperator.Equal, title);

            if(year!=null)scanFilter.AddCondition("year", ScanOperator.Equal, year);

            if(artist!=null)scanFilter.AddCondition("artist", ScanOperator.Equal, artist);

            Search search = musicTable.Scan(scanFilter);

            var result = search.Count;

            if(result == 0)
            {
                List<music> m = new List<music>();
                music song = new music();
                song.title = "No result is retrieved. Please query again";
                song.web_url ="";
                song.img_url ="";
                m.Add(song);

                ViewData["error"] = "No result is retrieved. Please query again";
                return View(m);
            }

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


            //turn search results into a list that the view can read

            return View(musicList);
        }



    }
}