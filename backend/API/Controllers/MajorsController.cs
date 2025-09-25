using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MajorsController : ControllerBase
    {
        private readonly IMajorService _majorService;

        public MajorsController(IMajorService majorService)
        {
            _majorService = majorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMajors()
        {
            var (message, majors) = await _majorService.GetAllMajorsAsync();
            if (string.IsNullOrEmpty(message))
            {
                return Ok(majors);
            }
            return NotFound(message);
        }
    }
}
