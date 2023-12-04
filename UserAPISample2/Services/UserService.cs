using MongoDB.Driver;
using UserAPISample2.DAL;
using UserAPISample2.Models;
using UserAPISample2.Settings;

namespace UserAPISample2.Services
{
    public class UserService : IUserService
    {
        private readonly IMongoDbContext _mongoDbContext;
        private readonly string _userCollectionName;
        private readonly string _countersCollectionName;

        public UserService(IMongoDbContext mongoDbContext, IUsersDatabaseSettings settings)
        {
            _mongoDbContext = mongoDbContext ?? throw new ArgumentNullException(nameof(mongoDbContext));
            _userCollectionName = 
                settings.UsersCollectionName ?? throw new ArgumentNullException(nameof(settings));
            _countersCollectionName =
                settings.CountersCollectionName ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Getting and incrementing a specific sequence by 1 
        /// </summary>
        /// <param name="sequenceName"></param>
        /// <returns></returns>
        public async Task<Counter> IncrementSeq(string sequenceName)
        {
            var filter = Builders<Counter>.Filter.Eq("_id", sequenceName);
            var update = Builders<Counter>.Update.Inc("seq", 1);

            var result = await _mongoDbContext.GetCollection<Counter>(_countersCollectionName).FindOneAndUpdateAsync(
                filter,
                update,
                new FindOneAndUpdateOptions<Counter>() {ReturnDocument = ReturnDocument.After}
            );

            return result;
        }

        public async Task<User?> GetUserAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var users = await _mongoDbContext.GetCollection<User>(_userCollectionName).FindAsync(filter);
            return await users.FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<User>> GetUsersAsync()
        {
            var users = await _mongoDbContext.GetCollection<User>(_userCollectionName).FindAsync(_ => true);
            return await users.ToListAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            //This can be used if we want to use a custom / unique numeric user Id.
            //var nextSeq = await IncrementSeq("userId");

            await _mongoDbContext.GetCollection<User>(_userCollectionName).InsertOneAsync(user);
            return user;
        }

        public async Task<bool> UpdateUserAsync(string id, User user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var result = await _mongoDbContext.GetCollection<User>(_userCollectionName).ReplaceOneAsync(filter, user);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, id);
            var result = await _mongoDbContext.GetCollection<User>(_userCollectionName).DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }
    }
}
