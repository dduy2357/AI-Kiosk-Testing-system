using API.Strategy.Interface;
using API.Strategy;
using API.Helper;

namespace API.Factory
{
    public class ScoringStrategyFactory : IScoringStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ScoringStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IScoringStrategy GetStrategy(QuestionTypeChoose type)
        {
            return type switch
            {
                QuestionTypeChoose.MultipleChoice => _serviceProvider.GetRequiredService<MultichoiceScoringStrategy>(),
                QuestionTypeChoose.Essay => _serviceProvider.GetRequiredService<EssayScoringStrategy>(),
                _ => throw new NotImplementedException($"No strategy implemented for {type}")
            };
        }
    }

}
