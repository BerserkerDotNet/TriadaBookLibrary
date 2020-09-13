using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TriadaBookLibrary.Models;
using Newtonsoft.Json;

namespace TriadaBookLibrary.Functions
{
    public static class Books
    {
        const string BooksBlob = "books.json";

        [FunctionName(nameof(Books))]
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

            var json = await MetadataUtils.Download(BooksBlob, config);
            var result = JsonConvert.DeserializeObject<Book[]>(json);
            return new JsonResult(result);
        }
    }
}
