using API.Attributes;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthenPermission]
    public class QuestionsController : Authentication.Authentication
    {
        private readonly IQuestionService _questionService;

        public QuestionsController(IQuestionService questionService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _questionService = questionService;
        }

        [HttpPost("format-file-questions")]
        public async Task<IActionResult> ImportFile([FromForm] FormatQuestionList file)
        {
            var (message, data) = await _questionService.FormatListQuestion(file);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Format data successfully", data });
        }

        [HttpPost("importQuestions")]
        public async Task<IActionResult> ImportQuestions([FromBody] List<AddQuestionRequest> questions)
        {
            if (questions == null || questions.Count == 0)
                return BadRequest(new { message = "No questions provided for import." });

            var userId = UserToken.UserID;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionService.ImportListQuestionAsync(questions, userId);

            if (success)
                return Ok(new { message });

            return BadRequest(new { message });
        }



        [HttpPost("add")]
        public async Task<IActionResult> AddQuestion([FromForm] AddQuestionRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });
            //var (success, message) = await _questionService.AddQuestionAsync(request, UserToken.UserID);
            var (success, message) = await _questionService.AddQuestionBPAsync(request, UserToken.UserID);
            if (success)
            {
                return Ok(new { message });
            }

            return BadRequest(new { message });
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditQuestion([FromBody] EditQuestionRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionService.EditQuestionAsync(request, UserToken.UserID);

            if (success)
            {
                return Ok(new { message });
            }

            return BadRequest(new { message });
        }

        [HttpPut("toggle")]
        public async Task<IActionResult> ToggleQuestionStatus([FromBody] ToggleQuestionStatusRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionService.ToggleStatusAsync(request, UserToken.UserID);

            if (success)
            {
                return Ok(new { message });
            }

            return BadRequest(new { message });
        }

        [HttpDelete("{questionBankId}/question/{questionId}")]
        public async Task<IActionResult> DeleteQuestion(string questionBankId, string questionId)
        {
            if (string.IsNullOrEmpty(questionBankId) || string.IsNullOrEmpty(questionId))
                return BadRequest(new { message = "Invalid request body, required field(s) questionBankId or questionId are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionService.DeleteQuestionAsync(questionBankId, questionId, UserToken.UserID);

            if (success)
                return Ok(new { message });

            return BadRequest(new { message });
        }


        [HttpGet("get-list")]
        public async Task<IActionResult> GetListQuestion([FromQuery] QuestionRequestVM request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message, result) = await _questionService.GetListQuestionAsync(request, UserToken.UserID);

            if (success)
                return Ok(new
                {
                    success = true,
                    message,
                    data = result
                });

            return BadRequest(new
            {
                success = false,
                message
            });
        }



    }

}
