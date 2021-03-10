﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace UserAPI.Models
{
    public class AuthModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public UserTypes UserType { get; set; }
        public UserStatus Status { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        [Required]
        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [Required]
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
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

        [JsonPropertyName("tokenExpireAt")]
        public DateTime TokenExpireAt { get; set; }

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

    public class ResponseMessage
    {
        public string type { get; set; }

        public string message { get; set; }
    }
}
