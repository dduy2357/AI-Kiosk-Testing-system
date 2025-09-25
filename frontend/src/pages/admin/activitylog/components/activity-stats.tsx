import { Activity, Users } from 'lucide-react';
import { UserStats } from '../../manageuser/components/user-stats';
import { useTranslation } from 'react-i18next';

interface ActivityStatsProps {
  stats: {
    totalActivities: number;
    activeUsers: number;
  };
}

export function ActivityStats({ stats }: Readonly<ActivityStatsProps>) {
  const { t } = useTranslation('shared');

  const statCards = [
    {
      title: t('ActivityLog.totalActivities'),
      value: stats.totalActivities.toLocaleString(),
      subtitle: t('ActivityLog.last30Days'),
      icon: <Activity className="h-8 w-8 text-emerald-600" />,
      bgColor: 'bg-gradient-to-br from-emerald-50 to-emerald-100',
    },
    {
      title: t('ActivityLog.activeUsers'),
      value: stats.activeUsers.toString(),
      subtitle: t('ActivityLog.last24Hours'),
      icon: <Users className="h-8 w-8 text-blue-600" />,
      bgColor: 'bg-gradient-to-br from-blue-50 to-blue-100',
    },
  ];

  return <UserStats statItems={statCards} className="grid-cols-1 md:grid-cols-2 lg:grid-cols-2" />;
}
