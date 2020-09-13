using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Sas;
using Azure.Storage;

namespace TriadaBookLibrary.Functions
{
    public static class Link
    {
        const string BooksContainer = "books";

        [FunctionName(nameof(Link))]
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

            var accountName  = config["AccountName"];
            var category = req.Query["category"];
            var name = req.Query["name"];
            var blobName = $"{category}/{name}.txt";

            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = BooksContainer,
                BlobName = blobName,
                Resource = "b",
                ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
            };

            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(accountName, config["AccountKey"])).ToString();

            var fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = string.Format("{0}.blob.core.windows.net", accountName), // Urggg
                Path = string.Format("{0}/{1}", BooksContainer, blobName),
                Query = sasToken
            };

            return new OkObjectResult(fullUri.Uri);
        }
    }
}
