using Microsoft.EntityFrameworkCore;

namespace ELearning_Backend.Models
{
    public class ELContext : DbContext
    {
            public ELContext(DbContextOptions<ELContext> options) : base(options)
            {
            }
            
        
    }
}
