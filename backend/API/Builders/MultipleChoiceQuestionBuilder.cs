using API.Models;
using Newtonsoft.Json;

namespace API.Builders
{
    public class MultipleChoiceQuestionBuilder : QuestionBuilder
{
    public void SetOptions(List<string> options)
    {
        question.Options = JsonConvert.SerializeObject(options);
    }

    public void SetCorrectAnswer(string answer)
    {
        question.CorrectAnswer = answer;
    }

    public override Question GetResult() => question;
}


}
