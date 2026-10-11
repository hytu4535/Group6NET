using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace PetManagementSystem.ViewModels;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class RecognitionImageFileAttribute : ValidationAttribute
{
    public const long MaximumFileSizeBytes = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    public override bool IsValid(object? value)
    {
        if (value is null)
        {
            return true;
        }

        if (value is not IFormFile file)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        return file.Length > 0 &&
               file.Length <= MaximumFileSizeBytes &&
               AllowedExtensions.Contains(extension);
    }

    public override string FormatErrorMessage(string name) =>
        "Ảnh phải có định dạng JPG, JPEG, PNG hoặc WEBP và không vượt quá 5 MB.";
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class FiniteNumberAttribute : ValidationAttribute
{
    public override bool IsValid(object? value) =>
        value is null || value is double number && double.IsFinite(number);
}
