import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import React from 'react';
import { Plus, FileText, Building, BarChart3 } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import BaseUrl from '@/consts/baseUrl';

interface QuickAccessItemProps {
  icon: React.ReactNode;
  title: string;
  onClick?: () => void;
}

const QuickAccessItem = ({ icon, title, onClick }: QuickAccessItemProps) => {
  return (
    <Button
      variant="ghost"
      className="flex h-auto flex-col items-center space-y-3 rounded-lg border border-gray-200 p-6 transition-all duration-200 hover:bg-gray-50 hover:shadow-md"
      onClick={onClick}
    >
      <div className="flex items-center justify-center">{icon}</div>
      <span className="text-center text-sm font-medium leading-tight text-gray-700">{title}</span>
    </Button>
  );
};

const QuickAccess = () => {
  //! State
  const { t } = useTranslation('shared');
  const navigate = useNavigate();

  //! Functions
  const handleAddQuestionBank = () => {
    navigate(BaseUrl.BankQuestion);
  };

  const handleCreateExam = () => {
    navigate(BaseUrl.ManageExam);
  };

  const handleViewExamMonitoring = () => {
    navigate(BaseUrl.ExamSupervision);
  };

  const handleViewExamResults = () => {
    navigate(BaseUrl.ExamResultTeacher);
  };

  //! Render
  return (
    <Card className="mt-6">
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
        <div>
          <CardTitle className="flex items-center text-lg font-semibold text-gray-900">
            {t('Overview.QuickAccess')}
          </CardTitle>
          <p className="mt-1 text-sm text-gray-500">{t('Overview.CommonUseTasks')}</p>
        </div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <QuickAccessItem
            icon={
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-purple-100">
                <Plus className="h-6 w-6 text-purple-600" />
              </div>
            }
            title={t('Overview.AddQuestionBank')}
            onClick={handleAddQuestionBank}
          />

          <QuickAccessItem
            icon={
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-green-100">
                <FileText className="h-6 w-6 text-green-600" />
              </div>
            }
            title={t('Overview.CreateNewExamPaper')}
            onClick={handleCreateExam}
          />

          <QuickAccessItem
            icon={
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-blue-100">
                <Building className="h-6 w-6 text-blue-600" />
              </div>
            }
            title={t('Overview.ViewExamMonitoring')}
            onClick={handleViewExamMonitoring}
          />

          <QuickAccessItem
            icon={
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-orange-100">
                <BarChart3 className="h-6 w-6 text-orange-600" />
              </div>
            }
            title={t('Overview.ViewExamResults')}
            onClick={handleViewExamResults}
          />
        </div>
      </CardContent>
    </Card>
  );
};

export default QuickAccess;
