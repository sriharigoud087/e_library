using Microsoft.Extensions.Options;
using E_LibraryManager.Models;
using MongoDB.Driver;
using E_LibraryManager.Common.Models;
using E_LibraryManager.Common.IntitialData;
using E_LibraryManager.ViewModels;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace E_LibraryManager.Services
{
    public class TokenService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Admin> _adminCollection;
        private readonly JWTService _jwtService;

        public TokenService(IOptions<DatabaseSettings> bookStoreDatabaseSettings, JWTService jwtService)
        {
            BookStoreData bookStoreData = new BookStoreData();

            var mongoClient = new MongoClient(
            bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);


            _userCollection = mongoDatabase.GetCollection<User>("Users");
            _adminCollection = mongoDatabase.GetCollection<Admin>("Admin");

            if (_userCollection.Find(_ => true).CountDocuments() == 0)
            {
                _userCollection.InsertMany(bookStoreData.GetUsers());
            }
            if (_adminCollection.Find(_ => true).CountDocuments() == 0)
            {
                _adminCollection.InsertMany(bookStoreData.GetAdmins());
            }

            _jwtService = jwtService;
        }
        public async Task<User?> GetAsync(string id,string password)
        {
            return await _userCollection.Find(x => x.Email == id&&x.Password==password).FirstOrDefaultAsync();
        }
        public async Task<Admin?> GetAdminAsync(string id, string password)
        {
            return await _adminCollection.Find(x => x.Email == id && x.Password == password).FirstOrDefaultAsync();
        }

        
        public async Task<User?> CheckLogin(LoginVM loginVM)
        {
            return await _userCollection.Find(x => x.Email == loginVM.UserName && x.Password == loginVM.Password).FirstOrDefaultAsync();
        }
        private async Task<User> AuthenticateUser(LoginVM loginVM)
        {
            var model = await GetAsync(loginVM.UserName,loginVM.Password);

            if (model == null)
            {
                // authentication failed
                return null;
            }
            return model;
        }
        private async Task<Admin> AuthenticateAdminUser(LoginVM loginVM)
        {
            var model = await GetAdminAsync(loginVM.UserName, loginVM.Password);

            if (model == null)
            {
                // authentication failed
                return null;
            }
            return model;
        }
        

        public async Task<object> UserLogin(LoginVM loginVM)
        {
            var user = await AuthenticateUser(loginVM);
            if (user != null)
            {
                var claims = GetUserClaims(user);
                var tokenstring = _jwtService.GenerateJSONWebToken(claims: claims);
                return new { Token = tokenstring,userDetails = user };
            }
            else
            {
                var admin = await AuthenticateAdminUser(loginVM);
                if (admin != null)
                {
                    var claims = GetUserAdminClaims(admin);
                    var tokenstring = _jwtService.GenerateJSONWebToken(claims: claims);
                    var model = new
                    {
                        Id = admin.Id,
                        Email = admin.Email,
                        Password = admin.Password,
                        isAdmin=1,
                        Role="Admin"
                    };
                    return new { Token = tokenstring, userDetails = model };
                }
            }
            return "Unauthorized";
        }

        public List<Claim> GetUserClaims(User user)
        {
            var role = user.Role == "Admin" ? 1 : (user.Role == "Librarian" ? 2 : 0);
            List<Claim> claims = new List<Claim>()
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim("FirstName", user.FirstName),
                    new Claim("LastName", user.LastName),
                    new Claim("Email", user.Email),
                    new Claim("RoleId", role.ToString()),
                    new Claim("isAdmin",role.ToString()),
                };
            return claims;
        }
        public List<Claim> GetUserAdminClaims(Admin admin)
        {
            var role  = 1;
            List<Claim> claims = new List<Claim>()
                {
                    new Claim("UserId", admin.Id.ToString()),
                    new Claim("FirstName",admin.Email),
                    new Claim("LastName", admin.Email),
                    new Claim("Email", admin.Email),
                    new Claim("RoleId", role.ToString()),
                    new Claim("isAdmin",role.ToString()),
                };
            return claims;
        }
    }

}
