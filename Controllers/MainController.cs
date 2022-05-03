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
using Amazon.DynamoDBv2.Model;
using System.Dynamic;

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


            //Get subscriptions from user's subscriptions array

            var email = Request.Cookies["Email"];

            Table users = Table.LoadTable(_dynamoDb, "login");
            Table musicTable = Table.LoadTable(_dynamoDb, "music");

            Document user = await users.GetItemAsync(email);

            List<string> userSubscriptions = (List<string>)user["subscriptions"];

            List<Document> docs = new List<Document>();
            foreach (var item in userSubscriptions)
            {

                if(item==""){

                    continue;
                }

                Document sub = await musicTable.GetItemAsync(item);


                docs.Add(sub);
                
            }

            List<music> subs = new List<music>();

            foreach (var document in docs)
                {
                    music song = new music();

                    song.title = document["title"];
                    song.artist =document["artist"];
                    song.year =document["year"];
                    song.web_url =document["web_url"];
                    song.img_url =document["img_url"];

                    subs.Add(song);
                }



/////////////////////////////////////////////////////////////////////////////////////////////

            //Get Music in query area
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

            
            
            dynamic Model = new ExpandoObject();
            Model.music = musicList;
            Model.subs = subs;

            return View(Model);
        } 

    
        [Route ("subscribe/{id}")]
        public async Task<IActionResult> subscribe(string id)
        {

            var email = Request.Cookies["Email"];

            string tableName = "login";

            var request = new UpdateItemRequest
            {
                TableName = tableName,
                Key = new Dictionary<string,AttributeValue>() { { "email", new AttributeValue { S = email } } },
                ExpressionAttributeNames = new Dictionary<string,string>()
                {
                    {"#s", "subscriptions"},
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":subscriptions", new AttributeValue { SS = {id}}},
                    
                },

                // This expression does the following:
                // 1) Adds two new authors to the list
                
                UpdateExpression = "ADD #s :subscriptions"
            };
            
            await _dynamoDb.UpdateItemAsync(request);

            return RedirectToAction("Index", "Main");
        }

        [Route ("remove/{id}")]
        public async Task<IActionResult> remove(string id)
        {

            var email = Request.Cookies["Email"];

            Table users = Table.LoadTable(_dynamoDb, "login");

            Document user = await users.GetItemAsync(email);

            List<string> userSubscriptions = (List<string>)user["subscriptions"];
            
            // int counter = 0;

            // for (int i = 0; i<userSubscriptions.Count;i++)
            // {
            //     if(id == userSubscriptions[i]) break;

            //     counter++;
            // }

            userSubscriptions.Remove(id);


            var request = new PutItemRequest
            {
                TableName = "login",
                Item = new Dictionary<string,AttributeValue>() 
                { 
                    { "email", new AttributeValue { S = user["email"] } },
                    { "user_name", new AttributeValue { S = user["user_name"] } },
                    { "password", new AttributeValue { S = user["password"] } },
                    { "subscriptions", new AttributeValue {SS = userSubscriptions}}
                        
                }
            };
            
                        
            await _dynamoDb.PutItemAsync(request);
    
            return RedirectToAction("Index", "Main");
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