import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { QUESTION_BANK_URL } from '@/consts/apiUrl';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import httpService from '@/services/httpService';
import {
  IQuestionBankForm,
  QuestionBankDetail,
} from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import useGetAllSubjectV2 from '@/services/modules/subject/hooks/useGetAllSubjectV2';
import { ISubjectRequest } from '@/services/modules/subject/interfaces/subject.interface';
import axios from 'axios';
import { Download, Edit } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useParams } from 'react-router-dom';
import AddQuestionBankModal from '../add-question-bank-modal';
import { useTranslation } from 'react-i18next';

interface QuestionBankHeaderProps {
  questionBank: QuestionBankDetail | null;
  refetch: () => void;
}

export function QuestionBankDetailHeader({
  questionBank,
  refetch,
}: Readonly<QuestionBankHeaderProps>) {
  const { t } = useTranslation('shared');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const token = httpService.getTokenStorage();
  const { questionBankId } = useParams<{ questionBankId: string }>();
  const { filters } = useFiltersHandler({
    pageSize: 10000,
    currentPage: 1,
    textSearch: '',
  });

  const stableFilters = useMemo(() => filters as ISubjectRequest, [filters]);
  const { data: subjects } = useGetAllSubjectV2(stableFilters, {});
  const getStatusBadge = (status: number) => {
    switch (status) {
      case 1:
        return (
          <Badge className="border-green-200 bg-green-100 text-green-800">
            {t('BankQuestion.Active')}
          </Badge>
        );
      case 0:
        return (
          <Badge className="border-gray-200 bg-gray-100 text-gray-800">
            {t('BankQuestion.NotActive')}
          </Badge>
        );
      default:
        return <Badge variant="outline">{t('BankQuestion.Unknown')}</Badge>;
    }
  };

  const handleEditQuestionBank = (data: IQuestionBankForm, questionBankId?: string) => {
    try {
      httpService.put(`${QUESTION_BANK_URL}/edit/${questionBankId}`, data);
      showSuccess(t('BankQuestion.EditSuccess'));
    } catch (error) {
      showError(t('BankQuestion.EditError'));
      console.error('Error submitting form:', error);
    }
    setTimeout(() => refetch(), 1000);
  };

  const getExportFile = async () => {
    try {
      const response = await axios.get(`${QUESTION_BANK_URL}/export/${questionBankId}`, {
        responseType: 'blob',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      const url = window.URL.createObjectURL(
        new Blob([response.data], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }),
      );
      const link = document.createElement('a');
      link.href = url;
      const contentDisposition = response.headers['content-disposition'];
      let fileName = `${questionBank?.questionBankName}.xlsx`;
      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename="(.+)"/);
        if (fileNameMatch && fileNameMatch[1]) {
          fileName = fileNameMatch[1];
        }
      }

      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
    } catch (error) {
      console.error('Error exporting file:', error);
      showError(t('BankQuestion.ExportError'));
    }
  };
  return (
    <div className="border-b border-gray-200 bg-white p-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">{questionBank?.questionBankName}</h1>
            <p className="mt-1 text-gray-600">
              {t('BankQuestion.BankQuestion')} • {questionBank?.subjectName}
            </p>
          </div>
          {getStatusBadge(questionBank?.status ?? -1)}
        </div>
        <div className="flex items-center space-x-3">
          <Button variant="outline" onClick={() => setIsModalOpen(true)}>
            <Edit className="mr-2 h-4 w-4" />
            {t('BankQuestion.Edit')}
          </Button>
          <Button variant="outline" onClick={() => getExportFile()}>
            <Download className="mr-2 h-4 w-4" />
            {t('BankQuestion.Export')}
          </Button>
        </div>
      </div>
      <AddQuestionBankModal
        id={questionBankId}
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleEditQuestionBank}
        dataSubjects={subjects}
      />
    </div>
  );
}
