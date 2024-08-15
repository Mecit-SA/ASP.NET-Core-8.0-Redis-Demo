using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddStackExchangeRedisCache(action => {
    var connection = builder.Configuration["RedisConnection"];
    action.Configuration = connection;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/count", async (IDistributedCache cache) =>
{
    var value = await cache.GetStringAsync("count");
    if (string.IsNullOrWhiteSpace(value))
        return 0;

    return int.Parse(value);
})
.WithName("Get Count")
.WithOpenApi();

app.MapPut("/increase", async (IDistributedCache cache) =>
{
    var value = await cache.GetStringAsync("count");

    var nextValue = string.IsNullOrWhiteSpace(value) ?
                    1.ToString() :
                    (int.Parse(value) + 1).ToString();

    cache.SetString("count", nextValue);
    return Results.Ok();
})
.WithName("Increase Count")
.WithOpenApi();

app.Run();