import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import httpService from '@/services/httpService';
import useGetListBankQuestion from '@/services/modules/bankquestion/hooks/useGetAllBankQuestion';
import { IBankQuestionRequest } from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface QuestionBankSelectorProps {
  selectedBank: string;
  onBankChange: (bank: string) => void;
  isBankDisabled: boolean;
  setSelectedSubjectName: (name: string) => void;
}

export function QuestionBankSelector({
  selectedBank,
  onBankChange,
  isBankDisabled,
  setSelectedSubjectName,
}: Readonly<QuestionBankSelectorProps>) {
  const userId = httpService.getUserStorage()?.roleId[0];
  const { t } = useTranslation('shared');
  const [filtersBankquestion] = useState<IBankQuestionRequest>({
    pageSize: 100,
    currentPage: 1,
    status: 1,
    IsMyQuestion: userId === 2 ? true : undefined,
  });
  const { data } = useGetListBankQuestion(filtersBankquestion);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-medium">{t('AddQuestion.SelectBankQuestion')}</CardTitle>
        <p className="text-sm text-gray-500">{t('AddQuestion.SelectBankQuestionDescription')}</p>
      </CardHeader>
      <CardContent>
        <Select
          value={selectedBank}
          onValueChange={(value) => {
            onBankChange(value);
            const bank = data?.find((b) => b.questionBankId === value);
            setSelectedSubjectName(bank?.subjectName ?? '');
          }}
          disabled={isBankDisabled}
        >
          <SelectTrigger className="w-full">
            <SelectValue placeholder={t('AddQuestion.SelectBankQuestion')} />
          </SelectTrigger>
          <SelectContent>
            {data?.map((bank) => (
              <SelectItem key={bank?.questionBankId} value={bank?.questionBankId}>
                {bank?.title} ({bank?.totalQuestions} {t('AddQuestion.Questions')})
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        {!selectedBank && (
          <p className="mt-2 text-sm text-red-500">{t('AddQuestion.RequireSelectQSBank')}</p>
        )}
      </CardContent>
    </Card>
  );
}
