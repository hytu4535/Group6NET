using PetManagementSystem.ViewModels;

namespace PetManagementSystem.Services;

public interface IPetRecognitionService
{
    Task<PetRecognitionResponse> RecognizeAsync(
        IFormFile file,
        CancellationToken cancellationToken);

    Task<PetRecognitionConfirmationResult> ConfirmAsync(
        IFormFile file,
        int petId,
        int userId,
        double? similarity,
        CancellationToken cancellationToken);

    Task<PetRecognitionHistoryPageViewModel> GetRecognitionHistoriesAsync(
        PetRecognitionHistoryFilter filter,
        CancellationToken cancellationToken);
}