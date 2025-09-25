using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecializationsController : ControllerBase
    {
        private readonly ISpecializationService _specializationService;

        public SpecializationsController(ISpecializationService specializationService)
        {
            _specializationService = specializationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSpecializations()
        {
            var (message, specializations) = await _specializationService.GetAllSpecializationsAsync();
            if (string.IsNullOrEmpty(message))
            {
                return Ok(specializations);
            }
            return NotFound(message);
        }
    }
}
