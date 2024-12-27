using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Learning.Data;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using E_Learning.Models;

namespace E_Learning.Controllers
{
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // MVC Index: Displays a list of trainings
        public IActionResult Index()
        {
            var trainings = _context.Trainings
                .Include(t => t.User) // Include the related professor (user)
                .ToList();

            var users = _context.Users.ToList();

            ViewBag.Trainings = trainings;
            ViewBag.Users = users;

            return View();
        }

        // API GET: Retrieve all training records
        [HttpGet]
        [Route("api/training")]
        public async Task<IActionResult> GetAll()
        {
            var trainings = await _context.Trainings
                .Include(t => t.User) // Include the professor details
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    ProfessorName = t.User.UserName // Get the username of the professor
                })
                .ToListAsync();
            return Ok(trainings);
        }

        // API POST: Create a new training record (Admin role required)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [Route("api/training")]
        public async Task<IActionResult> CreateTraining([FromBody] TrainingDto trainingDto)
        {
            // Validate the incoming data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FindAsync(trainingDto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var training = new Training
            {
                Name = trainingDto.Name,
                FilePath = trainingDto.FilePath,
                UserId = trainingDto.UserId,
                User = user
            };

            try
            {
                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetTraining), new { id = training.Id }, training);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error creating training: {ex.Message}" });
            }
        }

        // API PUT: Update an existing training record (Admin role required)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTraining(int id, [FromForm] TrainingUpdateModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var training = await _context.Trainings.FindAsync(id);
            if (training == null)
            {
                return NotFound();
            }

            // Update properties
            training.Name = model.Name;

            if (model.File != null)
            {
                var fileName = Path.GetFileName(model.File.FileName);
                var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                //var fullPath = Path.Combine(fileDirectory, fileName);
                //using (var stream = new FileStream(fullPath, FileMode.Create))
                //{
                //    await model.File.CopyToAsync(stream);
                //}
                //training.FilePath = Path.Combine("Uploads", fileName);  // Store relative path to file
            }

            _context.Entry(training).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();  // Indicates the update was successful, but no content to return
        }


        // API GET: Retrieve a single training record by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTraining(int id)
        {
            var training = await _context.Trainings
                .Include(t => t.User) // Include professor/user data
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound(new { message = "Training not found." });
            }

            return Ok(training);
        }
        [HttpPost]
        [Route("api/upload")]
        public async Task<IActionResult> UploadFile(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a valid file.");
                return RedirectToAction("Details", new { id });
            }

            var training = await _context.Trainings
                .Include(t => t.Lectures)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            // Save file to a specific folder
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Create a new TrainingFile record
            var trainingFile = new Lecture
            {
                FileName = file.FileName,
                FilePath = $"/uploads/{uniqueFileName}",
                TrainingId = id
            };

            _context.Lectures.Add(trainingFile);
            await _context.SaveChangesAsync();

            TempData["Message"] = "File uploaded successfully.";
            return RedirectToAction("Details", new { id });
        }


        public IActionResult Details(int id)
        {
            var training = _context.Trainings
                .Include(t => t.User) // Include related user data
                .Include(t => t.Lectures) // Include related lectures
                .FirstOrDefault(t => t.Id == id);

            if (training == null)
            {
                return NotFound();
            }

            return View(training);
        }



        // API GET: Retrieve all training records with file path and professor name
        [HttpGet("sss")]
        public async Task<IActionResult> GetTrainings()
        {
            var trainings = await _context.Trainings
                .Include(t => t.User)
                .ToListAsync();

            var trainingDtos = trainings.Select(t => new
            {
                t.Id,
                t.Name,
                t.FilePath,
                UserName = t.User?.UserName ?? "Not Assigned"
            });

            return Ok(trainingDtos);
        }

        // API GET: Retrieve all users (Admin role required)
        [Authorize(Roles = "Admin")]
        [HttpGet("api/users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.UserName })
                .ToListAsync();
            return Ok(users);
        }

        // API DELETE: Delete a training record (Admin role required)
        [Authorize(Roles = "Admin")]
        [HttpDelete]
        [Route("api/training/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var training = await _context.Trainings.FindAsync(id);
            if (training == null)
            {
                return NotFound(new { message = "Training not found." });
            }

            _context.Trainings.Remove(training);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO for training creation and updates
    public class TrainingDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? FilePath { get; set; }

        [Required]
        public string UserId { get; set; }
    }

    public class TrainingUpdateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile? File { get; set; } // Nullable file for file uploads
    }

}
