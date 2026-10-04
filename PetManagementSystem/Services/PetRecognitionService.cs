using System.Net.Http.Headers;
using System.Net.Http.Json;
using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public class PetRecognitionService(
    HttpClient httpClient,
    ILogger<PetRecognitionService> logger) : IPetRecognitionService
{
    public async Task<PetRecognitionResponse> RecognizeAsync(IFormFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();

            await using var stream = file.OpenReadStream();

            using var fileContent = new StreamContent(stream);

            if (!string.IsNullOrEmpty(file.ContentType))
            {
                fileContent.Headers.ContentType =
                    new MediaTypeHeaderValue(file.ContentType);
            }

            content.Add(
                fileContent,
                "file",
                file.FileName);

            var response = await httpClient.PostAsync(
                "/api/ai/recognize",
                content);

            response.EnsureSuccessStatusCode();

            var result = await response.Content
                .ReadFromJsonAsync<PetRecognitionResponse>();

            if (result is null)
            {
                throw new InvalidOperationException(
                    "AI service returned an empty response.");
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Pet recognition failed.");

            throw;
        }
    }
}