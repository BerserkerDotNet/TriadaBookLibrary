using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using TriadaBookLibrary.Models;

namespace TriadaBookLibrary
{
    public class TriadaBookClient
    {
        private readonly HttpClient _client;

        public TriadaBookClient(HttpClient client)
        {
            this._client = client;
        }

        public Task<Book[]> GetBooks()
        {
            return _client.GetFromJsonAsync<Book[]>("Books");
        }

        public Task<Category[]> GetCategories()
        {
            return _client.GetFromJsonAsync<Category[]>("Categories");
        }

        public Task<string> DownloadUrl(string category, string name)
        {
            return _client.GetFromJsonAsync<string>($"Link?name={name}&category={category}");
        }

        public void SetAccessCode(string accessCode)
        {
            _client.DefaultRequestHeaders.Remove("X-Access-Code");
            _client.DefaultRequestHeaders.Add("X-Access-Code", accessCode);
        }
    }
}
