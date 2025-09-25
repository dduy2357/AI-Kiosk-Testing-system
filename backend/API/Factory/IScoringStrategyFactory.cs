using API.Helper;
using API.Strategy.Interface;

namespace API.Factory
{
    public interface IScoringStrategyFactory
    {
        IScoringStrategy GetStrategy(QuestionTypeChoose examType);
    }
}
