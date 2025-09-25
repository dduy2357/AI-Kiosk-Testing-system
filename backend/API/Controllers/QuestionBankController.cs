using System.Runtime.CompilerServices;
using System.Security.Claims;
using API.Attributes;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using API.ViewModels.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AuthenPermission]
    public class QuestionBankController : Authentication.Authentication
    {
        private readonly IQuestionBankService _questionBankService;

        public QuestionBankController(IQuestionBankService questionBankService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _questionBankService = questionBankService;
        }

        [HttpGet("export/{questionBankId}")]
        public async Task<IActionResult> ExportQuestionBankReport(string questionBankId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (message, fileStream) = await _questionBankService.ExportQuestionBankReportAsync(questionBankId, UserToken.UserID);

            if (fileStream == null)
                return BadRequest(message);

            var fileName = $"QuestionBank_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
            fileStream.Position = 0;
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        //Share question bank
        [HttpPost("share")]
        public async Task<IActionResult> ShareQuestionBank([FromBody] ShareQuestionBankRequest request)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionBankService.ShareQuestionBankAsync(request.QuestionBankId, request.TargetUserEmail, UserToken.UserID, request.AccessMode);

            if (success)
                return Ok(new { message });

            return BadRequest(new { message });
        }
        //Change access mode
        [HttpPut("access-mode")]
        public async Task<IActionResult> ChangeAccessMode([FromBody] ChangeAccessModeRequest request)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionBankService.ChangeAccessModeAsync(request.QuestionBankId, request.TargetUserEmail, UserToken.UserID, request.NewAccessMode);

            if (success)
                return Ok(new { message });

            return BadRequest(new { message });
        }


        //Add
        [HttpPost("add")]
        public async Task<IActionResult> AddQuestionBank([FromBody] AddQuestionBankRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionBankService.AddQuestionBankAsync(request, UserToken.UserID);

            if (success)
            {
                return Ok(new { message });
            }

            return BadRequest(new { message });
        }

        //Edit
        [HttpPut("edit/{id}")]
        public async Task<IActionResult> EditQuestionBank(string id, [FromBody] EditQuestionBankRequest request)
        {
            if (request == null)
                return BadRequest(new { message = "Invalid request body, required field(s) are missing." });

            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionBankService.EditQuestionBankAsync(
                id, request.Title, request.SubjectId, request.Description, UserToken.UserID);

            if (success) return Ok(new { message });

            return BadRequest(new { message });
        }

        //Deactive/active
        [HttpPut("status/{id}")]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var userId = UserToken.UserID;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionBankService.ToggleQuestionBankStatusAsync(id, userId);

            if (success) return Ok(new { message });

            return BadRequest(new { message });
        }


        //View list
        [HttpGet("get-list")]
        public async Task<IActionResult> GetQuestionBankList([FromQuery] QuestionBankFilterVM input)
        {
            var (message, list) = await _questionBankService.GetList(input, UserToken.UserID);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Successfully retrieved question bank list.", data = list });
        }

        // Detail
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetQuestionBankDetail(string id)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { success = false, message = "User not authenticated." });

            var (success, message, data) = await _questionBankService.GetQuestionBankDetailAsync(id, UserToken.UserID);

            if (!success)
                return BadRequest(new { success = false, message });

            return Ok(new
            {
                success = true,
                message,
                data
            });
        }


        //delete
        [HttpDelete("{questionBankId}/subject/{subjectId}")]
        public async Task<IActionResult> DeleteQuestionBank(string questionBankId, string subjectId)
        {
            if (string.IsNullOrEmpty(UserToken.UserID))
                return Unauthorized(new { message = "User not authenticated." });

            var (success, message) = await _questionBankService.DeleteQuestionBankAsync(questionBankId, subjectId, UserToken.UserID);

            if (success)
                return Ok(new { message });

            return BadRequest(new { message });
        }



    }
}
