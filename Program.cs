using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Extensions.NETCore.Setup;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
ConfigurationManager Configuration = builder.Configuration;

builder.Services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddScoped<IDynamoDBContext, DynamoDBContext>();


// var credentials = new BasicAWSCredentials(Configuration["ACCESS_KEY"], Configuration["SECRET_KEY"]);
// var config = new AmazonDynamoDBConfig()
// {
//     RegionEndpoint = RegionEndpoint.USEast1  
// };

// var client = new AmazonDynamoDBClient(credentials,config);

// builder.Services.AddSingleton<IAmazonDynamoDB>(client);
// builder.Services.AddSingleton<IDynamoDBContext, DynamoDBContext>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
