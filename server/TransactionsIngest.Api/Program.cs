var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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