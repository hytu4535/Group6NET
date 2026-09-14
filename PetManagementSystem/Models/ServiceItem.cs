using System.ComponentModel.DataAnnotations;

namespace PetManagementSystem.Models
{
    public class ServiceItem
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // Tắm, cắt tỉa lông, khám bệnh, tiêm phòng...

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
