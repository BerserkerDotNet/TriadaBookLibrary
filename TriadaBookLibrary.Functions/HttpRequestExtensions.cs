using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace TriadaBookLibrary.Functions
{
    public static class HttpRequestExtensions
    {
        public static bool ValidateAccessCode(this HttpRequest req, IConfigurationRoot config)
        {
            var code = req.Headers["X-Access-Code"];
            if (code.Count == 0)
            {
                return false;
            }

            return string.Equals(code[0], config["AccessCode"], System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
