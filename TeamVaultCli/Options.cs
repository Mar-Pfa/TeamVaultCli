using CommandLine;

namespace TeamVaultRest
{
    partial class Program
    {
        public class Options
        {
            [Option(longName: "api", Required = false, HelpText = "TeamVault Api address e.g. https://MyOwnTeamVault.MyDomain.xyz/ ")]
            public string TeamVaultUrl { get; set; }

            [Option(longName: "tvu", Required = true, HelpText = "username for authenticating against TeamVault Api")]
            public string TeamVaultUser { get; set; }

            [Option(longName: "tvp", Required = false, HelpText = "password for authenticating against TeamVault Api")]
            public string TeamVaultPassword { get; set; }

            [Option(longName: "tvps", Required = false, HelpText = "use password in the windows credential store or ask for it")]
            public bool TeamVaultPasswordStore { get; set; }

            [Option(shortName: 's', longName: "send", HelpText = "send update of given secret to TeamVault")]
            public bool Send { get; set; }

            [Option(shortName: 'n', longName: "name", HelpText = "name-field of the secret in TeamVault")]
            public string SecretName { get; set; }

            [Option(shortName: 'u', longName: "username", HelpText = "username-field of the secret in TeamVault")]
            public string SecretUser { get; set; }

            [Option(shortName: 'p', longName: "password", HelpText = "password-field of the secret in TeamVault")]
            public string SecretPassword { get; set; }

            [Option(longName: "url", HelpText = "url-field of the secret in TeamVault")]
            public string SecretUrl { get; set; }

            [Option(shortName: 'd', longName: "description", HelpText = "description-field of the secret in TeamVault e.g. 5Dlkg5")]
            public string SecretDescription { get; set; }

            [Option(shortName: 't', longName: "template", HelpText = "secret id of a template when adding new entries - security settings will be copied")]
            public string SecretTemplateId { get; set; }

            [Option(shortName: 'r', longName: "receive", HelpText = "receive password of a credential and return it")]
            public bool Receive { get; set; }
        }
    }
}