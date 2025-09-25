import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { IListExamActivityLog } from '@/services/modules/examactivitylog/interfaces/examactivitylog.interface';
import { AlertTriangle } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import AlertItem from './alert-item';

interface Alert {
  dataListExamLogActivity: IListExamActivityLog[];
}

const LatestAlerts = ({ dataListExamLogActivity }: Alert) => {
  //! State
  const { t } = useTranslation('shared');

  //! Functions
  const parseMetadata = (metadata: string) => {
    try {
      type Process = { Name: string; InstanceCount: number };
      const processes: Process[] = JSON.parse(metadata.replace(/\r\n/g, ''));
      return {
        totalProcesses: processes.length,
      };
    } catch (e) {
      return { totalProcesses: 0 };
    }
  };

  //! Render
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-4">
        <CardTitle className="flex items-center text-lg font-semibold">
          <AlertTriangle className="mr-2 h-5 w-5 text-red-600" />
          {t('Overview.LatestAlerts')}
        </CardTitle>
      </CardHeader>
      <CardContent className="p-0">
        <div className="space-y-1">
          {dataListExamLogActivity?.slice(0, 5).map((alert, index) => {
            const { totalProcesses } = parseMetadata(alert.metadata);
            return (
              <AlertItem
                key={index}
                title={`${alert.fullName} - ${alert.actionType}`}
                location={`${t('Overview.TotalProcess')}: ${totalProcesses}`}
                time={new Date(alert.createdAt).toLocaleTimeString([], {
                  hour: '2-digit',
                  minute: '2-digit',
                })}
                severity={alert.logType === 0 ? 'low' : 'medium'}
                type="info"
              />
            );
          })}
        </div>
        <div className="border-t p-4">
          <Button variant="ghost" className="w-full text-blue-600 hover:text-blue-700">
            {t('Overview.ViewAllWarnings')}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
};

export default LatestAlerts;
