using System;

namespace PetManagementSystem.Models
{
    public class PetImage
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsAvatar { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public Pet? Pet { get; set; }
    }
}