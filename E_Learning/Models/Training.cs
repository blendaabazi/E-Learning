using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
namespace E_Learning.Data
{
    [Table("Training")]
    public class Training
    {
        public int Id{ get; set; }
        [Required]
        public string Name { get; set; }
    }
}
