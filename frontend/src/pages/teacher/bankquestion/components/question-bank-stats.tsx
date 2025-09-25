import { UserStats } from '@/pages/admin/manageuser/components/user-stats';
import { Landmark, TextQuote, TextSelectIcon } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface StatsProps {
  stats: {
    questionBanks: number;
    totalQuestions: number;
    subjects: number;
  };
}

export default function QuestionBankStats({ stats }: Readonly<StatsProps>) {
  const { t } = useTranslation('shared');
  const statItems = [
    {
      title: t('BankQuestion.BankQuestion'),
      value: stats.questionBanks,
      icon: <Landmark className="h-6 w-6 text-emerald-600" />,
      bgColor: 'bg-gradient-to-br from-emerald-50 to-emerald-100',
    },
    {
      title: t('BankQuestion.TotalQuestions'),
      value: stats.totalQuestions,
      icon: <TextQuote className="h-6 w-6 text-blue-600" />,
      bgColor: 'bg-gradient-to-br from-blue-50 to-blue-100',
    },
    {
      title: t('BankQuestion.NameSubject'),
      value: stats.subjects,
      icon: <TextSelectIcon className="h-6 w-6 text-purple-600" />,
      bgColor: 'bg-gradient-to-br from-purple-50 to-purple-100',
    },
  ];
  return <UserStats statItems={statItems} className="grid-cols-1 md:grid-cols-2 lg:grid-cols-3" />;
}
