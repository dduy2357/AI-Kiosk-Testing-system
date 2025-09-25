using API.Models;
using Newtonsoft.Json;

namespace API.Builders
{
    public class EssayQuestionBuilder : QuestionBuilder
    {
        public void SetGuideAnswer(string guide)
        {
            question.Options = JsonConvert.SerializeObject(new List<string>());
            question.CorrectAnswer = guide;
        }

        public override Question GetResult() => question;
    }

}
