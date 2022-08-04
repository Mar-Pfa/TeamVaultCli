using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeamVaultRestApi
{
    public class TeamVaultSecret
    {
        [JsonProperty("access_policy")]
        public string AccessPolicy { get; set; }

        [JsonProperty("allowed_groups")]
        public List<string> AllowedGroups { get; set; }

        [JsonProperty("allowed_users")]
        public List<string> AllowedUsers { get; set; }

        [JsonProperty("api_url")]
        public string ApiUrl { get; set; }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }

        [JsonProperty("current_revision")]
        public string CurrentRevision { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("last_read")]
        public string LastRead { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("needs_changing_on_leave")]
        public bool NeedsChangingOnLeave { get; set; }

        /// <summary>
        /// list of users to notify
        /// </summary>
        [JsonProperty("notify_on_access_request")]
        public List<string> NotifyOnAccessRequest { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }

        public static string ParseString(string source, string search, string endMarker)
        {
            var pos = source.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return string.Empty;
            }

            var result = source.Substring(pos + search.Length);
            var endPos = result.IndexOf(endMarker, StringComparison.Ordinal);
            if (endPos < 0)
            {
                return result;
            }

            result = result.Substring(0, endPos);
            return result;
        }

        public string GetNewestRevisionId()
        {
            var magicString = "secret-revisions/";
            return ParseString(CurrentRevision, magicString, "/");
        }

        public string GetSecretId()
        {
            var magicString = "secrets/";
            return ParseString(ApiUrl, magicString, "/");
        }

        public Dictionary<string, string> ToDictionary()
        {
            var formData = new Dictionary<string, string>
            {
                { "name", Name }
            };
            if (!string.IsNullOrEmpty(AccessPolicy))
            {
                // access_policy
                formData.Add("access_policy", "1");
            }

            if (AllowedGroups != null && AllowedGroups.Count > 0)
            {
                //formData.Add("allowed_groups", string.Join(",", allowed_groups));
                formData.Add("allowed_groups", "");
            }

            if (!string.IsNullOrEmpty(Status))
            {
                formData.Add("status", Status);
            }

            if (AllowedUsers != null && AllowedUsers.Count > 0)
            {
                //formData.Add("allowed_users", string.Join(",", allowed_users));
                formData.Add("allowed_users", "206");
            }

            if (!string.IsNullOrEmpty(Password))
            {
                formData.Add("password", Password);
            }

            if (!string.IsNullOrEmpty(Username))
            {
                formData.Add("username", Username);
            }

            if (!string.IsNullOrEmpty(Description))
            {
                formData.Add("description", Description);
            }

            if (!string.IsNullOrEmpty(Url))
            {
                formData.Add("url", Url);
            }

            /*
            if (!string.IsNullOrEmpty(needs_changing_on_leave))
            {
                formData.Add("needs_changing_on_leave", needs_changing_on_leave == "true" ? "on" : "off");
            }
            */

            if (NotifyOnAccessRequest != null && NotifyOnAccessRequest.Count > 0)
            {
                //formData.Add("notify_on_access_request", string.Join(",", notify_on_access_request));
            }

            return formData;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}