using System;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage;
using System.Text;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace TriadaBookLibrary.Functions
{
    public static class MetadataUtils
    {
        public static async Task<string> Download(string blobName, IConfigurationRoot configuration)
        {
            const string MetadataContainer = "metadata";
            var accountName = configuration["AccountName"];
            var uri = new Uri(configuration["Endpoint"]);
            var blobServiceClient = new BlobServiceClient(uri, new StorageSharedKeyCredential(accountName, configuration["AccountKey"]));
            var container = blobServiceClient.GetBlobContainerClient(MetadataContainer);
            var blobClient = container.GetBlobClient(blobName);

            var downloadResponse = await blobClient.DownloadAsync();
            using var downloadInfo = downloadResponse.Value;
            using var memoryStream = new MemoryStream((int)downloadInfo.ContentLength);
            await downloadInfo.Content.CopyToAsync(memoryStream);
            var json = Encoding.UTF8.GetString(memoryStream.ToArray());

            return json;
        }
    }
}
