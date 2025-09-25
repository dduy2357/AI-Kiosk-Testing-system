using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class FeedbackController : Authentication.Authentication
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FeedbackController(IFeedbackService feedbackService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetListFeedbacks([FromQuery] FeedbackSearchVM search)
        {
            var (message, data) = await _feedbackService.GetList(search);
            if (!string.IsNullOrEmpty(message))
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get list feedbacks successfully.", data });
        }

        [HttpGet("get-one/{id}")]
        public async Task<IActionResult> GetOneFeedback(string id)
        {
            var (message, data) = await _feedbackService.GetOne(id);
            if (!string.IsNullOrEmpty(message))
            {
                return BadRequest(new { success = false, message, data = new List<object>() });
            }
            return Ok(new { success = true, message = "Get feedback successfully.", data });
        }

        [HttpPost("create-update")]
        public async Task<IActionResult> CreateUpdateFeedback([FromBody] CreateUpdateFeedbackVM model)
        {
            var message = await _feedbackService.CreateUpdate(model, UserToken.UserID!);
            if (!string.IsNullOrEmpty(message))
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Feedback created/updated successfully." });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFeedback(string id)
        {
            var message = await _feedbackService.Delete(id);
            if (!string.IsNullOrEmpty(message))
            {
                return BadRequest(new { success = false, message });
            }
            return Ok(new { success = true, message = "Feedback deleted successfully." });
        }
    }
}
