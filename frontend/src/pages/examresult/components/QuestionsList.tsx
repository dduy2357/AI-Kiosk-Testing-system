import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Answer } from '@/services/modules/examresultdetail/interfaces/examresultdetail.interface';
import { FileText } from 'lucide-react';
import QuestionItem from './QuestionItem';
import { useTranslation } from 'react-i18next';

interface QuestionsListProps {
  answers: Answer[];
}

export default function QuestionsList({ answers }: Readonly<QuestionsListProps>) {
  const { t } = useTranslation('shared');
  return (
    <Card className="mt-6">
      <CardHeader>
        <CardTitle>
          {t('StudentExamResultDetail.questionsDetail')} ({answers.length}{' '}
          {t('StudentExamResultDetail.questions')})
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-6">
        {answers.length > 0 ? (
          answers.map((answer, index) => (
            <QuestionItem key={answer.questionId} answer={answer} questionIndex={index} />
          ))
        ) : (
          <div className="py-8 text-center text-gray-500">
            <FileText className="mx-auto h-12 w-12 text-gray-300" />
            <p className="mt-2">{t('StudentExamResultDetail.noQuestions')}</p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
