using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using TeamVaultRestApi.dto;

namespace TeamVaultRestApi
{
    public class TeamVaultWrapper
    {
        private readonly string _baseUrl;

        public TeamVaultWrapper(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public bool Authenticated
        {
            get => _authenticated;
        }

        private string UserName { get; set; }
        private string Password { get; set; }
        private static string CredentialTarget = "TeamVaultAccess";
        private bool _authenticated;

        private string csrftoken;

        private HttpClient client;

        private void ExtractCsrfToken(HttpResponseMessage response, bool updateDefault)
        {
            var cookie = response.Headers.ToList().FirstOrDefault(a => a.Key == "Set-Cookie");

            if (cookie.Value != null)
            {
                var entries = cookie.Value.First().Split(';');
                foreach (var entry in entries)
                {
                    var parts = entry.Split('=');
                    if (parts.Length == 2 && parts[0] == "csrftoken")
                        csrftoken = parts[1];
                }

                if (!string.IsNullOrEmpty(csrftoken) && updateDefault)
                {
                    client.DefaultRequestHeaders.Remove("csrfmiddlewaretoken");
                    client.DefaultRequestHeaders.Add("csrfmiddlewaretoken", csrftoken);
                }

            }
        }

        public bool Authenticate(string userName, string password)
        {
            UserName = userName;
            Password = password;
            client = new HttpClient();

            var response = client.GetAsync(_baseUrl).Result;
            ExtractCsrfToken(response, true);

            //client.DefaultRequestHeaders.Add("authority");
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("csrfmiddlewaretoken", csrftoken)
            });

            client.DefaultRequestHeaders.Add("origin", _baseUrl);
            client.DefaultRequestHeaders.Add("referer", _baseUrl);
            response = client.PostAsync(_baseUrl + "login", formContent).Result;
            var body = response.Content.ReadAsStringAsync().Result;
            ExtractCsrfToken(response, true);

            if (body.Contains("TeamVault &middot; Login"))
                return false;

            if (!response.IsSuccessStatusCode) 
                return false;

            return _authenticated = true;
        }

        /*
        public TeamVaultSecret GetSecret(string id)
        {
            //var response = client.GetAsync($"/secrets/{id}/?format=json&").Result;
            var response = client.GetAsync(_baseUrl + $"api/secrets/{id}?format=json").Result;
            response.EnsureSuccessStatusCode();
            ExtractCsrfToken(response, true);
            var resultBody = response.Content.ReadAsStringAsync().Result;
            var secret = JsonConvert.DeserializeObject<TeamVaultSecret>(resultBody);
            return secret;
        }
        */
        public TeamVaultSecret GetSecret(string id)
        {
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password)));
            using (var getClient = new HttpClient()
                   {
                       DefaultRequestHeaders = { Authorization = authValue }
                   })
            {
                string url = _baseUrl;
                getClient.BaseAddress = new Uri(url);

                var response = getClient.GetAsync($"/api/secrets/{id}/?format=json&").Result;
                response.EnsureSuccessStatusCode();
                var resultBody = response.Content.ReadAsStringAsync().Result;
                var secret = JsonConvert.DeserializeObject<TeamVaultSecret>(resultBody);
                return secret;
            }
        }


        public TeamVaultSearchResponse SearchSecret(string search, bool exact, int? page = null)
        {
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password)));
            using (var httpClient = new HttpClient()
                   {
                       DefaultRequestHeaders = { Authorization = authValue }
                   })
            {
                string url = _baseUrl;
                httpClient.BaseAddress = new Uri(url);

                string uri = $"api/secrets/?format=json&search={search}";
                if (page.HasValue)
                    uri += $"&page={page.Value}";

                var response = httpClient.GetAsync(uri).Result;
                response.EnsureSuccessStatusCode();
                var resultBody = response.Content.ReadAsStringAsync().Result;
                var secret = JsonConvert.DeserializeObject<TeamVaultSearchResponse>(resultBody);
                if (exact)
                {
                    secret.Results.RemoveAll(a => a.Name != search);
                }
                return secret;
            }


            /*
            string uri = _baseUrl + $"api/secrets/?search={WebUtility.UrlEncode(search)}&format=json";
            //string uri = _baseUrl + $"secrets/live-search?q={WebUtility.UrlEncode(search)}";
            if (page.HasValue)
                uri += $"&page={page.Value}";
            //client.DefaultRequestHeaders.Remove("referer");
            //client.DefaultRequestHeaders.Remove("origin");

            var response = client.GetAsync(uri).Result;
            response.EnsureSuccessStatusCode();
            var resultBody = response.Content.ReadAsStringAsync().Result;
            var secret = JsonConvert.DeserializeObject<List<TeamVaultSearchDto>>(resultBody);
            if (exact)
            {
                secret.RemoveAll(a => a.name != search);
            }
            return secret;
            */
        }

        public static string GetUpdateCredentialStoredPassword(string username, string newPassword)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            var all = new List<Windows.Security.Credentials.PasswordCredential>();
            foreach (var a in vault.RetrieveAll())
            {
                all.Add(a);
            }

            var existing = all.FirstOrDefault(a => a.Resource == CredentialTarget && a.UserName == username);
            if (existing != null && string.IsNullOrEmpty(existing.Password))
            {
                existing = vault.Retrieve(existing.Resource, existing.UserName);
            }


            // update password
            if (!string.IsNullOrEmpty(newPassword))
            {
                if (existing == null)
                {
                    // insert 
                    vault.Add(new Windows.Security.Credentials.PasswordCredential
                    {
                        UserName = username,
                        Password = newPassword,
                        Resource = CredentialTarget,
                    });
                    return newPassword;
                }
                existing.Password = newPassword;
                vault.Remove(existing);
                vault.Add(existing);
                return newPassword;
            }

            if (existing == null)
            {
                newPassword = Prompt.ShowDialog("Password", "Plz Authenticate for Teamvault Access");
                if (string.IsNullOrEmpty(newPassword))
                    return newPassword;
                vault.Add(new Windows.Security.Credentials.PasswordCredential
                {
                    UserName = username,
                    Password = newPassword,
                    Resource = CredentialTarget,
                });
                return newPassword;
            }

            return existing.Password;
        }

        public bool Authenticate(string username, bool addIfNew)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();

            var all = new List<Windows.Security.Credentials.PasswordCredential>();
            foreach (var a in vault.RetrieveAll())
            {
                all.Add(a);
            }

            var existing = all.FirstOrDefault(a => a.Resource == CredentialTarget && a.UserName == username);

            if (existing != null && string.IsNullOrEmpty(existing.Password))
            {
                existing = vault.Retrieve(existing.Resource, existing.UserName);
            }

            if (existing != null && !string.IsNullOrEmpty(existing.Password))
            {
                return Authenticate(existing.UserName, existing.Password);
            }

            // stored credential not found - add it?
            if (!addIfNew)
                return false;

            string promptValue = Prompt.ShowDialog("Password", "Plz Authenticate for Teamvault Access");
            if (existing != null)
            {
                existing.Password = promptValue;
                vault.Remove(existing);
                vault.Add(existing);
            }
            else
            {
                vault.Add(new Windows.Security.Credentials.PasswordCredential
                {
                    UserName = username,
                    Password = promptValue,
                    Resource = CredentialTarget,
                });

            }

            UserName = username;
            Password = promptValue;

            return Authenticate(this.UserName, this.Password);
        }

        private string CreateFormString(TeamVaultSecret secret)
        {
            var formString = "&name=" + WebUtility.UrlEncode(secret.Name) +
                "&description=" + WebUtility.UrlEncode(secret.Description) +
                "&password=" + WebUtility.UrlEncode(secret.Password) +
                "&username=" + WebUtility.UrlEncode(secret.Username) +
                "&url=" + WebUtility.UrlEncode(secret.Url) +
                "&access_policy=1"+
                "&needs_changing_on_leave=on&"+
                "allowed_groups="+ResolveGroups(secret.AllowedGroups)+
                "&notify_on_access_request=on"+
                "&allowed_users="+ResolveUsers(secret.AllowedUsers);
            return formString;
        }

        public TeamVaultSecret AddSecretUi(TeamVaultSecret secret)
        {
            var gResponse = client.GetAsync(_baseUrl + $"secrets/add/password").Result;
            ExtractCsrfToken(gResponse, true);
            var formString = CreateFormString(secret);
            var s = new StringContent("csrfmiddlewaretoken=" + csrftoken + formString, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = client.PostAsync(_baseUrl + $"secrets/add/password", s).Result;
            var resultBody = response.Content.ReadAsStringAsync().Result;
            var id = resultBody.Substring(resultBody.IndexOf("/edit")-6,6);
            ExtractCsrfToken(response, true);
            return GetSecret(id);
        }

        public TeamVaultSecret AddSecret(TeamVaultSecret secret)
        {
            var authValue = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(this.UserName + ":" + this.Password)));
            using (var addSecretClient = new HttpClient()
                   {
                       DefaultRequestHeaders = { Authorization = authValue }
                   })
            {
                string url = _baseUrl;
                addSecretClient.BaseAddress = new Uri(url);
                var content = new StringContent(secret.ToJson(), Encoding.UTF8, "application/json");
                var response = addSecretClient.PostAsync($"/api/secrets/?format=json&", content).Result;
                //response.EnsureSuccessStatusCode();
                var resultBody = response.Content.ReadAsStringAsync().Result;
                var result = JsonConvert.DeserializeObject<TeamVaultSecret>(resultBody);
                return result;
            }
        }

        public string GetPasswordForSecret(TeamVaultSecret secret)
        {
            var newId = secret.GetNewestRevisionId();
            secret.Password = GetPasswordForSecret(newId);
            return secret.Password;
        }

        public string GetPasswordForSecret(string id)
        {
            if (string.IsNullOrEmpty(id))
                return string.Empty;

            string uri = _baseUrl + $"api/secret-revisions/{id}/data?format=json";

            var response = client.GetAsync(uri).Result;
            response.EnsureSuccessStatusCode();
            var resultBody = response.Content.ReadAsStringAsync().Result;
            var passwordResponse = JsonConvert.DeserializeObject<GetPasswordResponse>(resultBody);
            return passwordResponse?.Password ?? string.Empty;
        }

        public void DeleteSecret(TeamVaultSecret secret)
        {
            DeleteSecret(secret.GetSecretId());
        }

        public void DeleteSecret(string secretId)
        {
            var gResponse = client.GetAsync(_baseUrl + $"secrets/{secretId}").Result;
            ExtractCsrfToken(gResponse, true);

            var response = client.DeleteAsync(_baseUrl + $"api/secrets/{secretId}/?format=json&").Result;
            var resultBody = response.Content.ReadAsStringAsync().Result;
        }

        public TeamVaultSecret UpdateSecret(TeamVaultSecret secret)
        {
            var id = secret.GetSecretId();

            var firstResponse = client.GetAsync(_baseUrl + $"secrets/{secret.GetSecretId()}/edit").Result;
            ExtractCsrfToken(firstResponse, true);

            var formString = CreateFormString(secret);
            var s = new StringContent("csrfmiddlewaretoken=" + csrftoken + formString, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = client.PostAsync(_baseUrl + $"secrets/{secret.GetSecretId()}/edit", s).Result;
            ExtractCsrfToken(response, true);
            var resultBody = response.Content.ReadAsStringAsync().Result;
            return GetSecret(id);
        }

        public List<TeamVaultEntity> TeamVaultUserCache = new List<TeamVaultEntity>();
        public List<TeamVaultEntity> TeamVaultGroupCache = new List<TeamVaultEntity>();

        public List<TeamVaultEntity> SearchGroup(string name)
        {
            var response = client.GetAsync(_baseUrl + $"search/groups?q={WebUtility.UrlEncode(name)}").Result;
            ExtractCsrfToken(response, true);
            var resultBody = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TeamVaultEntitySearchResponse>(resultBody)?.Results;
        }

        public List<TeamVaultEntity> SearchUser(string name)
        {
            var response = client.GetAsync(_baseUrl + $"search/users?q={WebUtility.UrlEncode(name)}").Result;
            ExtractCsrfToken(response, true);
            var resultBody = response.Content.ReadAsStringAsync().Result;
            return JsonConvert.DeserializeObject<TeamVaultEntitySearchResponse>(resultBody)?.Results;
        }

        public string ResolveNames(List<string> names, List<TeamVaultEntity> cache, Func<string, List<TeamVaultEntity>> resolveMethod)
        {
            List<int> entityIds = new List<int>();
            foreach (var name in names)
            {
                var cached = cache.FirstOrDefault(a => a.Text == name);
                if (cached == null)
                {
                    var users = resolveMethod(name);
                    if (users.Count == 1)
                    {
                        cache.Add(users[0]);
                        entityIds.Add(users[0].Id);
                    }
                    else
                    {
                        Console.WriteLine("cannot find entity " + name);
                    }
                    continue;
                }
                entityIds.Add(cached.Id);
            }

            switch (entityIds.Count)
            {
                case 0:
                    return string.Empty;
                case 1:
                    return entityIds[0].ToString();
                default:
                    return string.Join(",", entityIds);
            }
        }

        public string ResolveUsers(List<string> userNames)
        {
            return ResolveNames(userNames, TeamVaultUserCache, SearchUser);
        }

        public string ResolveGroups(List<string> groupNames)
        {
            return ResolveNames(groupNames, TeamVaultGroupCache, SearchGroup);
        }
    }
}


