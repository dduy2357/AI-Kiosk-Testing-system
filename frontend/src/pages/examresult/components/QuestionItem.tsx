import { Badge } from '@/components/ui/badge';
import { CheckCircle, XCircle } from 'lucide-react';
import QuestionOptions from './QuestionOptions';
import { Answer } from '@/services/modules/examresultdetail/interfaces/examresultdetail.interface';
import { useTranslation } from 'react-i18next';

interface QuestionItemProps {
  answer: Answer;
  questionIndex: number;
}

export default function QuestionItem({ answer, questionIndex }: Readonly<QuestionItemProps>) {
  const { t } = useTranslation('shared');
  return (
    <div className="rounded-lg border p-4 shadow-sm">
      <div className="flex items-start gap-3">
        <Badge variant={answer.isCorrect ? 'secondary' : 'destructive'} className="shrink-0">
          {t('StudentExamResultDetail.questions')} {questionIndex + 1}
        </Badge>
        {answer.isCorrect ? (
          <CheckCircle className="mt-0.5 h-5 w-5 shrink-0 text-green-500" />
        ) : (
          <XCircle className="mt-0.5 h-5 w-5 shrink-0 text-red-500" />
        )}
        <div className="flex-1">
          <p className="font-medium leading-relaxed">{answer.questionContent}</p>
        </div>
      </div>

      <QuestionOptions answer={answer} />
    </div>
  );
}
