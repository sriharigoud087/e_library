using Microsoft.Extensions.Options;
using E_LibraryManager.Models;
using MongoDB.Driver;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Common.IntitialData;
using E_LibraryManager.ViewModels;
using MongoDB.Bson;

namespace E_LibraryManager.Services
{
    public class UserService
    {

        private readonly IMongoCollection<User> _userCollection;
        private readonly EmailService _emailService;

        public UserService(IOptions<DatabaseSettings> bookStoreDatabaseSettings, EmailService emailService)
        {
            var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _userCollection = mongoDatabase.GetCollection<User>("Users");

            _emailService = emailService;
        }
        public async Task<List<User>> GetAsync()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }
        public async Task<User?> GetAsync(string id)
        {
            return await _userCollection.Find(x => x.UserId == id).FirstOrDefaultAsync();
        }
        public async Task<User?> CheckLogin(LoginVM loginVM)
        {
            return await _userCollection.Find(x => x.UserId == loginVM.UserName && x.Password == loginVM.Password).FirstOrDefaultAsync();
        }
        public async Task UpdateAsyncUser(string id, User user)
        {
            await _userCollection.ReplaceOneAsync(x => x.Id == id, user);
        }
        public async Task CreateAsync(User user)
        {
            _emailService.SendEmail(new Message(new string[] { user.Email }, "Welcome to E-Library", $"Hi {user.FirstName},<br> You've successfully registered to the E-Library <br><br> Regards,<p>E-Library</p>", new string[] { }, new string[] { }, true));

            await _userCollection.InsertOneAsync(user);
        }

        public async Task<long> GetTotalUserCount()
        {
            return await _userCollection.CountDocumentsAsync(new BsonDocument());
        }
    }
}
