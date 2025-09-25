using API.Helper;
using API.Models;

namespace API.Validators
{
    public static class AnswerValidator
    {
        public static string Validate(StudentExam studentExam, DateTime now)
        {
            if (studentExam.Exam!.EndTime < now) return "Exam has ended, you cannot save answers.";
            if (studentExam.Status != (int)StudentExamStatus.InProgress)
                return "You cannot save answers, your test is not in progress.";

            return "";
        }
    }
}
