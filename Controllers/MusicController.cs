using Microsoft.AspNetCore.Mvc;
using Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;


namespace CloudComputingAss2.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IConfiguration _configuration;

    public MusicController(IAmazonDynamoDB dynamoDB, IConfiguration configuration)
    {
        _dynamoDb = dynamoDB;
        _configuration = configuration;
        
    }

    [HttpPost]
    public async Task<string> CreateTable()
    {
        var options = _configuration.GetAWSOptions();
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
                    IndexName = "image_url",
                    ProvisionedThroughput = ptIndex,
                    KeySchema = {
                        new KeySchemaElement {
                            AttributeName = "image_url", 
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
                        AttributeName = "image_url",
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
