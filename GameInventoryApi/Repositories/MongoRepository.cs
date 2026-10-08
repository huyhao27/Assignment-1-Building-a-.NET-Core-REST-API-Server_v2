using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GameInventoryApi.Repositories;

public class MongoRepository<T> : IMongoRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoDatabase database, string collectionName)
    {
        _collection = database.GetCollection<T>(collectionName);
    }

    public async Task<List<T>> GetAllAsync() =>
        await _collection.Find(_ => true).ToListAsync();

    public async Task<T?> GetByIdAsync(string id) =>
        await _collection.Find(IdFilter(id)).FirstOrDefaultAsync();

    public async Task<T?> GetByFilterAsync(Expression<Func<T, bool>> filter) =>
        await _collection.Find(filter).FirstOrDefaultAsync();

    public async Task CreateAsync(T entity) =>
        await _collection.InsertOneAsync(entity);

    public async Task UpdateAsync(string id, T entity) =>
        await _collection.ReplaceOneAsync(IdFilter(id), entity);

    public async Task DeleteAsync(string id) =>
        await _collection.DeleteOneAsync(IdFilter(id));

    // A filter built from the string "_id" bypasses the [BsonRepresentation(ObjectId)] mapping,
    // so a raw string never matches the stored ObjectId; convert it explicitly.
    private static FilterDefinition<T> IdFilter(string id) =>
        ObjectId.TryParse(id, out var objectId)
            ? Builders<T>.Filter.Eq("_id", objectId)
            : Builders<T>.Filter.Eq("_id", id);
}
