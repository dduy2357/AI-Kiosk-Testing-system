
namespace API.Hubs
{
    public class ExamHub : HubBase
    {
        public static readonly string RECEIVE_EXTRA_TIME = "ReceiveExtraTime";
        public static readonly string RECEIVE_EXAM_EXTRA_TIME = "ReceiveExamExtraTime";
        public static readonly string FINISH_EXAM = "FinishExam";
        public static readonly string FINISH_STUDENT_EXAM = "FinishStudentExam";
        public static readonly string RE_ASSIGN_EXAM = "ReAssignExam";
        public static readonly string ADD_NEW_FACECAPTURE = "AddNewFaceCapture";


        public async Task JoinExamGroup(string examId, string studentExamId)
        {
            if (!string.IsNullOrWhiteSpace(examId))
                await Groups.AddToGroupAsync(Context.ConnectionId, examId);
            if (!string.IsNullOrWhiteSpace(studentExamId))
                await Groups.AddToGroupAsync(Context.ConnectionId, studentExamId);
        }

        public async Task LeaveExamGroup(string examId, string studentExamId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, examId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, studentExamId);
        }
    }
}
