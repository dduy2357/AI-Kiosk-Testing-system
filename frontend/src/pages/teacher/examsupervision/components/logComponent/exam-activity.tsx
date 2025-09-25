import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import BaseUrl from '@/consts/baseUrl';
import { LogType } from '@/consts/common';
import httpService from '@/services/httpService';
import { IListExamActivityLog } from '@/services/modules/examactivitylog/interfaces/examactivitylog.interface';
import { format } from 'date-fns';
import { enUS, vi } from 'date-fns/locale';
import { AlertTriangle, Clock, FileText, Navigation, Settings } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Link, useParams } from 'react-router-dom';

interface ExamActivityProps {
  activities: IListExamActivityLog[];
}

export function ExamActivity({ activities }: Readonly<ExamActivityProps>) {
  const { t, i18n } = useTranslation('shared');
  const dateLocale = i18n.language === 'vi' ? vi : enUS;
  const roleId = Number(httpService.getUserStorage()?.roleId) ?? 0;
  const { examId, studentExamId } = useParams();

  const getActivityIcon = (type: string) => {
    switch (type) {
      case 'question':
        return <FileText className="h-4 w-4 text-blue-600" />;
      case 'navigation':
        return <Navigation className="h-4 w-4 text-green-600" />;
      case 'warning':
        return <AlertTriangle className="h-4 w-4 text-red-600" />;
      case 'system':
        return <Settings className="h-4 w-4 text-gray-600" />;
      default:
        return <Clock className="h-4 w-4 text-gray-600" />;
    }
  };

  const getStatusBadge = (status: number) => {
    switch (status) {
      case LogType.Warning:
        return (
          <Badge className="border-yellow-200 bg-yellow-100 text-yellow-800">
            {t('ExamActivityLog.warning')}
          </Badge>
        );
      case LogType.Info:
        return (
          <Badge className="border-blue-200 bg-blue-100 text-blue-800">
            {t('ExamActivityLog.info')}
          </Badge>
        );
      case LogType.Violation:
        return (
          <Badge className="border-red-200 bg-red-100 text-red-800">
            {t('ExamActivityLog.violation')}
          </Badge>
        );
      case LogType.Critical:
        return (
          <Badge className="border-orange-200 bg-orange-100 text-orange-800">
            {t('ExamActivityLog.critical')}
          </Badge>
        );
      default:
        return null;
    }
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>{t('ExamActivityLog.activities')}</CardTitle>
          <div className="flex items-center space-x-2 text-sm text-gray-500">
            <span>{t('ExamActivityLog.violation')}</span>
            <span>{t('ExamActivityLog.system')}</span>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {activities.map((activity) => (
            <Link
              key={activity?.examLogId}
              to={`${
                roleId === 4 ? BaseUrl.AdminSupervision : BaseUrl.ExamSupervision
              }/${examId}/examLog/${studentExamId}/${activity.examLogId}`}
              className={`flex items-start space-x-3 rounded-lg border p-3 ${
                activity?.logType === LogType.Warning
                  ? 'border-yellow-200 bg-yellow-50'
                  : activity?.logType === LogType.Violation
                    ? 'border-red-100 bg-red-100'
                    : 'border-gray-200 bg-white'
              }`}
            >
              <div className="mt-0.5">{getActivityIcon(activity?.actionType)}</div>
              <div className="min-w-0 flex-1">
                <p className="text-sm text-gray-900">{activity?.description}</p>
                <div className="mt-1 flex items-center justify-between">
                  <span className="text-xs text-gray-500">
                    {activity?.createdAt
                      ? format(new Date(activity.createdAt), 'dd/MM/yyyy HH:mm:ss', {
                          locale: dateLocale,
                        })
                      : ''}
                  </span>
                  {getStatusBadge(activity?.logType)}
                </div>
              </div>
            </Link>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}
