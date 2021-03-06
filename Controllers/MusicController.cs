using Microsoft.AspNetCore.Mvc;
using Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;

namespace CloudComputingAss2.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MusicController : ControllerBase
    {
        private readonly IAmazonDynamoDB _dynamoDb;
        private readonly IConfiguration _config;

        public MusicController(IAmazonDynamoDB dynamoDB, IConfiguration configuration)
        {
            _dynamoDb = dynamoDB;
            _config = configuration;        
            
        }


        [HttpPost("downloadImages")]
        public async Task<string> downloadImages(){

            string json;
            dynamic items;
            
            using (StreamReader r = new StreamReader("a2.json"))
            {
                json = r.ReadToEnd();
                
                items = JArray.Parse(json);
                
            }
            
            
            for(int i = 0;i<items.Count;i++)
            {
                
                using (WebClient client = new WebClient())
                {
                
                    string title =Convert.ToString(items[i].title);
                    string img = Convert.ToString(items[i].img_url);

                    client.DownloadFileAsync(new Uri(img), @"Images\"+ title + ".jpg");
                    
                }
                
            }

            return ("Downloaded");
        }

        
        [HttpPost("uploadImages")]
        public async Task<string> uploadImages(){

            string access_key = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID");
            string secret_key = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY");


            var s3Client = new AmazonS3Client(access_key, secret_key, Amazon.RegionEndpoint.USEast1);
                
            var directoryTransferUtility =  new TransferUtility(s3Client);
            
            await directoryTransferUtility.UploadDirectoryAsync("Images","connorlogancloudcomputingass2");

            return ("Uploaded");
        }



        [HttpPost("loadA2Data")]
        public async Task<string> LoadA2Data()
        {

            DynamoDBContext context = new DynamoDBContext(_dynamoDb);
            var musicBatch = context.CreateBatchWrite<music>();
            List<music> items;
            string json;
            
            using (StreamReader r = new StreamReader("a2.json"))
            {
                json = r.ReadToEnd();
                
                items = JsonConvert.DeserializeObject<List<music>>(json);
            }
            musicBatch.AddPutItems(items);
            await musicBatch.ExecuteAsync();
            return ("Done");
        }


        [HttpPost("musicTable")]
        public async Task<string> CreateTable()
        {
            var options = _config.GetAWSOptions();
            string status;
            using (var client = options.CreateServiceClient<IAmazonDynamoDB>())
            {
                string tableName = "music";
                try
                {   

                    
                    //GLOBAL SECONDARY INDEXES

                    var ptIndex = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 1L,
                        WriteCapacityUnits = 1L
                    };

                    var createArtistIndex = new GlobalSecondaryIndex()
                    {
                        IndexName = "artist",
                        ProvisionedThroughput = ptIndex,
                        KeySchema = {
                            new KeySchemaElement {
                                AttributeName = "artist", 
                                KeyType = "HASH" //Partition key
                            }
                        },
                        Projection = new Projection
                        {
                            ProjectionType = "KEYS_ONLY"
                        }
                    };
                    
                    var createYearIndex = new GlobalSecondaryIndex()
                    {
                        IndexName = "year",
                        ProvisionedThroughput = ptIndex,
                        KeySchema = {
                            new KeySchemaElement {
                                AttributeName = "year", 
                                KeyType = "HASH" //Partition key
                            }
                        },
                        Projection = new Projection
                        {
                            ProjectionType = "KEYS_ONLY"
                        }
                    };

                    var createWebUrlIndex = new GlobalSecondaryIndex()
                    {
                        IndexName = "web_url",
                        ProvisionedThroughput = ptIndex,
                        KeySchema = {
                            new KeySchemaElement {
                                AttributeName = "web_url", 
                                KeyType = "HASH" //Partition key
                            }
                        },
                        Projection = new Projection
                        {
                            ProjectionType = "KEYS_ONLY"
                        }
                    };

                    var createImageUrlIndex = new GlobalSecondaryIndex()
                    {
                        IndexName = "img_url",
                        ProvisionedThroughput = ptIndex,
                        KeySchema = {
                            new KeySchemaElement {
                                AttributeName = "img_url", 
                                KeyType = "HASH" //Partition key
                            }
                        },
                        Projection = new Projection
                        {
                            ProjectionType = "KEYS_ONLY"
                        }
                    };

                    //CREATE TABLE AND ADD GLOBAL SECONDARY INDEXES

                    var request = new CreateTableRequest
                    {
                        TableName = tableName,
                        ProvisionedThroughput = new ProvisionedThroughput
                        {
                            ReadCapacityUnits = (long)1,
                            WriteCapacityUnits = (long)1
                        },
                        AttributeDefinitions = new List<AttributeDefinition>()
                        {
                            new AttributeDefinition
                            {
                            AttributeName = "title",
                            AttributeType = "S"
                            },
                            new AttributeDefinition
                            {
                            AttributeName = "artist",
                            AttributeType = "S"
                            },
                            new AttributeDefinition
                            {
                            AttributeName = "year",
                            AttributeType = "S"
                            },
                            new AttributeDefinition
                            {
                            AttributeName = "web_url",
                            AttributeType = "S"
                            },
                            new AttributeDefinition
                            {
                            AttributeName = "img_url",
                            AttributeType = "S"
                            },


                        },
                        KeySchema = new List<KeySchemaElement>()
                        {
                            new KeySchemaElement
                            {
                            AttributeName = "title",
                            KeyType = "HASH"  //Partition key
                            }
                        },
                        GlobalSecondaryIndexes={
                            createArtistIndex,
                            createYearIndex,
                            createWebUrlIndex,
                            createImageUrlIndex
                        }
                    };

                    var response = await client.CreateTableAsync(request);

                    var tableDescription = response.TableDescription;

                    status = tableDescription.TableStatus;

                    return status;

                }
                catch (IOException e)
                {
                    if (e.Source != null)
                        Console.WriteLine("IOException source: {0}", e.Source);
                    throw;
                }



            }
            

            
        }
    }
}
