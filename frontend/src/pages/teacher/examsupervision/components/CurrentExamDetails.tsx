import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardTitle } from '@/components/ui/card';
import { MonitorList } from '@/services/modules/monitor/interfaces/monitor.interface';
import { Clock } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface CurrentExamDetailsProps {
  currentExam: MonitorList;
  getStatusBadge: (status: number) => JSX.Element;
  getRemainingTime: (endTime: Date) => string;
}

const CurrentExamDetails = ({
  currentExam,
  getStatusBadge,
  getRemainingTime,
}: CurrentExamDetailsProps) => {
  const { t } = useTranslation('shared');
  return (
    <Card className="overflow-hidden border-0 shadow-lg">
      <div className="bg-gradient-to-r from-blue-600 to-blue-700 p-4">
        <CardTitle className="flex items-center space-x-2 text-lg text-white">
          <Clock className="h-5 w-5" />
          <span>{t('ExamSupervision.CurrentExam')}</span>
        </CardTitle>
      </div>
      <CardContent className="space-y-4 p-6">
        <div>
          <p className="mb-1 text-sm text-gray-600">{t('ExamSupervision.ExamTitle')}</p>
          <p className="font-semibold text-gray-900">{currentExam.title}</p>
        </div>
        <div>
          <p className="mb-1 text-sm text-gray-600">{t('ExamSupervision.ClassSubject')}</p>
          <div className="flex items-center space-x-2">
            <span className="font-semibold">{currentExam.classCode} -</span>
            <Badge variant="outline" className="border-blue-200 bg-blue-50 text-blue-600">
              {currentExam.subjectName}
            </Badge>
          </div>
          <div>{getStatusBadge(currentExam.status)}</div>
        </div>
        <div>
          <p className="mb-1 text-sm text-gray-600">{t('ExamSupervision.Supervisor')}</p>
          <p className="font-semibold">{currentExam.createUserName}</p>
        </div>
        <div>
          <p className="mb-1 text-sm text-gray-600">{t('ExamSupervision.RemainingTime')}</p>
          <p className="font-semibold text-red-600">{getRemainingTime(currentExam.endTime)}</p>
        </div>
        <div className="flex items-center justify-between border-t pt-4">
          <div className="text-center">
            <p className="text-2xl font-bold text-orange-600">{currentExam.studentDoing}</p>
            <p className="text-sm text-gray-600">{t('ExamSupervision.DoingExam')}</p>
          </div>
          <div className="text-center">
            <p className="text-2xl font-bold text-emerald-600">{currentExam.studentCompleted}</p>
            <p className="text-sm text-gray-600">{t('ExamSupervision.Completed')}</p>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default CurrentExamDetails;
