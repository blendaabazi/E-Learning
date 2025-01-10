using E_Learning.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace E_Learning.Controllers
{
    [Authorize(Roles = "Professor")]
    public class DashboardProfController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DashboardProfController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("api/professor/students")]
        public async Task<IActionResult> GetStudents()
        {
            // Merr të gjithë përdoruesit nga databaza
            var users = await _userManager.Users.ToListAsync();

            // Lista për ruajtjen e studentëve
            var students = new List<object>();

            foreach (var user in users)
            {
                // Merr rolet për secilin përdorues
                var roles = await _userManager.GetRolesAsync(user);

                // Kontrollo nëse përdoruesi ka rolin "Student"
                if (roles.Contains("User"))
                {
                    students.Add(new
                    {
                        user.Id,
                        user.Name,
                        user.UserName,
                        user.Email,
                        user.PhoneNumber,
                        Role = roles.FirstOrDefault() ?? "No Role"
                    });
                }
            }

            // Kthe studentët në format JSON
            return Ok(students);
        }
      

        [HttpGet]
        [Route("api/professor/countLecture")]
        public async Task<IActionResult> GetLectureCount()
        {
            // Merr të gjitha trajnimet dhe përfshin ligjëratat për secilin trajnim
            var trainings = await _context.Trainings
                .Include(t => t.Lectures) // Përfshi ligjëratat për çdo trajnim
                .ToListAsync();

            // Lista që do të mbajë numrin e ligjëratave për çdo trajnim
            var lectureCounts = trainings.Select(t => new
            {
                TrainingId = t.Id,
                TrainingName = t.Name, // Ose ndonjë emër tjetër për trajnimet që përdorni
                LectureCount = t.Lectures.Count
            }).ToList();

            // Kthe një JSON me të dhënat e numrave të ligjëratave për çdo trajnim
            return Ok(lectureCounts);
        }
    }

}
