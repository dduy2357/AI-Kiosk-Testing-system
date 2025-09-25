using API.Models;
using Newtonsoft.Json;

namespace API.Builders
{
    public abstract class QuestionBuilder
    {
        protected Question question = new Question();
        public virtual void SetContent(string content) => question.Content = content.Trim();
        public virtual void SetType(int type) => question.Type = type;
        public virtual void SetDifficultLevel(int level) => question.DifficultLevel = level;
        public virtual void SetPoint(decimal point) => question.Point = point;
        public virtual void SetExplanation(string? explanation) => question.Explanation = explanation?.Trim() ?? "";
        public virtual void SetObjectFile(string? file) => question.ObjectFile = file;
        public virtual void SetTags(string? tags) { /* handle if needed */ }
        public virtual void SetDescription(string? desc) { /* handle if needed */ }
        public abstract Question GetResult();
    }
}
