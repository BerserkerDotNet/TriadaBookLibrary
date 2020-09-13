using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TriadaBookLibrary.DataUploader
{
    class Program
    {
        const string BooksContainer = "books";
        const string MetadataContainer = "metadata";
        const string StorageConnectionStringName = "StorageConnection";

        static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(Environment.CurrentDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
                 .Build();

            await PublishWebSite(config);
        }

        private static async Task UploadMetadata(IConfigurationRoot config)
        {
            var container = new BlobContainerClient(config[StorageConnectionStringName], MetadataContainer);
            await container.CreateIfNotExistsAsync();

            var libraryPath = config["DirectoryPath"];
            var categoriesFile = Path.Combine(libraryPath, "cat.json");
            var booksFile = Path.Combine(libraryPath, "book.json");

            try
            {
                var blobClient = container.GetBlobClient("categories.json");
                await blobClient.UploadAsync(categoriesFile);

                blobClient = container.GetBlobClient("books.json");
                await blobClient.UploadAsync(booksFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to process {ex.Message}");
            }
        }

        private static async Task UploadBooks(IConfigurationRoot config)
        {
            var container = new BlobContainerClient(config[StorageConnectionStringName], BooksContainer);
            await container.CreateIfNotExistsAsync();

            var libraryPath = config["BooksPath"];
            var books = Directory.GetFiles(libraryPath, "*.zip", SearchOption.AllDirectories);

            var idx = 1;
            foreach (var book in books)
            {
                try
                {
                    var fileInfo = new FileInfo(book);
                    var relPath = book
                        .Replace(libraryPath, "")
                        .Replace("\\", "/")
                        .Replace(".zip", ".txt");
                    var blobClient = container.GetBlobClient(relPath);
                    ReportProgress($"Uploading {relPath} {idx++}/{books.Length}");
                    using var archive = ZipFile.Open(book, ZipArchiveMode.Read);
                    var entry = archive.Entries[0];
                    using var fileStream = entry.Open();

                    using var conversionStream = new MemoryStream();
                    await fileStream.CopyToAsync(conversionStream);
                    var rawDataBytes = conversionStream.ToArray();
                    var utf8data = Encoding.Convert(Encoding.GetEncoding(1251), Encoding.UTF8, rawDataBytes);
                    using var dataStream = new MemoryStream(utf8data);

                    await blobClient.UploadAsync(dataStream, overwrite: true);
                    // await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = GetContentType(".txt") });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process '{book}'. {ex.Message}");
                }
            }
        }

        private static async Task PublishWebSite(IConfigurationRoot config)
        {
            var container = new BlobContainerClient(config[StorageConnectionStringName], "$web");
            await container.CreateIfNotExistsAsync();

            var publishPath = config["PublishFolder"];
            var files = Directory.GetFiles(publishPath, "*.*", SearchOption.AllDirectories);

            var idx = 1;
            foreach (var file in files)
            {
                try
                {
                    var fileInfo = new FileInfo(file);
                    var relPath = file
                        .Replace(publishPath, "")
                        .Replace("\\", "/");
                    var blobClient = container.GetBlobClient(relPath);
                    ReportProgress($"Uploading {idx++}/{files.Length} {relPath} ");
                    await blobClient.UploadAsync(file, overwrite: true);
                    await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = GetContentType(fileInfo.Extension) });
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to process '{file}'. {ex.Message}");
                }
            }
        }

        private static void ReportProgress(string message)
        {
            var prevPosition = (Console.CursorLeft, Console.CursorTop);
            Console.SetCursorPosition(0, 0);
            Console.WriteLine(message.PadRight(Console.BufferWidth));
            Console.SetCursorPosition(prevPosition.CursorLeft, prevPosition.CursorTop);
        }

        private static string GetContentType(string ext)
        {
            switch (ext.ToLower())
            {
                case ".html":
                case ".htm":
                    return "text/html";
                case ".js":
                    return "text/javascript";
                case ".css":
                    return "text/css";
                case ".txt":
                    return "text/plain";
                case ".wasm":
                    return "application/wasm";
                case ".ico":
                    return "image/x-icon";
                case ".png":
                    return "image/png";
                case ".svg":
                    return "image/svg+xml";
                case ".json":
                    return "application/json";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
