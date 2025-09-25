import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { BookOpen, User } from 'lucide-react';
import { QuestionBankDetail } from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { useTranslation } from 'react-i18next';

interface QuestionBankInfoProps {
  questionBank: QuestionBankDetail | null;
}

export function QuestionBankDetailInfo({ questionBank }: Readonly<QuestionBankInfoProps>) {
  const { t } = useTranslation('shared');

  return (
    <Card className="flex h-full flex-col">
      <CardHeader>
        <CardTitle className="flex items-center space-x-2">
          <BookOpen className="h-5 w-5" />
          <span>{t('BankQuestion.BankquestionDetailTitle')}</span>
        </CardTitle>
      </CardHeader>
      <CardContent className="flex flex-1 flex-col space-y-6">
        <div className="grid grid-cols-2 gap-6">
          <div>
            <label className="mb-1 block text-sm font-medium text-gray-500">
              {t('BankQuestion.NameBankQuestion')}
            </label>
            <p className="text-lg font-semibold text-gray-900">{questionBank?.questionBankName}</p>
          </div>
        </div>
        <div className="mt-auto pt-4">
          <div className="grid grid-cols-2 gap-6 text-sm">
            <div>
              <span className="mb-1 block text-gray-500">{t('BankQuestion.NameSubject')}</span>
              <Badge variant="outline" className="text-sm">
                {questionBank?.subjectName}
              </Badge>
            </div>
            <div>
              <span className="mb-1 block text-gray-500">{t('BankQuestion.Creator')}</span>
              <div className="flex items-center space-x-2">
                <User className="h-4 w-4 text-gray-400" />
                <span className="text-gray-900">{questionBank?.createBy}</span>
              </div>
            </div>
          </div>
        </div>

        <div>
          <label className="mb-1 block text-sm font-medium text-gray-500">
            {t('BankQuestion.Description')}
          </label>
          <p className="text-gray-900">
            {questionBank?.description ?? t('BankQuestion.NoDescription')}
          </p>
        </div>
      </CardContent>
    </Card>
  );
}
