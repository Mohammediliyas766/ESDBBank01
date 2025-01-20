using BankAPI.Infrastructure.Data;
using BankAPI.Infrastructure.Data.Repositories;
using BankAPI.Infrastructure.EventStore;
using EventStore.Client;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<BankDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add EventStoreDB Client
var eventStoreConnection = builder.Configuration.GetConnectionString("EventStore");
if (string.IsNullOrEmpty(eventStoreConnection))
{
    throw new InvalidOperationException("EventStore connection string is not configured");
}

builder.Services.AddSingleton(new EventStoreClient(EventStoreClientSettings.Create(eventStoreConnection)));
//builder.Services.AddSingleton(new EventStoreClient(EventStoreClientSettings.Create(
//    builder.Configuration.GetConnectionString("EventStore"))));

// Add Repositories
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IEventStore, EventStoreRepository>();

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

// Apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
    db.Database.Migrate();
}

app.Run();
