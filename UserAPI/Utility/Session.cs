using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using UserAPI.Models;

namespace UserAPI.Utility
{
    public interface IntSession
    {
        UserAPIContext context { get; set; }
        //public IConfiguration config { get; set; }
        AccessToken Start(LoginRequest login);
        void Close(string userName);
        bool isSessionValid(string userName, HttpRequest request);
        //AuthModel Authenticate(LoginRequest login);
        //string GenerateToken(AuthModel user);
    }

    public class Session : IntSession
    {
        private readonly ConcurrentDictionary<string, AccessToken> _usersAccessTokens;
        //private readonly ConcurrentDictionary<string, RefreshToken> _usersRefreshTokens;
        //private readonly UserAPIContext _context;
        private readonly IConfiguration _config;

        public UserAPIContext context { get; set; }
        //public IConfiguration config { get; set; }

        public Session(IConfiguration config)
        {
            _config = config;
            //[pqa] Instantiate storage for token
            _usersAccessTokens = new ConcurrentDictionary<string, AccessToken>();
        }

        //public void Start(AccessToken token)
        public AccessToken Start(LoginRequest login)
        {
            //[pqa] When the session starts, authenticate the login then generate token. 

            AccessToken token = null;
            var user = Authenticate(login);

            if (user != null)
            {
                token = GenerateTokens(user);

                //token = new AccessToken { UserName = user.UserName, UserType = user.UserType, Email = user.Email, TokenString = tokenString };

                //[pqa] Store access token with the username as key.
                //_usersAccessTokens.AddOrUpdate(token.UserName, token, (s, t) => token);
            }

            return token;
        }

        public void Close(string userName)
        {
            //[pqa] Remove the token from the storage.
            var accessTokens = _usersAccessTokens.Where(t => t.Key == userName);
            foreach (var accessToken in accessTokens)
            {
                _usersAccessTokens.TryRemove(accessToken.Key, out _);
            }
        }

        public bool isSessionValid(string userName, HttpRequest request)
        {
            //[pqa] Check if the token is still in the storage. Return true if it is.
            bool result = false;

            StringValues token="";
            request.Headers.TryGetValue("Authorization", out token);

            var accessTokens = _usersAccessTokens.Where(t => t.Key == userName)
                .Where(t => "Bearer " + t.Value.TokenString == token.ToString()).ToList();
            
            if (accessTokens.Count() > 0)
            {
                result = true;
            }
            
            return result;
        }

        private AuthModel Authenticate(LoginRequest login)
        {
            AuthModel user = null;

            //[pqa] Check if database is not empty.
            if (context.Users.Count() > 0)
            {
                //[pqa] If database is NOT empty, evaluate the username and password against the value in the database.
                var reqUser = context.Users.SingleOrDefault(e => e.UserName == login.UserName);
                if (reqUser != null)
                {
                    //[pqa] The user should be active.
                    if (reqUser.Status == UserStatus.Active)
                    {
                        if (Cipher.Decrypt(reqUser.Password, reqUser.Email) == login.Password)
                        {
                            user = new AuthModel { UserName = login.UserName, Email = reqUser.Email, UserType = reqUser.UserType, Status = reqUser.Status };
                        }
                    }
                }
            }
            else
            {
                //[pqa] If database is empty, default the password.
                string defUserName = "Admin";
                string defPassword = "Password1";
                if (login.UserName == defUserName && login.Password == defPassword)
                {
                    user = new AuthModel { UserName = "Admin", Email = "admin@test.com", UserType = UserTypes.Admin, Status = UserStatus.Active };
                }
            }

            return user;
        }

        private AccessToken GenerateTokens(AuthModel user)
        {
            //[pqa] generate the token
            AccessToken accessTokens = null;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.UserType.ToString()),
                new Claim("status", user.Status.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:accessTokenValidity"])),
                signingCredentials: credentials);

            RefreshToken refreshToken = new RefreshToken
            {
                UserName = user.UserName,
                TokenString = GenerateRefreshTokenString(),
                ExpireAt = DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:refreshTokenValidity"]))
            };

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            accessTokens = new AccessToken
            {
                UserName = user.UserName,
                UserType = user.UserType,
                Email = user.Email,
                TokenString = tokenString,
                refreshToken = refreshToken
            };

            _usersAccessTokens.AddOrUpdate(accessTokens.UserName, accessTokens, (s, t) => accessTokens);

            return accessTokens;
    }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public class AccessToken
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("userType")]
        public UserTypes UserType { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("tokenString")]
        public string TokenString { get; set; }

        [JsonPropertyName("refreshToken")]
        public RefreshToken refreshToken { get; set; }
    }

    public class RefreshToken
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }  

        [JsonPropertyName("tokenString")]
        public string TokenString { get; set; }

        [JsonPropertyName("expireAt")]
        public DateTime ExpireAt { get; set; }
    }
}
