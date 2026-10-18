namespace PetManagementSystem.Services;

public enum PetRecognitionConfirmationStatus
{
    Confirmed,
    InvalidImage,
    StaffNotFound,
    PetNotFound
}

public sealed record PetRecognitionConfirmationResult(
    PetRecognitionConfirmationStatus Status,
    string Message,
    int? HistoryId = null,
    int? PetId = null,
    string? ImageUrl = null);
