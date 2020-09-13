using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TriadaBookLibrary.Models;

namespace TriadaBookLibrary.Functions
{
    public static class Categories
    {
        const string CategoriesBlob = "categories.json";

        [FunctionName(nameof(Categories))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            var config = ConfigurationUtils.BuildConfiguration(context.FunctionAppDirectory);
            if (!req.ValidateAccessCode(config))
            {
                return new BadRequestResult();
            }
            var json = await MetadataUtils.Download(CategoriesBlob, config);
            var result = JsonConvert.DeserializeObject<Category[]>(json);
            return new JsonResult(result);
        }
    }
}
