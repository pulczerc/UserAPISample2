using MongoDB.Driver;

namespace UserAPISample2.DAL
{
    public interface IMongoDbContext
    {
        IMongoCollection<T> GetCollection<T>(string collectionName);
    }
}
