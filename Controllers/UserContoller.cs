using Microsoft.AspNetCore.Mvc;
using Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace CloudComputingAss2.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IConfiguration _configuration;

    public UserController(IAmazonDynamoDB dynamoDB, IConfiguration configuration)
    {
        _dynamoDb = dynamoDB;
        _configuration = configuration;
        
    }
  
    [HttpGet]
    public async Task<string> GetUsers()
    {
        var options = _configuration.GetAWSOptions();
        string status;
        using (var client = options.CreateServiceClient<IAmazonDynamoDB>())
        {
            var tableData = await client.DescribeTableAsync("login");
            status = tableData.Table.TableStatus;
        }

        return status;
    }
}