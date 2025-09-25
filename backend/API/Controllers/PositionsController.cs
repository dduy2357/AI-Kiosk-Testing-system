using API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPositions(string departmentId)
        {
            var (message, positions) = await _positionService.GetPositionByDepartment(departmentId);
            if (string.IsNullOrEmpty(message))
            {
                return Ok(positions);
            }
            return NotFound(message);
        }
    }
}
