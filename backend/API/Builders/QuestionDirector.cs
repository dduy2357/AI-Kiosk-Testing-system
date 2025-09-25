using API.ViewModels;

namespace API.Builders
{
    public class QuestionDirector
    {
        public void Construct(QuestionBuilder builder, AddQuestionRequest request)
        {
            builder.SetContent(request.Content);
            builder.SetType(request.Type);
            builder.SetDifficultLevel(request.DifficultLevel);
            builder.SetPoint(1);
            builder.SetExplanation(request.Explanation);
            builder.SetObjectFile(request.ObjectFile);
            builder.SetTags(request.Tags);
            builder.SetDescription(request.Description);

            if (request.Type == 0)
            {
                var essayBuilder = builder as EssayQuestionBuilder;
                if (essayBuilder != null)
                {
                    essayBuilder.SetGuideAnswer(request.CorrectAnswer);
                }
            }
            else if (request.Type == 1 && builder is MultipleChoiceQuestionBuilder mcBuilder)
            {
                mcBuilder.SetOptions(request.Options);
                mcBuilder.SetCorrectAnswer(request.CorrectAnswer);
            }
        }
    }

}
