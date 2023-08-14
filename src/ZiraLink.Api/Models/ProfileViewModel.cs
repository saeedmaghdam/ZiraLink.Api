﻿using System.Text.Json.Serialization;

namespace ZiraLink.Api.Models
{
    public class ProfileViewModel
    {
        [JsonPropertyName("preferred_username")]
        public string Username { get; set; }
        [JsonPropertyName("email")]
        public string Email { get; set; }
        [JsonPropertyName("given_name")]
        public string Name { get; set; }
        [JsonPropertyName("family_name")]
        public string Family { get; set; }
    }
}
