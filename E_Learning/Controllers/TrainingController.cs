using Microsoft.AspNetCore.Mvc;
using E_Learning.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace E_Learning.Controllers
{
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Merrni të dhënat nga databaza
            var trainings = _context.Trainings.ToList();

            // Kthejeni të dhënat në View
            return View(trainings);  // Kjo kalon listën e trajnimeve në View
        }


        [HttpGet]
        [Route("api/training")]
        public async Task<IActionResult> GetAll()
        {
            var trainings = await _context.Trainings.ToListAsync();
            return Ok(trainings);  // Kthe të dhënat në format JSON
        }
    }
}