import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Eye, User, Clock } from 'lucide-react';
import { IListUserActivityLog } from '@/services/modules/useractivitylog/interfaces/useractivitylog.interface';
import { IListExamActivityLog } from '@/services/modules/examactivitylog/interfaces/examactivitylog.interface';
import { ActivityDetailModal } from '../pages/activity-detail-modal';
import { useEffect, useState } from 'react';
import useGetDetailUserActivityLog from '@/services/modules/useractivitylog/hooks/useGetDetailUserActivityLog';
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';
import { vi, enUS } from 'date-fns/locale';

interface ActivityListProps {
  activities: IListUserActivityLog[] | IListExamActivityLog[];
}

export function ActivityList({ activities }: Readonly<ActivityListProps>) {
  const { t, i18n } = useTranslation('shared');
  const dateLocale = i18n.language === 'vi' ? vi : enUS;
  const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
  const [activityDetail, setActivityDetail] = useState<any>(null);
  const [selectedId, setSelectedId] = useState<string | undefined>(undefined);

  const { data: userActivityData } = useGetDetailUserActivityLog(selectedId, {
    isTrigger: !!selectedId,
  });

  useEffect(() => {
    if (selectedId && userActivityData) {
      setActivityDetail(userActivityData);
      setIsDetailModalOpen(true);
    }
  }, [selectedId, userActivityData]);

  const handleViewDetail = (logId: string) => {
    setSelectedId(logId);
  };

  const handleCloseModal = () => {
    setIsDetailModalOpen(false);
    setSelectedId(undefined);
    setActivityDetail(null);
  };

  return (
    <>
      <Card>
        <CardHeader>
          <CardTitle>
            {t('ActivityLog.activityLogTitle')} ({activities.length})
          </CardTitle>
          <p className="text-sm text-gray-600">{t('ActivityLog.sortedByRecent')}</p>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {activities.map((activity) => {
              const key = 'logId' in activity ? activity.logId : activity.examLogId;
              return (
                <div
                  key={key}
                  className="rounded-lg border border-gray-200 p-4 transition-colors hover:border-gray-400 hover:bg-gray-50"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="mb-2 flex items-center space-x-3">
                        <span className="font-mono text-sm font-medium text-gray-900">
                          {activity?.actionType}
                        </span>
                      </div>
                      <div className="mb-2 flex items-center space-x-4 text-sm text-gray-600">
                        <div className="flex items-center space-x-1">
                          <User className="h-4 w-4" />
                          <span className="font-medium">{activity?.fullName}</span>
                        </div>
                        <div className="flex items-center space-x-1">
                          <Clock className="h-4 w-4" />
                          <span>
                            {activity?.createdAt
                              ? format(new Date(activity.createdAt), 'dd/MM/yyyy HH:mm:ss', {
                                  locale: dateLocale,
                                })
                              : ''}
                          </span>
                        </div>
                      </div>
                      <p className="mb-3 text-gray-700">{activity?.description}</p>
                    </div>
                    <div className="ml-4">
                      <button
                        onClick={() => handleViewDetail(key)}
                        className="rounded-full p-2 text-gray-400 transition-colors hover:bg-gray-100 hover:text-gray-600"
                        aria-label={t('ActivityLog.viewDetails')}
                      >
                        <Eye className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>
      {activityDetail && (
        <ActivityDetailModal
          isOpen={isDetailModalOpen}
          onClose={handleCloseModal}
          activity={activityDetail}
        />
      )}
    </>
  );
}
