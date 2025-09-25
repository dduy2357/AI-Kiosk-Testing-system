import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Copy, Eye, Plus } from 'lucide-react';
import useGetListQuestion from '@/services/modules/question/hooks/useGetAllQuestion';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import { useTranslation } from 'react-i18next';
import httpService from '@/services/httpService';

interface SavedQuestionsListProps {
  onCreateNew: () => void;
}

export function SavedQuestionsList({ onCreateNew }: Readonly<SavedQuestionsListProps>) {
  // const [questions, setQuestions] = useState<QuestionList[]>([])
  const userId = httpService.getUserStorage()?.roleId[0];
  const { t } = useTranslation('shared');
  const getTypeLabel = (type: number) => {
    switch (type) {
      case 1:
        return t('BankQuestion.MultipleChoice');
      case 0:
        return t('BankQuestion.Essay');
      default:
        return type;
    }
  };

  const getDifficultyLabel = (difficulty: number) => {
    switch (difficulty) {
      case 1:
        return t('AddQuestion.Easy');
      case 2:
        return t('AddQuestion.Medium');
      case 3:
        return t('AddQuestion.Hard');
      case 4:
        return t('AddQuestion.VeryHard');
      default:
        return difficulty;
    }
  };

  const getDifficultyColor = (difficulty: number) => {
    switch (difficulty) {
      case 1:
        return 'bg-green-100 text-green-800';
      case 2:
        return 'bg-yellow-100 text-yellow-800';
      case 3:
        return 'bg-red-100 text-orange-800';
      case 4:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  const { filters } = useFiltersHandler({
    pageSize: 5,
    currentPage: 1,
    textSearch: '',
    IsMyQuestion: userId === 2 ? true : undefined,
  });

  const { data: questions } = useGetListQuestion(filters);
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle className="text-xl font-semibold">
              {t('AddQuestion.QuestionSaved')} ({questions.length})
            </CardTitle>
            <p className="mt-1 text-sm text-gray-500">
              {t('AddQuestion.RecentlyCreatedQuestions')}
            </p>
          </div>
          <Button onClick={onCreateNew} className="bg-blue-600 hover:bg-blue-700">
            <Plus className="mr-2 h-4 w-4" />
            {t('AddQuestion.CreateNewQuestion')}
          </Button>
        </div>
      </CardHeader>
      <CardContent>
        {questions.length === 0 ? (
          <div className="py-12 text-center">
            <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
              <Copy className="h-8 w-8 text-gray-400" />
            </div>
            <h3 className="mb-2 text-lg font-medium text-gray-900">
              {t('AddQuestion.NotFoundSavedQuestions')}
            </h3>
            <p className="mb-4 text-gray-500">{t('AddQuestion.StartCreatingYourFirstQuestion')}</p>
            <Button onClick={onCreateNew}>
              <Plus className="mr-2 h-4 w-4" />
              {t('AddQuestion.CreateNewQuestion')}
            </Button>
          </div>
        ) : (
          <div className="space-y-4">
            {questions.map((question) => (
              <div
                key={question?.questionId}
                className="rounded-lg border border-gray-200 p-4 transition-colors hover:bg-gray-50"
              >
                <div className="flex items-start justify-between">
                  <div className="flex-1">
                    <div className="mb-3 flex items-center space-x-2">
                      <Badge className={getDifficultyColor(question?.difficultLevel)}>
                        {getDifficultyLabel(question?.difficultLevel)}
                      </Badge>
                      <Badge variant="outline">{getTypeLabel(question?.type)}</Badge>
                      <Badge variant="outline">
                        {question?.point} ${t('BankQuestion.Point')}
                      </Badge>
                    </div>
                    <h3 className="mb-2 text-lg font-medium text-gray-900">
                      {question?.content || t('AddQuestion.Content')}
                    </h3>
                    <div className="space-y-1 text-sm text-gray-500">
                      <p>
                        {t('BankQuestion.BankQuestion')}: {question?.questionBankName}
                      </p>
                    </div>
                  </div>
                  <div className="ml-4 flex items-center space-x-2">
                    <Button variant="ghost" size="sm">
                      <Eye className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
