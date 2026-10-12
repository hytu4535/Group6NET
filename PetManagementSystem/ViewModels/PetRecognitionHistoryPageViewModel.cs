namespace PetManagementSystem.ViewModels;

public sealed class PetRecognitionHistoryPageViewModel
{
    public required PetRecognitionHistoryFilter Filter { get; init; }
    public required IReadOnlyList<PetRecognitionHistoryItemViewModel> Items { get; init; }
    public required IReadOnlyList<PetRecognitionPetOptionViewModel> Pets { get; init; }
    public int TotalCount { get; init; }

    public int TotalPages => Filter.PageSize > 0
        ? (int)Math.Ceiling(TotalCount / (double)Filter.PageSize)
        : 0;

    public bool HasPreviousPage => Filter.Page > 1;
    public bool HasNextPage => Filter.Page < TotalPages;
}
