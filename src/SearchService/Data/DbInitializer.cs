using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb", MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
        .Key(q => q.Make, KeyType.Text)
        .Key(q => q.Model, KeyType.Text)
        .Key(q => q.Color, KeyType.Text)
        .CreateAsync();

        var count = await DB.CountAsync<Item>();

        if (count == 0)
        {
            Console.WriteLine("No Data Attempt To Seed");

            var itemData = await File.ReadAllTextAsync("Data/Auction.json");

            var option = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var items = JsonSerializer.Deserialize<List<Item>>(itemData, option);
            
            await DB.SaveAsync(items);
        }
    }
}
