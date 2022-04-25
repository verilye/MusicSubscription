using Microsoft.AspNetCore.Mvc;

namespace CloudComputingAss2.Controllers;

[ApiController]
[Route("[controller]")]
public class MusicController : ControllerBase
{
    private readonly ILogger<MusicController> _logger;

    public MusicController(
        IDynamoDBContext dynamoDbContext,
        ILogger<MusicController> logger)
    {
        _dynamoDbContext = dynamoDbContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IEnumerable<Music>> CreateMusicTable()
    {
        string tableName = "music";

        var request = new CreateTableRequest
        {
            TableName = tableName,
            AttributeDefinitions = new List<AttributeDefinition>()
        {
            new AttributeDefinition
            {
                AttributeName = "title",
                AttributeType = "S"
            }
        },
        KeySchema = new List<KeySchemaElement>()
        {
            new KeySchemaElement
            {
                AttributeName = "title",
                KeyType = "HASH"  //Partition key
            }
        },
        ProvisionedThroughput = new ProvisionedThroughput
        {
            ReadCapacityUnits = 10,
            WriteCapacityUnits = 5
        }
        };

        var response = await _dynamoDbContext.CreateTable(request);

    }
}
