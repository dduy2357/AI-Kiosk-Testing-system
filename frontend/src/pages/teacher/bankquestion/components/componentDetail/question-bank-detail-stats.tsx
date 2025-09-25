import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { QuestionBankDetail } from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { FileText, CheckCircle, PenTool } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface QuestionBankStatsProps {
  questionBank: QuestionBankDetail | null;
}

export function QuestionBankDetailStats({ questionBank }: Readonly<QuestionBankStatsProps>) {
  const { t } = useTranslation('shared');
  const stats = [
    {
      title: t('BankQuestion.TotalQuestions'),
      value: questionBank?.totalQuestions,
      icon: FileText,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
    },
    {
      title: t('BankQuestion.MultipleChoice'),
      value: questionBank?.multipleChoiceCount,
      icon: CheckCircle,
      color: 'text-green-600',
      bgColor: 'bg-green-50',
    },
    {
      title: t('BankQuestion.Essay'),
      value: questionBank?.essayCount,
      icon: PenTool,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
    },
  ];

  return (
    <Card className="flex h-full flex-col">
      <CardHeader>
        <CardTitle>{t('BankQuestion.Statistical')}</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-1 flex-col space-y-4">
        <div className="flex-1 space-y-4">
          {stats.map((stat, index) => {
            const Icon = stat.icon;
            return (
              <div key={index} className={`rounded-lg p-4 ${stat.bgColor}`}>
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm text-gray-600">{stat.title}</p>
                    <p className="text-2xl font-bold text-gray-900">{stat.value}</p>
                  </div>
                  <Icon className={`h-8 w-8 ${stat.color}`} />
                </div>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
