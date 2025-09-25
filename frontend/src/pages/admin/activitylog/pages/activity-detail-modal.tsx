import { Dialog, DialogContent, DialogHeader } from '@/components/ui/dialog';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { User, Globe } from 'lucide-react';
import { UserActivityLogDetail } from '@/services/modules/useractivitylog/interfaces/useractivitylog.interface';
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';
import { vi, enUS } from 'date-fns/locale';

interface ActivityDetailModalProps {
  isOpen: boolean;
  onClose: () => void;
  activity: UserActivityLogDetail;
}

export function ActivityDetailModal({
  isOpen,
  onClose,
  activity,
}: Readonly<ActivityDetailModalProps>) {
  const { t, i18n } = useTranslation('shared');
  const dateLocale = i18n.language === 'vi' ? vi : enUS;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-h-[90vh] max-w-4xl overflow-y-auto">
        <DialogHeader className="border-b pb-4">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-xl font-semibold">{t('ActivityLogDetail.title')}</h2>
              <p className="mt-1 text-sm text-gray-500">
                {t('ActivityLogDetail.id')}: {(activity as UserActivityLogDetail)?.logId ?? 'N/A'}
              </p>
            </div>
          </div>

          {/* Activity Summary */}
          <div className="mt-4 flex items-center space-x-4 rounded-lg bg-gray-50 p-4">
            <div className="flex h-10 w-10 items-center justify-center rounded-full bg-green-100">
              <User className="h-5 w-5 text-green-600" />
            </div>
            <div className="flex-1">
              <div className="flex items-center space-x-3">
                <h3 className="text-lg font-semibold">{activity?.actionType}</h3>
              </div>
              <p className="mt-1 text-gray-600">{activity?.description}</p>
              <p className="mt-1 text-sm text-gray-500">
                {activity?.createdAt
                  ? format(new Date(activity.createdAt), 'dd/MM/yyyy HH:mm:ss', {
                      locale: dateLocale,
                    })
                  : ''}
              </p>
              <p className="mt-1 text-sm text-gray-500">{activity?.metadata}</p>
            </div>
          </div>
        </DialogHeader>

        <div className="mt-6">
          <div className="grid grid-cols-2 gap-6">
            {/* User Information */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <User className="h-5 w-5" />
                  <span>{t('ActivityLogDetail.userInfo')}</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="flex items-center space-x-3">
                  <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-100">
                    <User className="h-5 w-5 text-blue-600" />
                  </div>
                  <div>
                    <p className="font-medium">{activity?.fullName}</p>
                    <p className="text-sm text-gray-500">{activity?.userCode}</p>
                  </div>
                </div>

                <div className="space-y-3 text-sm">
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('ActivityLogDetail.id')}:</span>
                    <span className="font-mono">{activity?.userId}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('ActivityLogDetail.email')}:</span>
                    <span>{'email' in activity ? activity.email : t('ActivityLogDetail.na')}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="text-gray-500">{t('ActivityLogDetail.lastLogin')}:</span>
                    <span>
                      {'lastLogin' in activity
                        ? format(new Date(activity.lastLogin), 'dd/MM/yyyy HH:mm:ss', {
                            locale: dateLocale,
                          })
                        : t('ActivityLogDetail.na')}
                    </span>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Session Information */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center space-x-2">
                  <Globe className="h-5 w-5" />
                  <span>{t('ActivityLogDetail.sessionInfo')}</span>
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-3 text-sm">
                  <div className="flex items-center space-x-2">
                    <Globe className="h-4 w-4 text-gray-400" />
                    <span>{activity?.ipAddress}</span>
                  </div>
                  <div>
                    <div className="text-gray-500">{t('ActivityLogDetail.browserInfo')}:</div>
                    <br />
                    <span>{activity?.browserInfo}</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
