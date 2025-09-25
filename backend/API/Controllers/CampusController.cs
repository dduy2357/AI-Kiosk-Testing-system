using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CampusController : ControllerBase
    {
        private readonly ICampusService _campusService;

        public CampusController(ICampusService campusService)
        {
            _campusService = campusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCampuses()
        {
            var (message, campuses) = await _campusService.GetAllCampusesAsync();
            if (string.IsNullOrEmpty(message))
            {
                return Ok(campuses);
            }
            return NotFound(message);
        }
    }
}
