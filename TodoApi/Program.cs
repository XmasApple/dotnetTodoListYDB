using TodoApi.Models;
using Ydb.Sdk;
using Ydb.Sdk.Table;

var driverConfig = new DriverConfig(
    endpoint: "http://localhost:2136",
    database: "/local"
);

using var driver = new Driver(driverConfig);

await driver.Initialize();

using var tableClient = new TableClient(driver, new TableClientConfig());

// await TodoApi.Database.RegisterTables(new TodoItem());
TodoApi.Database.Init(tableClient);
await TodoApi.Database.RegisterTables(new TodoItem());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
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
