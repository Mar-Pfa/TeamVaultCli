using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace TeamVaultRestApi
{
    /// <summary>
    /// container class to bake defaults into the application
    /// </summary>
    public class Defaults
    {
        public string TeamVaultUrl { get; set; }

        [JsonProperty("allowed_users")] 
        public List<string> AllowedUsers { get; set; }

        [JsonProperty("allowed_groups")] 
        public List<string> AllowedGroups { get; set; }

        public static Defaults LoadDefaults()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith("defaults.json"));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    return new Defaults();
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    var defaults = JsonConvert.DeserializeObject<Defaults>(result);
                    if (defaults == null)
                    {
                        return new Defaults();
                    }
                    return defaults;
                } }

        }

    }
}