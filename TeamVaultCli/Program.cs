using System;
using System.Collections.Generic;
using CommandLine;
using TeamVaultRestApi;

namespace TeamVaultRest
{
    partial class Program
    {
        private static void Validate(Options o)
        {
            if (string.IsNullOrEmpty(o.TeamVaultUser))
            {
                Console.WriteLine("please provide a TeamVault user id ");
                Environment.ExitCode = -1;
                return;
            }
            PasswordCheck(o);

            if (string.IsNullOrEmpty(o.SecretName))
            {
                Console.WriteLine("please provide a secret name for reading out");
                Environment.Exit(-1);
            }
        }


        private static void Read(Options o)
        {
            var api = new TeamVaultWrapper(o.TeamVaultUrl);
            Validate(o);
            if (!api.Authenticate(o.TeamVaultUser, Password))
            {
                Console.WriteLine("Authentication Failed for user "+o.TeamVaultUser);
                Environment.Exit(-1);
            }
            var existing = api.SearchSecret(o.SecretName, true);
            if (existing.Results.Count != 1)
            {
                Console.WriteLine("cannot identify secret " + o.SecretName + " for update");
                Environment.Exit(-1);
            }
            var secret = api.GetPasswordForSecret(existing.Results[0].GetNewestRevisionId());
            Console.WriteLine(secret);
        }

        public static string Password;
        private static void PasswordCheck(Options o)
        {
            // password is provided -> we will store it :-)
            if (o.TeamVaultPasswordStore)
            {
                Password = TeamVaultWrapper.GetUpdateCredentialStoredPassword(o.TeamVaultUser, o.TeamVaultPassword);
                if (string.IsNullOrEmpty(Password))
                {
                    Console.WriteLine("TeamVault password not available");
                    Environment.Exit(-1);
                }

                return;
            }

            Password = o.TeamVaultPassword;
            if (string.IsNullOrEmpty(Password))
            {
                Console.WriteLine("TeamVault password not available");
                Environment.Exit(-1);
            }

        }

        static TeamVaultSecret PrepareSecret(Options options, TeamVaultWrapper api)
        {
            if (!string.IsNullOrEmpty(options.SecretTemplateId))
            {
                return api.GetSecret(options.SecretTemplateId);
            }

            if (_defaults != null && (_defaults.AllowedGroups?.Count > 0 || _defaults.AllowedUsers?.Count > 0))
            {
                return new TeamVaultSecret
                {
                    AllowedUsers = _defaults.AllowedUsers ?? new List<string>(),
                    AllowedGroups = _defaults.AllowedGroups ?? new List<string>()
                };
            }
            Console.WriteLine("secret template id not defined " + options.SecretTemplateId); 
            Environment.ExitCode = -1;
            return null;
        }

        public static void Update(Options o)
        {
            var api = new TeamVaultWrapper(o.TeamVaultUrl);
            Validate(o);

            if (!api.Authenticate(o.TeamVaultUser, Password))
            {
                Console.WriteLine("Authentication Failed for user " + o.TeamVaultUser);
                Environment.Exit(-1);
            }
            var existing = api.SearchSecret(o.SecretName, true);
            if (existing.Results.Count > 1)
            {
                Console.WriteLine("cannot identify secret " + o.SecretName + " for update");
                Environment.ExitCode = -1;
                return;
            }

            TeamVaultSecret secret;
            if (existing.Results.Count == 1)
            {
                secret = api.GetSecret(existing.Results[0].GetSecretId());
            }
            else
            {
                secret = PrepareSecret(o, api);
                secret.Name = o.SecretName;
            }

            if (!string.IsNullOrEmpty(o.SecretPassword))
            {
                secret.Password = o.SecretPassword;
            }

            if (!string.IsNullOrEmpty(o.SecretDescription))
            {
                secret.Description = o.SecretDescription;
            }

            if (!string.IsNullOrEmpty(o.SecretUrl))
            {
                secret.Url = o.SecretUrl;
            }

            if (!string.IsNullOrEmpty(o.SecretUser))
            {
                secret.Username = o.SecretUser;
            }

            TeamVaultSecret updated;
            if (existing.Results.Count == 0)
            {
                if (string.IsNullOrEmpty(secret.Password) || string.IsNullOrEmpty(secret.Name) || string.IsNullOrEmpty(secret.Password))
                {
                    Console.WriteLine("need to have username, password and name for a secret to add! "+secret.Name);
                    Environment.Exit(-1);
                }
                updated = api.AddSecret(secret);
                Console.WriteLine("added secret");
            }
            else
            {
                updated = api.UpdateSecret(secret);
            }

            if (!string.IsNullOrEmpty(o.SecretUrl))
            {
                if (secret.Url != updated.Url)
                {
                    Console.WriteLine("was not able to update url to "+secret.Url);
                    Environment.Exit(-1);
                }
            }
        }

        private static Defaults _defaults;

        static void Main(string[] args)
        {
            _defaults = Defaults.LoadDefaults();
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                if (o.Receive)
                {
                    Read(o);
                    return;
                }
                Console.WriteLine("TeamVault CLI 0.1");
                if (o.Send)
                {
                    Update(o);
                    return;
                }
            });
        }
    }

}
