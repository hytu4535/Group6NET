using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public interface IPetRecognitionService
{
    Task<PetRecognitionResponse> RecognizeAsync(IFormFile file);
}