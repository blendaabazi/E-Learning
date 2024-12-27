using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using E_Learning.Data;
namespace E_Learning.Models
{
    public class Lecture
    {
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; } // File name for display purposes

        [Required]
        public string FilePath { get; set; } // File path for storage

        // Foreign key to associate with Training
        [Required]
        public int TrainingId { get; set; }

        // Navigation property
        public Training Training { get; set; }
    }
}
