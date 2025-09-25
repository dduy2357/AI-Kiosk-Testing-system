using API.Attributes;
using API.Services.Interfaces;
using API.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AuthenPermission]
    public class FaceCaptureController : Authentication.Authentication
    {
        private readonly IFaceCaptureService _faceCaptureService;

        public FaceCaptureController(IFaceCaptureService faceCaptureService, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _faceCaptureService = faceCaptureService;
        }

        [HttpPost("analyze-face-capture/{studentExamId}")]
        public async Task<IActionResult> AnalyzeFaceCapture(string studentExamId)
        {
            var (message, result) = await _faceCaptureService.AnalyzeFaceCapture(studentExamId);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Analyze face capture successfully.", data = result });
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList([FromQuery] FaceCaptureSearchVM input)
        {
            var (message, result) = await _faceCaptureService.GetList(input);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Get captures list successfully.", data = result });
        }

        [HttpGet("get-by-id/{captureId}")]
        public async Task<IActionResult> GetOne(string captureId)
        {
            var (message, result) = await _faceCaptureService.GetOne(captureId);
            if (message.Length > 0) return BadRequest(new { success = false, message, data = new List<object>() });
            return Ok(new { success = true, message = "Get capture successfully.", data = result });
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddCapture([FromForm] FaceCaptureRequest input)
        {
            var message = await _faceCaptureService.AddCapture(input);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Add capture successfully." });
        }

        [HttpDelete("delete/{captureId}")]
        public async Task<IActionResult> Delete(string captureId)
        {
            var message = await _faceCaptureService.Delete(captureId);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Delete capture successfully." });
        }

        [HttpDelete("delete-all-by-student-exam/{studentExamId}")]
        public async Task<IActionResult> DeleteByStudentExamId(string studentExamId)
        {
            var message = await _faceCaptureService.DeleteByStudentExamId(studentExamId);
            if (message.Length > 0) return BadRequest(new { success = false, message });
            return Ok(new { success = true, message = "Delete all captures for student exam successfully." });
        }

        [HttpGet("download-all-captures/{studentExamId}")]
        public async Task<IActionResult> DownloadAllCapturesAsZip(string studentExamId)
        {
            var (message, stream) = await _faceCaptureService.DownloadAllCapturesAsZip(studentExamId);
            if (message.Length > 0) return BadRequest(new { success = false, message });

            var fileName = $"{studentExamId}_captures.zip";
            return File(stream, "application/zip", fileName);
        }
    }
}
