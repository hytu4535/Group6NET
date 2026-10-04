public static class PetIdMappingService
{
    public static bool TryGetDatabasePetId(
        string aiPetId,
        out int DatabasePetId
    )
    {
        DatabasePetId = 0;
        if (string.IsNullOrWhiteSpace(aiPetId))
            return false;
        if (!aiPetId.StartsWith("PET"))
            return false;
        var numberPart = aiPetId["PET".Length..];
        return int.TryParse(numberPart, out DatabasePetId);
    }
}