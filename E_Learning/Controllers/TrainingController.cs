using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Learning.Data;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using E_Learning.Models;
using E_Learning.Services;
using Newtonsoft.Json; // Sigurohu që klasa RedisCacheService është e importuar

namespace E_Learning.Controllers
{
    public class TrainingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RedisCacheService _redisCache;

        public TrainingController(ApplicationDbContext context, RedisCacheService redisCache)
        {
            _context = context;
            _redisCache = redisCache; // Injektimi i shërbimit RedisCacheService
        }

        // MVC Index: Displays a list of trainings
        public IActionResult Index()
        {
            var trainingsCacheKey = "trainings";
            var trainings = _redisCache.GetValueAsync(trainingsCacheKey).Result;

            if (string.IsNullOrEmpty(trainings))
            {
                var trainingList = _context.Trainings
                    .Include(t => t.User)
                    .ToList();

                if (trainingList.Count > 0)
                {
                    var serializedTrainings = JsonConvert.SerializeObject(trainingList);
                    _redisCache.SetValueAsync(trainingsCacheKey, serializedTrainings).Wait();
                    Console.WriteLine("Trainings saved to Redis successfully.");
                }

                var users = _context.Users.ToList();
                ViewBag.Users = users;
                ViewBag.Trainings = trainingList;
            }
            else
            {
                Console.WriteLine("Trainings retrieved from Redis.");
                try
                {
                    var trainingList = JsonConvert.DeserializeObject<List<Training>>(trainings);
                    if (trainingList == null)
                    {
                        ViewBag.Trainings = new List<Training>();
                    }
                    else
                    {
                        ViewBag.Trainings = trainingList;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error deserializing Redis data: " + ex.Message);
                    ViewBag.Trainings = new List<Training>();
                }
            }

            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("api/training/set")]
        public async Task<IActionResult> CreateTrainingAndSetCache([FromBody] TrainingDto trainingDto)
        {
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
                // Shto trajnim në databazë
                _context.Trainings.Add(training);
                await _context.SaveChangesAsync();

                // Pasi krijohet trajnim, pastroni cache për trajnime
                await _redisCache.SetValueAsync("trainings", null); // Pastroni cache të trajnimeve të vjetra

                // Ruaj trajnimet e reja në cache
                var trainingList = _context.Trainings.Include(t => t.User).ToList();
                await _redisCache.SetValueAsync("trainings", Newtonsoft.Json.JsonConvert.SerializeObject(trainingList), TimeSpan.FromMinutes(5));

                return CreatedAtAction(nameof(GetTraining), new { id = training.Id }, training);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error creating training: {ex.Message}" });
            }
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

                // Pasi krijohet trajnim, pastroni cache për trajnime
                await _redisCache.SetValueAsync("trainings", null); // Pastroni cache të trajnimeve të vjetra

                // Ruaj trajnimet e reja në cache
                var trainingList = _context.Trainings.Include(t => t.User).ToList();
                await _redisCache.SetValueAsync("trainings", Newtonsoft.Json.JsonConvert.SerializeObject(trainingList), TimeSpan.FromMinutes(5));

                return CreatedAtAction(nameof(GetTraining), new { id = training.Id }, training);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error creating training: {ex.Message}" });
            }
        }


        // API PUT: Update a training record (Admin role required)
        [Authorize(Roles = "Admin")]
        [HttpPut]
        [Route("api/training/{id}")]
        public async Task<IActionResult> UpdateTraining(int id, [FromBody] TrainingDto trainingDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var training = await _context.Trainings.FindAsync(id);
            if (training == null)
            {
                return NotFound(new { message = "Training not found." });
            }

            var user = await _context.Users.FindAsync(trainingDto.UserId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            training.Name = trainingDto.Name;
            training.UserId = trainingDto.UserId;
            training.User = user;

            try
            {
                _context.Trainings.Update(training);
                await _context.SaveChangesAsync();
                return Ok(training);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error updating training: {ex.Message}" });
            }
        }


        // API GET: Retrieve a single training record by ID
        [HttpGet("{id}")]

        public async Task<IActionResult> GetTraining(int id)
        {
            var trainingCacheKey = $"training:{id}";
            var training = await _redisCache.GetValueAsync(trainingCacheKey);

            if (string.IsNullOrEmpty(training)) // Nëse nuk ka të dhëna në cache
            {
                var trainingFromDb = await _context.Trainings
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (trainingFromDb == null)
                {
                    return NotFound(new { message = "Training not found." });
                }

                // Ruaj trajnimin në cache për 10 minuta
                _redisCache.SetValueAsync(trainingCacheKey, Newtonsoft.Json.JsonConvert.SerializeObject(trainingFromDb)).Wait();

                return Ok(trainingFromDb);
            }
            else
            {
                var trainingFromCache = Newtonsoft.Json.JsonConvert.DeserializeObject<Training>(training);
                return Ok(trainingFromCache);
            }
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
        [HttpGet]
        [Route("api/training/latest")]
        public async Task<IActionResult> GetLatestTrainings()
        {
            var latestTrainings = await _context.Trainings
                .Include(t => t.User) // Përfshi të dhënat e profesorit
                .OrderByDescending(t => t.Id) // Rendit sipas ID-së në mënyrë zbritëse
                .Take(3) // Merr vetëm 3 trajnimet e fundit
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    ProfessorName = t.User.UserName, // Emri i profesorit
                    t.FilePath
                })
                .ToListAsync();

            return Ok(latestTrainings);
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
        //[HttpGet("")]
        //public async Task<IActionResult> GetTrainings()
        //{
        //    var trainings = await _context.Trainings
        //        .Include(t => t.User)
        //        .ToListAsync();


        //    var trainingDtos = trainings.Select(t => new
        //    {
        //        t.Id,
        //        t.Name,
        //        t.FilePath,
        //        UserName = t.User?.UserName ?? "Not Assigned"
        //    });

        //    return Ok(trainingDtos);
        //}

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

            // Clear cache after deletion
            await _redisCache.SetValueAsync("trainings", null);

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

 

}
