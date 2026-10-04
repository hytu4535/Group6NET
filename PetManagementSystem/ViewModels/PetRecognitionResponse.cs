using System.Text.Json.Serialization;

namespace PetManagementSystem.ViewModels;

public class PetRecognitionResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("pet_id")]
    public string? PetId { get; set; }

    [JsonPropertyName("similarity")]
    public double? Similarity { get; set; }

    [JsonPropertyName("candidates")]
    public List<PetRecognitionCandidate> Candidates { get; set; } = [];
}

public class PetRecognitionCandidate
{
    [JsonPropertyName("pet_id")]
    public string PetId { get; set; } = string.Empty;

    [JsonPropertyName("similarity")]
    public double Similarity { get; set; }
}