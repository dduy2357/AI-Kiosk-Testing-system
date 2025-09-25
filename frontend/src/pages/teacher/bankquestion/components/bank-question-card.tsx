import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { CheckCircle, Eye, MenuSquare, Share2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useState } from 'react';
import {
  BankQuestionList,
  IQuestionBankForm,
  IShareBankQuestionForm,
} from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { useNavigate } from 'react-router-dom';
import BaseUrl from '@/consts/baseUrl';
import AddQuestionBankModal from './add-question-bank-modal';
import { SubjectList } from '@/services/modules/subject/interfaces/subject.interface';
import httpService from '@/services/httpService';
import { QUESTION_BANK_URL } from '@/consts/apiUrl';
import { showError, showSuccess } from '@/helpers/toast';
import ShareQuestionBankModal from './share-question-bank-modal';
import bankquestionService from '@/services/modules/bankquestion/bankquestion.Service';
import { AxiosError } from 'axios';
import { Switch } from '@/components/ui/switch';
import { Badge } from '@/components/ui/badge';

interface BankQuestionCardProps {
  refetch: () => void;
  bankquestion: BankQuestionList[];
  dataSubjects: SubjectList[];
}

export default function BankQuestionCard({
  refetch,
  bankquestion,
  dataSubjects,
}: Readonly<BankQuestionCardProps>) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isShareModalOpen, setIsShareModalOpen] = useState(false);
  const [selectedBankQuestion, setSelectedBankQuestion] = useState<BankQuestionList | undefined>(
    undefined,
  );
  const { t } = useTranslation('shared');
  const navigate = useNavigate();
  const roleId = httpService.getUserStorage()?.roleId.at(0);

  const handleEditQuestionBank = (data: IQuestionBankForm, questionBankId?: string) => {
    try {
      httpService.put(`${QUESTION_BANK_URL}/edit/${questionBankId}`, data);
      showSuccess(t('BankQuestion.EditSuccess'));
    } catch (error) {
      showError(t('BankQuestion.EditError'));
      console.error('Error submitting form:', error);
    }
    refetch();
  };

  const handleShareSubmit = async (formData: IShareBankQuestionForm) => {
    try {
      await bankquestionService.getShareBankQuestion(formData);

      showSuccess(t('BankQuestion.ShareSuccess'));
    } catch (error) {
      let errorMessage = t('BankQuestion.ShareError');
      if (error instanceof AxiosError) {
        errorMessage = error.response?.data?.message ?? errorMessage;
      } else if (error instanceof Error) {
        errorMessage = error.message ?? errorMessage;
      }
      showError(errorMessage);
    }
  };

  const getStatusColor = (status: number) => {
    switch (status) {
      case 1:
        return 'bg-green-100 text-green-800 border-green-200';
      case 0:
        return 'bg-gray-100 text-gray-800 border-gray-200';
      default:
        return 'bg-blue-100 text-blue-800 border-blue-200';
    }
  };

  const getStatusText = (status: number) => {
    switch (status) {
      case 1:
        return t('QuestionCard.statusActive');
      case 0:
        return t('QuestionCard.statusInactive');
      default:
        return t('QuestionCard.statusDraft');
    }
  };

  return (
    <div className="mt-4 grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
      {bankquestion && bankquestion.length > 0 ? (
        bankquestion?.map((bankquestion) => (
          <Card key={bankquestion?.questionBankId}>
            <CardHeader className="pb-2">
              <div className="flex items-start justify-between">
                <div className="flex-1">
                  <CardTitle className="text-lg font-medium">{bankquestion?.title}</CardTitle>
                  <CardDescription>{`${t('BankQuestion.Subject')}: ${bankquestion?.subjectName}`}</CardDescription>
                  <div className="mb-3 mt-1 flex flex-wrap gap-2">
                    <Badge variant="outline" className={getStatusColor(bankquestion.status)}>
                      <CheckCircle className="mr-1 h-3 w-3" />
                      {getStatusText(bankquestion.status)}
                    </Badge>
                  </div>
                </div>
                <div className="ml-2 flex gap-1">
                  <Switch
                    checked={bankquestion.status === 1}
                    onCheckedChange={async () => {
                      await bankquestionService.toggleQuestionBank(bankquestion?.questionBankId);
                      showSuccess(t('QuestionCard.ToggleSuccess'));
                      refetch();
                    }}
                    className="mt-0.7 h-[20px] w-[36px] data-[state=checked]:bg-primary data-[state=unchecked]:bg-input [&>span]:h-4 [&>span]:w-4 [&>span]:data-[state=checked]:translate-x-4 [&>span]:data-[state=unchecked]:translate-x-0"
                    aria-label="Toggle question bank status"
                  />
                </div>
              </div>
              <p className="text-sm text-muted-foreground">
                {bankquestion?.totalQuestions} {t('BankQuestion.Question')}
              </p>
            </CardHeader>
            <CardContent>
              <div className="mb-4 grid grid-cols-2 gap-4">
                <div>
                  <div className="text-sm font-medium">{t('BankQuestion.MultipleChoice')}:</div>
                  <div className="text-lg">{bankquestion?.multipleChoiceCount}</div>
                </div>
                <div>
                  <div className="text-sm font-medium">{t('BankQuestion.Essay')}:</div>
                  <div className="text-lg">{bankquestion?.essayCount}</div>
                </div>
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  className="flex flex-1 items-center justify-center gap-2"
                  onClick={() => {
                    roleId === 2
                      ? navigate(`${BaseUrl.BankQuestion}/${bankquestion.questionBankId}`)
                      : navigate(`${BaseUrl.AdminBankQuestion}/${bankquestion.questionBankId}`);
                  }}
                >
                  <Eye size={16} />
                  {t('View')}
                </Button>
                <Button
                  variant="outline"
                  className="flex flex-1 items-center justify-center gap-2"
                  onClick={() => {
                    setIsModalOpen(true);
                    setSelectedBankQuestion(bankquestion);
                  }}
                >
                  <MenuSquare size={16} />
                  {t('Edit')}
                </Button>
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => {
                    setIsShareModalOpen(true);
                    setSelectedBankQuestion(bankquestion);
                  }}
                >
                  <Share2 size={16} />
                </Button>
              </div>
            </CardContent>
          </Card>
        ))
      ) : (
        <div className="col-span-full py-8 text-center text-gray-500">
          {t('BankQuestion.Nodata')}
        </div>
      )}
      <ShareQuestionBankModal
        isOpen={isShareModalOpen}
        onClose={() => setIsShareModalOpen(false)}
        onSubmit={handleShareSubmit}
        questionBank={selectedBankQuestion}
      />

      {/* modal edit bank question */}
      <AddQuestionBankModal
        id={selectedBankQuestion?.questionBankId}
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        onSubmit={handleEditQuestionBank}
        dataSubjects={dataSubjects}
      />
    </div>
  );
}
