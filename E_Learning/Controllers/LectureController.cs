using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E_Learning.Data;
using E_Learning.Models;
using System.Security.Claims;

namespace E_Learning.Controllers
{
    [Authorize(Roles = "Professor")]
    [Route("api/[controller]")]
    [ApiController]
    public class LectureController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LectureController(ApplicationDbContext context)
        {
            _context = context;
        }

        //// GET: api/Lecture
        //[HttpGet]
        //public async Task<IActionResult> GetLectures()
        //{
        //    var lectures = await _context.Lectures.Include(l => l.Training).ToListAsync();
        //    return Ok(lectures);
        //}

        //// GET: api/Lecture/{id}
        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetLecture(int id)
        //{
        //    var lecture = await _context.Lectures.Include(l => l.Training).FirstOrDefaultAsync(l => l.Id == id);

        //    if (lecture == null)
        //    {
        //        return NotFound(new { message = "Lecture not found." });
        //    }

        //    return Ok(lecture);
        //}

        //// POST: api/Lecture
        //[HttpPost]
        //public async Task<IActionResult> AddLecture([FromBody] Lecture lecture)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Verify Training exists
        //    var trainingExists = await _context.Trainings.AnyAsync(t => t.Id == lecture.TrainingId);
        //    if (!trainingExists)
        //    {
        //        return BadRequest(new { message = "Training not found." });
        //    }

        //    _context.Lectures.Add(lecture);
        //    await _context.SaveChangesAsync();

        //    return CreatedAtAction(nameof(GetLecture), new { id = lecture.Id }, lecture);
        //}

        //// PUT: api/Lecture/{id}
        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateLecture(int id, [FromBody] Lecture lecture)
        //{
        //    if (id != lecture.Id)
        //    {
        //        return BadRequest(new { message = "Lecture ID mismatch." });
        //    }

        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    // Verify Training exists
        //    var trainingExists = await _context.Trainings.AnyAsync(t => t.Id == lecture.TrainingId);
        //    if (!trainingExists)
        //    {
        //        return BadRequest(new { message = "Training not found." });
        //    }

        //    var existingLecture = await _context.Lectures.FindAsync(id);
        //    if (existingLecture == null)
        //    {
        //        return NotFound(new { message = "Lecture not found." });
        //    }

        //    existingLecture.FileName = lecture.FileName;
        //    existingLecture.FilePath = lecture.FilePath;
        //    existingLecture.TrainingId = lecture.TrainingId;

        //    _context.Entry(existingLecture).State = EntityState.Modified;
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        // DELETE: api/Lecture/{id}
        [HttpPost]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> DeleteLecture(int id)
        {
            var lecture = await _context.Lectures.FindAsync(id);

            if (lecture == null)
            {
                return NotFound(new { message = "Ligjërata nuk u gjet." });
            }

            // Sigurohuni që profesori është ai që ka krijuar trajnimin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var training = await _context.Trainings
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Id == lecture.TrainingId && t.User.Id == userId);

            if (training == null)
            {
                return Forbid(); // Nëse profesori nuk është pronar, ndalohet veprimi
            }

            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            // Ktheje përdoruesin në faqen e detajeve të trajnimit pas fshirjes
            return RedirectToAction("Details", "Training", new { id = lecture.TrainingId });
        }



    }
}
