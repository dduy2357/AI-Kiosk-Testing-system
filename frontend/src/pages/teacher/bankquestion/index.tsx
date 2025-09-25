import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { Button } from '@/components/ui/button';
import BaseUrl from '@/consts/baseUrl';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import { GenericFilters } from '@/pages/admin/manageuser/components/generic-filters';
import httpService from '@/services/httpService';
import bankquestionService from '@/services/modules/bankquestion/bankquestion.Service';
import useGetListBankQuestion from '@/services/modules/bankquestion/hooks/useGetAllBankQuestion';
import {
  IBankQuestionRequest,
  IQuestionBankForm,
} from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { IQuestionForm } from '@/services/modules/question/interfaces/question.interface';
import questionService from '@/services/modules/question/question.service';
import useGetAllSubjectV2 from '@/services/modules/subject/hooks/useGetAllSubjectV2';
import { ISubjectRequest } from '@/services/modules/subject/interfaces/subject.interface';
import { Landmark } from 'lucide-react';
import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import ExamHeader from '../examsupervision/components/ExamHeader';
import ImportWordModal from '../managequestion/components/import-word-modal';
import AddQuestionBankModal from './components/add-question-bank-modal';
import BankQuestionCard from './components/bank-question-card';
import QuestionBankStats from './components/question-bank-stats';

const BankQuestion = () => {
  const user = httpService.getUserStorage();
  const { t } = useTranslation('shared');
  const [isModalOpen, setIsModalOpen] = useState(false);
  const token = httpService.getTokenStorage();
  const [isImportModalOpen, setIsImportModalOpen] = useState(false);
  const roleId = httpService.getUserStorage()?.roleId.at(0);
  const navigate = useNavigate();

  //! State
  const [filtersBankquestion, setFiltersBankquestion] = useState<IBankQuestionRequest>({
    pageSize: 6,
    currentPage: 1,
    status: 1,
    textSearch: '',
    IsMyQuestion: user?.roleId.at(0) === 4 ? false : true,
    filterSubject: '',
  });

  const [filtersForStat] = useState<IBankQuestionRequest>({
    pageSize: 2,
    currentPage: 1,
    status: 1,
    textSearch: '',
    IsMyQuestion: user?.roleId.at(0) === 4 ? false : true,
    filterSubject: '',
  });
  const { totalQuestionBanks, totalQuestionsQB } = useGetListBankQuestion(filtersForStat);
  const { filters } = useFiltersHandler({
    pageSize: 10000,
    currentPage: 1,
    textSearch: '',
  });

  const stableFilters = useMemo(() => filters as ISubjectRequest, [filters]);
  const { data: subjects } = useGetAllSubjectV2(stableFilters, {});
  const { data, refetch, totalPage, loading } = useGetListBankQuestion(filtersBankquestion);

  // Stats data
  const stats = {
    questionBanks: totalQuestionBanks,
    totalQuestions: totalQuestionsQB,
    subjects: subjects ? subjects.length : 0,
  };

  // function
  const handleAddQuestionBank = async (values: IQuestionBankForm) => {
    try {
      httpService.attachTokenToHeader(token);
      await bankquestionService.addQuestionBank(values);
      showSuccess(t('BankQuestion.AddSuccess'));
      refetch();
    } catch (error) {
      showError(t('BankQuestion.AddError'));
    }
  };

  // Phân trang
  const currentPage = filtersBankquestion.currentPage;

  // Xử lý chuyển trang
  const handlePageChange = (page: number) => {
    if (page > 0 && page <= totalPage) {
      setFiltersBankquestion((prev) => ({
        ...prev,
        currentPage: page,
      }));
    }
  };

  const handleImportComplete = async (questions: IQuestionForm[]) => {
    try {
      await questionService.importQuestion(questions);
    } catch (error) {
      showError(error);
    }
    refetch();

    setIsImportModalOpen(false);
    showSuccess(`${t('ManageQuestion.importSuccessMessage')}`);
  };

  //! Render
  return (
    <div className="min-h-screen bg-gray-50">
      <PageWrapper
        name={t('BankQuestion.BankQuestion')}
        className="bg-gradient-to-br from-slate-50 to-blue-50/30"
        isLoading={loading}
      >
        <div className="space-y-6">
          <ExamHeader
            title={t('BankQuestion.Title')}
            subtitle={t('BankQuestion.Subtitle')}
            icon={<Landmark className="h-8 w-8 text-white" />}
            className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
          />
          <div className="space-y-8 p-6">
            <QuestionBankStats stats={stats} />
            <GenericFilters
              className="lg:grid-cols- md:grid-cols-3"
              searchPlaceholder={t('BankQuestion.Search')}
              onSearch={(e) => {
                setFiltersBankquestion((prev) => ({
                  ...prev,
                  textSearch: e.textSearch ?? '',
                }));
              }}
              initialSearchQuery={filters.textSearch}
              filters={[
                {
                  key: 'filterSubject',
                  placeholder: t('BankQuestion.AllSubjects'),
                  options: [
                    { label: t('BankQuestion.AllSubjects'), value: ' ' },
                    ...(subjects?.map((subject) => ({
                      label: subject.subjectName,
                      value: subject.subjectName,
                    })) ?? []),
                  ],
                },
              ]}
              onFilterChange={(
                newFilters: Record<string, string | number | boolean | null | undefined>,
              ) => {
                setFiltersBankquestion((prev) => {
                  const updatedFilters = {
                    ...prev,
                    ...newFilters,
                    currentPage: 1,
                  };
                  return updatedFilters;
                });
              }}
              addNewButtonText={t('BankQuestion.AddBankQuestion')}
              onAddNew={() => setIsModalOpen(true)}
              importButtonText={t('ManageQuestion.importButtonText')}
              onImport={() => setIsImportModalOpen(true)}
              addNewButtonTextV2={t('ManageQuestion.addNewButtonText')}
              onAddNewV2={() => {
                roleId === 2
                  ? navigate(BaseUrl.AddQuestion)
                  : roleId === 4
                    ? navigate(BaseUrl.AdminAddQuestion)
                    : null;
              }}
            />
            <BankQuestionCard refetch={refetch} bankquestion={data} dataSubjects={subjects} />
            {/* Phân trang */}
            {totalPage >= 1 && (
              <div className="mt-4 flex justify-center gap-2">
                <Button
                  variant="outline"
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                >
                  Previous
                </Button>
                {Array.from({ length: totalPage }, (_, i) => i + 1).map((page) => (
                  <Button
                    key={page}
                    variant={currentPage === page ? 'default' : 'outline'}
                    onClick={() => handlePageChange(page)}
                  >
                    {page}
                  </Button>
                ))}
                <Button
                  variant="outline"
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPage}
                >
                  Next
                </Button>
              </div>
            )}
          </div>
        </div>
      </PageWrapper>

      <AddQuestionBankModal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleAddQuestionBank}
        dataSubjects={subjects}
      />

      <ImportWordModal
        isOpen={isImportModalOpen}
        onClose={() => setIsImportModalOpen(false)}
        onImportComplete={handleImportComplete}
        dataSubject={subjects}
        refetch={refetch}
      />
    </div>
  );
};

export default BankQuestion;
