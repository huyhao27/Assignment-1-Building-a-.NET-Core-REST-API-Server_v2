using GameInventoryApi.Models;
using MongoDB.Driver;

namespace GameInventoryApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IMongoDatabase database)
    {
        var users = database.GetCollection<User>("Users");
        var inventory = database.GetCollection<InventoryItem>("InventoryItems");
        var profiles = database.GetCollection<PlayerProfile>("PlayerProfiles");

        await users.DeleteManyAsync(_ => true);
        await inventory.DeleteManyAsync(_ => true);
        await profiles.DeleteManyAsync(_ => true);

        // Seed Admin
        var admin = new User { Username = "admin", PasswordHash = "admin123", Role = "Admin" };
        await users.InsertOneAsync(admin);

        // Seed Player1
        var player1 = new User { Username = "player1", PasswordHash = "player123", Role = "Player" };
        await users.InsertOneAsync(player1);

        // Seed Player2
        var player2 = new User { Username = "player2", PasswordHash = "player123", Role = "Player" };
        await users.InsertOneAsync(player2);

        // Seed Inventory for player1
        await inventory.InsertOneAsync(new InventoryItem
        {
            ItemId = "sword_001",
            Name = "Iron Sword",
            Quantity = 1,
            PlayerId = player1.Id
        });

        // Seed Profile for player1
        await profiles.InsertOneAsync(new PlayerProfile
        {
            PlayerId = player1.Id,
            Username = "player1",
            Level = 10,
            Experience = 2450
        });

        // Seed Profile for player2
        await profiles.InsertOneAsync(new PlayerProfile
        {
            PlayerId = player2.Id,
            Username = "player2",
            Level = 5,
            Experience = 800
        });
    }
}
