import { Card, CardContent } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress';
import { getScoreColor } from '@/utils/exam.utils';
import { Clock } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface ScoreStatisticsProps {
  score: number;
  totalCorrectAnswers: number;
  totalWrongAnswers: number;
  totalQuestions: number;
  durationSpent: number;
}

export default function ScoreStatistics({
  score,
  totalCorrectAnswers,
  totalWrongAnswers,
  totalQuestions,
  durationSpent,
}: Readonly<ScoreStatisticsProps>) {
  const { t } = useTranslation('shared');
  const scorePercentage = (score / 10) * 100;

  return (
    <Card className="mt-6">
      <CardContent className="p-6">
        <div className="grid grid-cols-1 items-center gap-6 md:grid-cols-4">
          {/* Score */}
          <div className="text-center">
            <div className={`text-4xl font-bold ${getScoreColor(score)}`}>{score.toFixed(1)}</div>
            <p className="mt-1 text-sm text-gray-500">{t('StudentExamResultDetail.score')}</p>
            <Progress value={scorePercentage} className="mt-2 h-2" />
          </div>

          {/* Correct Answers */}
          <div className="text-center">
            <div className="text-3xl font-bold text-green-600">{totalCorrectAnswers}</div>
            <p className="mt-1 text-sm text-gray-500">
              {t('StudentExamResultDetail.correctAnswers')}
            </p>
            <div className="mt-2 h-2 w-full rounded-full bg-gray-200">
              <div
                className="h-2 rounded-full bg-green-600"
                style={{
                  width: `${(totalCorrectAnswers / totalQuestions) * 100}%`,
                }}
              ></div>
            </div>
          </div>

          {/* Wrong Answers */}
          <div className="text-center">
            <div className="text-3xl font-bold text-red-600">{totalWrongAnswers}</div>
            <p className="mt-1 text-sm text-gray-500">
              {t('StudentExamResultDetail.wrongAnswers')}
            </p>
            <div className="mt-2 h-2 w-full rounded-full bg-gray-200">
              <div
                className="h-2 rounded-full bg-red-600"
                style={{
                  width: `${(totalWrongAnswers / totalQuestions) * 100}%`,
                }}
              ></div>
            </div>
          </div>

          {/* Duration */}
          <div className="text-center">
            <div className="text-3xl font-bold text-blue-600">{durationSpent}</div>
            <p className="mt-1 text-sm text-gray-500">{t('StudentExamResultDetail.minutes')}</p>
            <div className="mt-2 flex items-center justify-center gap-1 text-xs text-gray-500">
              <Clock className="h-3 w-3" />
              <span>{t('StudentExamResultDetail.durationSpent')}</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
