using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Uploadify.Maui.Services
{
    public class UploadifyApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public UploadifyApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _cancellationTokenSource = new CancellationTokenSource();
            // Determine the API base URL based on the platform
            #if ANDROID
                var baseAddress = "http://10.0.2.2:5171/";
            #else
                var baseAddress = "http://10.0.0.141:5171/";
            #endif

            _httpClient.BaseAddress = new Uri(baseAddress);
        }

        public async Task<CreateSessionResponse> CreateUploadSessionAsync(string fileName, long fileSize)
        {
                        var createSessionUrl = $"{_httpClient.BaseAddress}api/createUploadSession?fileName={fileName}&fileSize={fileSize}";
            var sessionResponse = await _httpClient.PostAsync(createSessionUrl, null, _cancellationTokenSource.Token);
            sessionResponse.EnsureSuccessStatusCode();

            var sessionJson = await sessionResponse.Content.ReadAsStringAsync(_cancellationTokenSource.Token);
            var sessionData = JsonSerializer.Deserialize<CreateSessionResponse>(sessionJson);

            return sessionData;
        }

        public async Task<long> GetNextExpectedRangeAsync(string sessionId)
        {
            var response = await _httpClient.GetStringAsync($"api/upload/{sessionId}/nextrange");
            var nextRangeResponse = JsonSerializer.Deserialize<NextRangeResponse>(response);
            return nextRangeResponse.NextExpectedRangeStart;
        }

        public async Task UploadFileChunkAsync(Stream fileStream, string sessionId, long startRange, string fileName)
        {
            var buffer = new byte[8192];
            int bytesRead;
            while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var content = new ByteArrayContent(buffer, 0, bytesRead);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var response = await _httpClient.PutAsync($"api/upload/{sessionId}/chunk?start={startRange}", content);
                response.EnsureSuccessStatusCode();
                startRange += bytesRead;
            }
        }

        public async Task CompleteUploadAsync(string sessionId)
        {
            var completeUrl = $"api/upload/{sessionId}/complete";
            var response = await _httpClient.PostAsync(completeUrl, null);
            response.EnsureSuccessStatusCode();
        }

        public record CreateSessionResponse(string uploadUrl, DateTime expirationDateTime, string sessionId);
        public record NextRangeResponse(long NextExpectedRangeStart);
    }
}