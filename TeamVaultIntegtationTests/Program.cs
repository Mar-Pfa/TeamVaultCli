using System;
using System.Diagnostics;
using System.Linq;
using TeamVaultRestApi;

namespace TeamVaultIntegrationTests
{
    class Program
    {
        static void Main(string[] args)
        {

            var defaults = Defaults.LoadDefaults();
            var vault = new TeamVaultWrapper(defaults.TeamVaultUrl);

            if (!vault.Authenticate(Environment.UserName, true))
            {
                Console.WriteLine("authentication error");
                if (Debugger.IsAttached)
                    Console.ReadLine();

                return;
            }

            var secrets = vault.SearchSecret("TeamVault api | test", true);
            Console.WriteLine("found " + secrets.Results.Count);
            Console.WriteLine("found " + string.Join(",", secrets.Results.Select(a => a.Name)));

            var secretid = "jAVgJ6";
            var secret = vault.GetSecret(secretid);

            Console.WriteLine(string.Join(",", secret.AllowedUsers));
            Console.WriteLine(vault.ResolveUsers(secret.AllowedUsers));
            Console.WriteLine(string.Join(",", secret.AllowedGroups));
            Console.WriteLine(vault.ResolveGroups(secret.AllowedGroups));
            if (Debugger.IsAttached)
                Console.ReadLine();
            return;

            var pw = vault.GetPasswordForSecret(secret);
            Console.WriteLine(secretid);
            Console.WriteLine("name " + secret.Name);
            Console.WriteLine("user " + secret.Username);
            Console.WriteLine("pw   " + pw);
            Console.WriteLine("url  " + secret.Url);
            Console.WriteLine("web  " + secret.WebUrl);


            var newSecret = vault.AddSecretUi(new TeamVaultSecret
            {
                Name = "teamvault api | test",
                Username = "testuser",
                AccessPolicy = secret.AccessPolicy,
                AllowedGroups = secret.AllowedGroups,
                AllowedUsers = secret.AllowedUsers,
                NeedsChangingOnLeave = secret.NeedsChangingOnLeave,
                NotifyOnAccessRequest = secret.NotifyOnAccessRequest,
                Status = secret.Status,
                Description = "initial",
                Password = "blablabla"
            });
            Console.WriteLine("secret added " + newSecret.Name + " - " + newSecret.WebUrl + " - " + newSecret.GetSecretId());

            newSecret.Password = "updated";
            newSecret.Description = "updated";
            vault.UpdateSecret(newSecret);


            Console.WriteLine("secret added " + newSecret.Name + " - " + newSecret.WebUrl + " - " + newSecret.GetSecretId());

            newSecret = vault.GetSecret(newSecret.GetSecretId());
            vault.GetPasswordForSecret(newSecret);
            Console.WriteLine(newSecret.Description);


            vault.DeleteSecret(newSecret);
            Console.WriteLine("delete again...");

            if (Debugger.IsAttached)
                Console.ReadLine();
        }
    }
}
