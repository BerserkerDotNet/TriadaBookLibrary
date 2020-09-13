using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace TriadaBookLibrary.Functions
{
    public static class ConfigurationUtils
    {
        public static IConfigurationRoot BuildConfiguration(string basePath)
        {
            return new ConfigurationBuilder()
                 .SetBasePath(basePath)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                 .AddEnvironmentVariables()
                 .Build();
        }
    }
}
