import { UserStats } from '@/pages/admin/manageuser/components/user-stats';
import { MonitorList } from '@/services/modules/monitor/interfaces/monitor.interface';
import { BookOpen, Monitor, TrendingUp, Users } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface ExamStatisticsProps {
  dataMain: MonitorList[];
}

const ExamStatistics = ({ dataMain }: ExamStatisticsProps) => {
  const { t } = useTranslation('shared');
  const statistics = {
    ongoingExams: dataMain.filter((exam) => exam.status === 1).length,
    totalExams: dataMain.length,
    totalStudents: dataMain.reduce((sum, exam) => sum + exam.maxCapacity, 0),
    activeStudents: dataMain.reduce((sum, exam) => sum + exam.studentDoing, 0),
  };

  const statItems = [
    {
      title: t('ExamSupervision.OngoingExams'),
      value: statistics.ongoingExams,
      icon: <Monitor className="h-6 w-6 text-emerald-600" />,
      bgColor: 'bg-gradient-to-br from-emerald-50 to-emerald-100',
    },
    {
      title: t('ExamSupervision.TotalExams'),
      value: statistics.totalExams,
      icon: <BookOpen className="h-6 w-6 text-blue-600" />,
      bgColor: 'bg-gradient-to-br from-blue-50 to-blue-100',
    },
    {
      title: t('ExamSupervision.TotalStudents'),
      value: statistics.totalStudents,
      icon: <Users className="h-6 w-6 text-purple-600" />,
      bgColor: 'bg-gradient-to-br from-purple-50 to-purple-100',
    },
    {
      title: t('ExamSupervision.ActiveStudents'),
      value: statistics.activeStudents,
      icon: <TrendingUp className="h-6 w-6 text-orange-600" />,
      bgColor: 'bg-gradient-to-br from-orange-50 to-orange-100',
    },
  ];

  return <UserStats statItems={statItems} className="grid-cols-1 md:grid-cols-2 lg:grid-cols-4" />;
};

export default ExamStatistics;
