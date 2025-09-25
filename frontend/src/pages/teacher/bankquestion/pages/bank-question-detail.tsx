import { useEffect, useState } from 'react';
import { QuestionBankDetailHeader } from '../components/componentDetail/question-bank-detail-header';
import { QuestionBankDetailInfo } from '../components/componentDetail/question-bank-detail-info';
import { QuestionBankDetailStats } from '../components/componentDetail/question-bank-detail-stats';
import { QuestionsList } from '../components/componentDetail/questions-list';
import { useParams } from 'react-router-dom';
import useGetQuestionBankDetail from '@/services/modules/bankquestion/hooks/useGetQuestionBankDetail';
import { QuestionBankDetail } from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { useTranslation } from 'react-i18next';

export default function QuestionBankDetailPage() {
  const { t } = useTranslation('shared');
  const [questionBank, setQuestionBank] = useState<QuestionBankDetail | null>(null);
  const { questionBankId } = useParams<{ questionBankId: string }>();

  const {
    data: dataQuestionBankDetail,
    isLoading: isLoadingQuestions,
    refetch,
  } = useGetQuestionBankDetail(questionBankId ?? null, {
    isTrigger: !!questionBankId,
  });
  useEffect(() => {
    if (dataQuestionBankDetail && !isLoadingQuestions) {
      setQuestionBank(dataQuestionBankDetail);
    }
  }, [dataQuestionBankDetail, isLoadingQuestions]);

  if (isLoadingQuestions) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="mx-auto mb-4 h-12 w-12 animate-spin rounded-full border-b-2 border-blue-600"></div>
          <p className="text-gray-600">{t('BankQuestion.Loading')}</p>
        </div>
      </div>
    );
  }

  if (!questionBank) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50">
        <div className="text-center">
          <p className="text-gray-600">{t('BankQuestion.Nodata')}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {dataQuestionBankDetail && (
        <QuestionBankDetailHeader questionBank={questionBank} refetch={refetch} />
      )}

      <div className="space-y-6 pt-6">
        <div className="grid grid-cols-3 items-stretch gap-6">
          <div className="col-span-2">
            {dataQuestionBankDetail && <QuestionBankDetailInfo questionBank={questionBank} />}
          </div>
          <div>
            {dataQuestionBankDetail && <QuestionBankDetailStats questionBank={questionBank} />}
          </div>
        </div>

        <QuestionsList
          refetch={refetch}
          questionbankDetail={questionBank}
          questions={questionBank?.questions ?? []}
        />
      </div>
    </div>
  );
}
