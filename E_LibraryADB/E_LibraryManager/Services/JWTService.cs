using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using E_LibraryManager.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;

namespace E_LibraryManager.Services
{
    public class JWTService
    {
        private readonly JwtConfiguration _jwtConfiguration;
        public JWTService(AppSettings appSettings)
        {
            _jwtConfiguration = appSettings.JwtConfiguration;
        }
       
        public string GenerateJSONWebToken([Optional] List<Claim> claims, [Optional] DateTime? expires)
        {
            try
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfiguration.Key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                  _jwtConfiguration.Issuer,
                  _jwtConfiguration.Audience,
                  claims ?? new List<Claim>(),
                  expires: expires ?? DateTime.Now.AddHours(24),
                  signingCredentials: credentials);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception exception)
            {
                return null;
            }

        }
        public JwtSecurityToken DecodeJwt(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token);
                var decodedToken = jsonToken as JwtSecurityToken;
                return decodedToken;
            }
            catch (Exception exception)
            {
                return null;
            }
        }
        public JwtSecurityToken ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtConfiguration.Key);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero,
                    LifetimeValidator = CustomLifetimeValidator,
                    ValidIssuer = _jwtConfiguration.Issuer,
                    ValidAudience = _jwtConfiguration.Audience,
                }, out SecurityToken validatedToken);

                var jwtSecurityToken = (JwtSecurityToken)validatedToken;
                return jwtSecurityToken;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }
       

        private bool CustomLifetimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken tokenToValidate, TokenValidationParameters @param)
        {
            if (expires != null)
            {
                return expires > DateTime.UtcNow;
            }
            return false;
        }
    }

}
