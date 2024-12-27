using E_Learning.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Learning.Data
{
    [Table("Training")]
    public class Training
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? FilePath { get; set; }

        // Foreign key to associate with AspNetUser
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }

        // Navigation property for the associated user
        public ApplicationUser User { get; set; }

        // Collection of uploaded files
        public ICollection<Lecture> Lectures { get; set; }
    }
}
