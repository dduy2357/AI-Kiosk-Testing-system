using API.ViewModels;

namespace API.Services.Interfaces
{
    public interface IFaceCaptureService
    {
        public Task<(string, List<AIResponseFaceCaptureVM>?)> AnalyzeFaceCapture(string studentExamId);
        public Task<(string, SearchResult?)> GetList(FaceCaptureSearchVM input);
        public Task<(string, FaceCaptureVM.CaptureImages?)> GetOne(string captureId);
        public Task<string> AddCapture(FaceCaptureRequest input);
        public Task<string> Delete(string captureId);
        public Task<string> DeleteByStudentExamId(string studentExamId);
        public Task<(string, MemoryStream?)> DownloadAllCapturesAsZip(string studentExamId);
    }
}
