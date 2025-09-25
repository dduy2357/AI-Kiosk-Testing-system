using API.Models;

namespace API.Repository.Interface
{
    public interface IFaceCaptureRepository
    {
        Task<List<FaceCapture>> GetByStudentExamId(string studentExamId);
        void DeleteRange(List<FaceCapture> faceCaptures);
    }
}
