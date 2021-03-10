using System;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
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
        AccessToken Start(LoginRequest login);
        bool Close(string userName);
        bool isSessionValid(string userName, HttpRequest request);
        ResponseMessage ResponseMsg(string type, string message);
        AccessToken RefreshToken(string refreshToken, string accessToken);
    }

    public class Session : IntSession
    {
        //[pqa] The token details will be stored in the concurrent dictionay. This is to facilitate logout and invalidate unneeded token. If the token is not in the dictionary (regardless if active or not), it will not be recognized. 
        private readonly ConcurrentDictionary<string, AccessToken> _usersAccessTokens;
        private readonly IConfiguration _config;

        public UserAPIContext context { get; set; }

        public Session(IConfiguration config)
        {
            _config = config;
            //[pqa] Instantiate storage for token
            _usersAccessTokens = new ConcurrentDictionary<string, AccessToken>();
        }

        public AccessToken Start(LoginRequest login)
        {
            //[pqa] When the session starts, authenticate the login then generate token. 

            AccessToken token = null;
            var user = Authenticate(login);

            if (user != null)
            {
                token = GenerateTokens(user);
            }

            return token;
        }

        public bool Close(string userName)
        {
            bool response = false;
            //[pqa] Remove the token from the storage.
            var accessTokens = _usersAccessTokens.Where(t => t.Key == userName);
            foreach (var accessToken in accessTokens)
            {
                response = _usersAccessTokens.TryRemove(accessToken.Key, out _);
            }

            return response;
        }

        public bool isSessionValid(string userName, HttpRequest request)
        {
            //[pqa] Check if the token is still in the storage. Return true if it is.
            bool result = false;
            
            StringValues token="";

            request.Headers.TryGetValue("Authorization", out token);

            //[pqa] Get the token details from the storage.
            var accessTokens = _usersAccessTokens.Where(t => t.Key == userName)
                .Where(t => "Bearer " + t.Value.TokenString == token.ToString()).ToList();
            
            if (accessTokens.Count() > 0)
            {
                result = true;
            }
            
            return result;
        }

        public AccessToken RefreshToken(string accessToken, string refreshToken)
        {
            AccessToken result = null;

            //[pqa] Decode JWT token and get the information e.g. username
            var (principal, jwtToken) = DecodeToken(accessToken);
            string userName = principal.Identity.Name;

            //[pqa] Validate tokens with the one in storage using the username from the previous code. The related details of the token should also be returned through the existingTokens variale.
            if (!_usersAccessTokens.TryGetValue(userName, out var existingTokens))
            {
                throw new SecurityTokenException("Invalid token");
            }

            //[pqa] The access token from the request should be the same from the token in the storage.
            if (accessToken != existingTokens.TokenString)
            {
                throw new SecurityTokenException("Invalid token");
            }

            //[pqa] Validate the encryption algo of the access token
            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
            {
                throw new SecurityTokenException("Invalid token");
            }
           
            //[pqa] Validate the validity of the refresh token
            if (existingTokens.refreshToken.TokenString != refreshToken || existingTokens.refreshToken.ExpireAt < DateTime.Now)
            {
                throw new SecurityTokenException("Invalid or expired refresh token");
            }

            //[pqa] Get the basic details of the user
            AuthModel user = new AuthModel
            {
                UserName = userName,
                Email = existingTokens.Email,
                UserType = existingTokens.UserType
            };

            //[pqa] Generate tokens
            result = GenerateTokens(user);

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
                        //[pqa] Try to decrypt the password.
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
            //[pqa] Initialize the needed information.
            AccessToken accessTokens = null;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            //[pqa] Assign claims details
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.UserType.ToString()),
                new Claim("status", user.Status.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            //[pqa] Details needed to generate JWT token.
            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:accessTokenValidity"])),
                signingCredentials: credentials);

            //[pqa] Generate the refresh token and collect the resulting details.
            RefreshToken refreshToken = new RefreshToken
            {
                UserName = user.UserName,
                TokenString = GenerateRefreshTokenString(),
                ExpireAt = DateTime.Now.AddMinutes(Convert.ToDouble(_config["Jwt:refreshTokenValidity"]))
            };

            //[pqa] Generate access token from the details above.
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            //[pqa] Collect details for access token.
            accessTokens = new AccessToken
            {
                UserName = user.UserName,
                UserType = user.UserType,
                Email = user.Email,
                TokenString = tokenString,
                refreshToken = refreshToken
            };

            //[pqa] Store the token information to the storage.
            _usersAccessTokens.AddOrUpdate(accessTokens.UserName, accessTokens, (s, t) => accessTokens);

            return accessTokens;
        }

        public (ClaimsPrincipal, JwtSecurityToken) DecodeToken(string token)
        {
            //[pqa] Make sure that the token is provided in the argument.
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new SecurityTokenException("Invalid token");
            }

            //[pqa] Validate the token.
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(token,
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = _config["Jwt:Issuer"],
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"])),
                        ValidAudience = _config["Jwt:Issuer"],
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.Zero
                    },
                    out var validatedToken);

            //[pqa] Return the resulting details.
            return (principal, validatedToken as JwtSecurityToken);
        }

        public ResponseMessage ResponseMsg(string type, string message)
        {
            return new ResponseMessage { type = type, message = message };
        }

        private string GenerateRefreshTokenString()
        {
            //[pqa] Randomly generate the token string for refresh token.
            var randomNumber = new byte[32];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

    }

    
}
