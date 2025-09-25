import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Monitor, Wifi } from 'lucide-react';
import useGetDetailExamActivityLog from '@/services/modules/examactivitylog/hooks/useGetDetailExamActivityLog';
import { useTranslation } from 'react-i18next';

interface SystemexamLogDetailProps {
  examLogId: string;
}

export function SystemInfo({ examLogId }: Readonly<SystemexamLogDetailProps>) {
  const { t } = useTranslation('shared');
  const { data: examLogDetail } = useGetDetailExamActivityLog(examLogId);

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center space-x-2">
            <Monitor className="h-5 w-5" />
            <span>{t('ExamActivityLog.deviceInfo')}</span>
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-500">{t('ExamActivityLog.deviceName')}</span>
            <span className="font-medium">{examLogDetail?.deviceUsername}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">{t('ExamActivityLog.deviceId')}</span>
            <span className="font-medium">{examLogDetail?.deviceId}</span>
          </div>
          <div className="flex justify-between">
            <span className="text-gray-500">{t('ExamActivityLog.browserInfo')}</span>
            <span className="font-medium">{examLogDetail?.browserInfo}</span>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center space-x-2">
            <Wifi className="h-5 w-5" />
            <span>{t('ExamActivityLog.networkInfo')}</span>
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3 text-sm">
          <div className="flex justify-between">
            <span className="text-gray-500">{t('ExamActivityLog.ipAddress')}</span>
            <span className="font-medium">{examLogDetail?.ipAddress}</span>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
