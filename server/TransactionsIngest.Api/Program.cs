// server/TransactionsIngest.Api/Program.cs


using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using TransactionsIngest.Api.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


// connection string for the databse
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing.");

// register the AppDbContext with the dependency 
// injection container using the connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.MapGet("/", () => 
    new {
        message =  "Transactions Ingest API",
        status = "running", 
        timestamp = DateTime.UtcNow
    }
);



app.Run();



// run 
// dotnet run --project TransactionsIngest.Api --urls http://localhost:5000