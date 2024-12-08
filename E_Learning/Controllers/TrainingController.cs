using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Learning.Controllers
{
    [Authorize]
    public class TrainingController : Controller
    {
        public IActionResult GetAll()
        {
            return View();
        }
    }
}
