using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace VoiceVaultAPI.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var tempFilePath = Path.GetTempFileName();
            var tempFileContent = new byte[10 * 1024 * 1024]; // 10 MB file
            new Random().NextBytes(tempFileContent);
            File.WriteAllBytes(tempFilePath, tempFileContent);

            var apiUrl = "http://localhost:5171/api/upload";
            var completeUrl = "http://localhost:5171/api/complete";
            var chunkSize = 1024 * 1024; // 1 MB

            var fileName = Path.GetFileName(tempFilePath);
            var totalChunks = (int)Math.Ceiling((double)new FileInfo(tempFilePath).Length / chunkSize);

            using (var httpClient = new HttpClient())
            {
                for (int chunkNumber = 1; chunkNumber <= totalChunks; chunkNumber++)
                {
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.Seek((chunkNumber - 1) * chunkSize, SeekOrigin.Begin);
                        var buffer = new byte[chunkSize];
                        var bytesRead = await fileStream.ReadAsync(buffer, 0, chunkSize);

                        using (var content = new MultipartFormDataContent())
                        {
                            var chunkContent = new ByteArrayContent(buffer, 0, bytesRead);
                            chunkContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                            content.Add(chunkContent, "chunk", fileName);
                            content.Add(new StringContent(fileName), "fileName");
                            content.Add(new StringContent(chunkNumber.ToString()), "chunkNumber");

                            var response = await httpClient.PostAsync(apiUrl, content);
                            if (!response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Chunk {chunkNumber} upload failed: {response.ReasonPhrase}");
                                return;
                            }
                        }
                    }
                }

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(fileName), "fileName");

                    var response = await httpClient.PostAsync(completeUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("File uploaded successfully (simulated).");
                    }
                    else
                    {
                        Console.WriteLine($"File completion failed: {response.ReasonPhrase}");
                    }
                }
            }
        }
    }
}