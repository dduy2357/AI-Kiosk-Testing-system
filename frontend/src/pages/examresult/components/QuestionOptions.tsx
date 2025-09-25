import { Answer } from '@/services/modules/examresultdetail/interfaces/examresultdetail.interface';
import { findOptionIndex, getOptionLabel, parseOptions } from '@/utils/exam.utils';
import AnswerBadge from './AnswerBadge';
import { useTranslation } from 'react-i18next';

interface QuestionOptionsProps {
  answer: Answer;
}

export default function QuestionOptions({ answer }: Readonly<QuestionOptionsProps>) {
  if (!answer.options) {
    return <SimpleAnswerDisplay answer={answer} />;
  }

  const options = parseOptions(answer.options);
  if (options.length === 0) {
    return <SimpleAnswerDisplay answer={answer} />;
  }

  const userAnswerIndex = findOptionIndex(options, answer.userAnswer);
  const correctAnswerIndex = findOptionIndex(options, answer.correctAnswer);
  const isCorrect = answer.isCorrect;

  return (
    <div className="mt-4 space-y-3">
      <div className="grid grid-cols-1 gap-2 md:grid-cols-2">
        {options.map((option, index) => {
          const isUserChoice = index === userAnswerIndex;
          const isCorrectChoice = index === correctAnswerIndex;

          let bgColor = 'bg-gray-50';
          let borderColor = 'border-gray-200';
          let textColor = 'text-gray-900';

          if (isCorrectChoice) {
            bgColor = 'bg-green-50';
            borderColor = 'border-green-200';
            textColor = 'text-green-900';
          } else if (isUserChoice && !isCorrect) {
            bgColor = 'bg-red-50';
            borderColor = 'border-red-200';
            textColor = 'text-red-900';
          }

          return (
            <div key={index} className={`rounded border p-3 ${bgColor} ${borderColor}`}>
              <div className="flex items-center justify-between">
                <div className={`${textColor}`}>
                  <span className="font-medium">{getOptionLabel(index)}.</span> {option}
                </div>
                <div className="flex gap-1">
                  {isCorrectChoice && !isUserChoice && <AnswerBadge type="correct" size="sm" />}
                  {isUserChoice && !isCorrectChoice && (
                    <AnswerBadge type="user-incorrect" size="sm" />
                  )}
                  {isUserChoice && isCorrectChoice && <AnswerBadge type="user-correct" size="sm" />}
                </div>
              </div>
            </div>
          );
        })}
      </div>

      <QuestionMetadata answer={answer} />
    </div>
  );
}

function SimpleAnswerDisplay({ answer }: Readonly<{ answer: Answer }>) {
  const { t } = useTranslation('shared');
  const isCorrect = answer.isCorrect;
  const userAnswer = answer.userAnswer || t('StudentExamResultDetail.not-answer');
  const correctAnswer = answer.correctAnswer;
  return (
    <div className="mt-4 space-y-3">
      {/* User's Answer */}
      <div
        className={`rounded-lg border p-3 ${isCorrect ? 'border-green-200 bg-green-50' : 'border-red-200 bg-red-50'}`}
      >
        <div className="flex items-center justify-between">
          <div>
            <span className="text-sm font-medium text-gray-600">
              {t('StudentExamResultDetail.your-answer')}
            </span>
            <p className="mt-1 font-semibold">{userAnswer}</p>
          </div>
          <div className="flex items-center gap-2">
            <AnswerBadge type={isCorrect ? 'user-correct' : 'user-incorrect'} />
          </div>
        </div>
      </div>

      {/* Correct Answer (only show if user was wrong) */}
      {!isCorrect && (
        <div className="rounded-lg border border-green-200 bg-green-50 p-3">
          <span className="text-sm font-medium text-gray-600">
            {t('StudentExamResultDetail.correct-answer')}
          </span>
          <p className="mt-1 font-semibold text-green-800">{correctAnswer}</p>
        </div>
      )}

      <QuestionMetadata answer={answer} />
    </div>
  );
}

function QuestionMetadata({ answer }: Readonly<{ answer: Answer }>) {
  const { t } = useTranslation('shared');
  return (
    <div className="flex items-center justify-between text-sm text-gray-500">
      <span>
        {t('StudentExamResultDetail.Result')}{' '}
        <strong className={answer.isCorrect ? 'text-green-600' : 'text-red-600'}>
          {answer.pointsEarned}
        </strong>
      </span>
      {answer.timeSpent > 0 && (
        <span>
          Thời gian: <strong>{answer.timeSpent}s</strong>
        </span>
      )}
    </div>
  );
}
