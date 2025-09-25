import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertTriangle, BarChart3, Clock, FileText, HelpCircle, LucideIcon } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import ActivityItem from './activity-item';
import { UserLogList } from '@/services/modules/log/interfaces/log.interface';
import { convertUTCToVietnamTime } from '@/helpers/common';
import { DateTimeFormat } from '@/consts/dates';

interface RecentActivitiesProps {
  dataUserLog?: UserLogList[];
}

const RecentActivities = ({ dataUserLog }: RecentActivitiesProps) => {
  //! State
  const { t } = useTranslation('shared');

  //! Functions

  //! Render
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
        <CardTitle className="flex items-center text-lg font-semibold">
          <Clock className="mr-2 h-5 w-5 text-gray-600" />
          {t('Overview.RecentActivities')}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        <div className="space-y-1">
          {dataUserLog?.slice(0, 5).map((activity, index) => (
            <ActivityItem
              key={index}
              icon={getIconForAction(activity.actionType)} // Hàm để chọn icon
              iconColor="text-gray-700"
              iconBgColor="bg-gray-100"
              title={`${activity.fullName} (${activity.userCode || 'N/A'})`}
              subtitle={activity.description}
              time={
                convertUTCToVietnamTime(
                  activity.createdAt,
                  DateTimeFormat.DateTimeWithTimezone,
                )?.toString() || ''
              }
              badge={getBadgeForAction(activity.actionType)}
            />
          ))}
        </div>
        <div className="border-t p-4">
          <Button variant="ghost" className="w-full text-blue-600 hover:text-blue-700">
            {t('Overview.ViewAllActivities')}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
};

// Hàm chọn icon dựa trên actionType
const getIconForAction = (actionType: string): LucideIcon => {
  switch (actionType.toLowerCase()) {
    case 'create':
      return FileText;
    case 'update':
      return BarChart3;
    case 'delete':
      return AlertTriangle;
    case 'view':
      return HelpCircle;
    default:
      return Clock; // Icon mặc định
  }
};

// Hàm chọn badge dựa trên actionType
const getBadgeForAction = (actionType: string) => {
  switch (actionType.toLowerCase()) {
    case 'create':
      return { text: 'Created', color: 'bg-green-100 text-green-800' };
    case 'update':
      return { text: 'Updated', color: 'bg-blue-100 text-blue-800' };
    case 'delete':
      return { text: 'Deleted', color: 'bg-red-100 text-red-800' };
    case 'view':
      return { text: 'Viewed', color: 'bg-yellow-100 text-yellow-800' };
    default:
      return undefined;
  }
};

export default RecentActivities;
