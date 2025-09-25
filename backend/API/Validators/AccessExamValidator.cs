using API.Helper;
using API.Models;

namespace API.Validators
{
    public class AccessExamValidator
    {
        public static string Validate(Exam exam, DateTime now, string userToken)
        {
            if (exam.StartTime > now) return ("The exam is not available at this time, exam has not started yet.");
            if (exam.EndTime < now) return ("The exam is not available at this time, exam has already ended.");

            if (!exam.Room?.RoomUsers.Any(ru => ru.UserId == userToken && ru.Status == (int)UserStatus.Active) ?? true)
                return ("You do not have permission to access this exam.");

            return (string.Empty);
        }
    }
}
