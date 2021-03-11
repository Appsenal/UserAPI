using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserAPI.Models
{
    //[pqa] This model is to handle the data classes needed in the authentication.

    //[pqa] Class to hold the details of the user that will be used in authentication processes.
    public class AuthModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public UserTypes UserType { get; set; }
        public UserStatus Status { get; set; }
    }

    //[pqa] Class to hold the user credential from the login request.
    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    //[pqa] Class to hold the tokens from the refresh token request.
    public class RefreshTokenRequest
    {
        [Required]
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [Required]
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }

    //[pqa] Class to hold the authentication details, including the access token and refresh token. Mostly use to facilitate the logout capability.
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

        [JsonPropertyName("tokenExpireAt")]
        public DateTime TokenExpireAt { get; set; }

        [JsonPropertyName("refreshToken")]
        public RefreshToken refreshToken { get; set; }
    }

    //[pqa] Details for the refresh token.
    public class RefreshToken
    {
        [JsonPropertyName("username")]
        public string UserName { get; set; }

        [JsonPropertyName("tokenString")]
        public string TokenString { get; set; }

        [JsonPropertyName("expireAt")]
        public DateTime ExpireAt { get; set; }
    }

    //[pqa] Response message.
    public class ResponseMessage
    {
        public string type { get; set; }

        public string message { get; set; }
    }
}
