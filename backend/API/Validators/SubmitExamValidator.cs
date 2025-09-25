using API.Helper;
using API.Models;

namespace API.Validators
{
    public static class SubmitExamValidator
    {
        public static string Validate(StudentExam? studentExam, DateTime now)
        {
            if (studentExam == null)
                return "You have not started this exam or it has already been submitted.";
            if (studentExam.Exam!.EndTime < now)
                return "Exam has ended, you cannot submit answers.";
            if (studentExam.Status != (int)StudentExamStatus.InProgress)
                return "You cannot save answers, your test is not in progress.";

            return "";
        }
    }
}
