using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace VoiceVaultAPI.Client
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the file path as an argument.");
                return;
            }

            var filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return;
            }

            var result = await UploadFileAsync(filePath);
            Console.WriteLine(result);
        }

        private static async Task<string> UploadFileAsync(string filePath)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            content.Add(fileContent, "file", Path.GetFileName(filePath));

            var response = await client.PostAsync("https://yourapiurl.com/upload", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }
    }
}