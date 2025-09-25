import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Progress } from '@/components/ui/progress';
import BaseUrl from '@/consts/baseUrl';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime } from '@/helpers/common';
import { showError, showSuccess } from '@/helpers/toast';
import useToggleDialog from '@/hooks/useToggleDialog';
import httpService from '@/services/httpService';
import alertService from '@/services/modules/alert/alert.service';
import { MonitorList } from '@/services/modules/monitor/interfaces/monitor.interface';
import monitorService from '@/services/modules/monitor/monitor.service';
import { BookOpen, Eye, MapPin, MoreHorizontal, Pause, Plus, Timer, Users } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import DialogSendAlert, { SendAlertValues } from '../dialogs/DialogSendAlert';
import { useTranslation } from 'react-i18next';

interface ExamCardProps {
  exam: MonitorList;
  getStatusBadge: (status: number) => JSX.Element;
  getRemainingTime: (endTime: Date) => string;
  getCompletionPercentage: (exam: MonitorList) => number;
  refetch?: () => void;
}

const ExamCard = ({
  exam,
  getStatusBadge,
  getRemainingTime,
  getCompletionPercentage,
  refetch,
}: ExamCardProps) => {
  //!State
  const { t } = useTranslation('shared');
  const navigate = useNavigate();
  const roleId = Number(httpService.getUserStorage()?.roleId) || 0;
  const [openSendAlert, toggleSendAlert, shouldRenderSendAlert] = useToggleDialog();

  //!Functions
  const handleSendAlert = async (values: SendAlertValues) => {
    try {
      await alertService.sendAlert(values);
      showSuccess(t('ExamSupervision.AlertSentSuccess'));
      toggleSendAlert();
      refetch && refetch();
    } catch (error) {
      showError(error);
    }
  };

  //!Render
  return (
    <Card className="group border border-emerald-200 bg-gradient-to-r from-emerald-50/50 to-blue-50/30 transition-all duration-300 hover:border-emerald-300 hover:shadow-lg">
      {shouldRenderSendAlert && (
        <DialogSendAlert
          isOpen={openSendAlert}
          toggle={toggleSendAlert}
          onSubmit={handleSendAlert}
          createUserId={exam?.createUserId || ''}
        />
      )}
      <CardContent className="p-6">
        <div className="mb-4 flex items-start justify-between">
          <div className="flex-1">
            <div className="mb-3 flex items-center space-x-3">
              <h3 className="text-lg font-semibold text-gray-900">{exam.title}</h3>
              {getStatusBadge(exam.status)}
              <Badge variant="outline" className="bg-white/50">
                {exam.duration} {t('ExamSupervision.Minutes')}
              </Badge>
            </div>

            <div className="grid grid-cols-2 gap-4 text-sm">
              <div className="flex items-center space-x-2">
                <MapPin className="h-4 w-4 text-gray-500" />
                <span className="text-gray-600">{t('ExamSupervision.Room')}:</span>
                <span className="font-medium text-gray-900">{exam.roomCode}</span>
              </div>
              <div className="flex items-center space-x-2">
                <Users className="h-4 w-4 text-gray-500" />
                <span className="text-gray-600">{t('ExamSupervision.Class')}:</span>
                <span className="font-medium text-gray-900">{exam.classCode}</span>
              </div>
              <div className="flex items-center space-x-2">
                <BookOpen className="h-4 w-4 text-gray-500" />
                <span className="text-gray-600">{t('ExamSupervision.Subject')}:</span>
                <Badge variant="outline" className="border-blue-200 bg-blue-50 text-blue-700">
                  {exam.subjectName}
                </Badge>
              </div>
              <div className="flex items-center space-x-2">
                <Timer className="h-4 w-4 text-gray-500" />
                <span className="text-gray-600">{t('ExamSupervision.RemainingTime')}:</span>
                <span className="font-medium text-red-600">{getRemainingTime(exam.endTime)}</span>
              </div>
            </div>

            <div className="mt-4">
              <div className="mb-2 flex items-center justify-between text-sm">
                <span className="text-gray-600">{t('ExamSupervision.CompletionProgress')}</span>
                <span className="font-medium">{getCompletionPercentage(exam)}%</span>
              </div>
              <Progress value={getCompletionPercentage(exam)} className="h-2" />
            </div>
          </div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="h-7 w-7 rounded p-0 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
              >
                <MoreHorizontal className="h-3.5 w-3.5 text-gray-600 dark:text-gray-400" />
                <span className="sr-only">{t('ExamSupervision.MoreOptions')}</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-40">
              <DropdownMenuItem
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                onClick={() =>
                  navigate(
                    `${
                      roleId === 4
                        ? BaseUrl.AdminSupervision
                        : roleId === 3
                          ? BaseUrl.SupervisorExamSupervision
                          : BaseUrl.ExamSupervision
                    }/${exam.examId}`,
                  )
                }
              >
                <Eye className="mr-2 h-3.5 w-3.5 text-blue-500" />
                <span>{t('ExamSupervision.ViewDetails')}</span>
              </DropdownMenuItem>

              <DropdownMenuItem
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                onClick={toggleSendAlert}
              >
                <Plus className="mr-2 h-3.5 w-3.5 text-blue-500" />
                <span>{t('ExamSupervision.SendAlert')}</span>
              </DropdownMenuItem>

              {!exam.isCompleted && (
                <DropdownMenuItem
                  className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                  onClick={async () => {
                    try {
                      await monitorService.finishExam({
                        examId: exam.examId,
                      });
                      showSuccess(t('ExamSupervision.ExamStopped'));
                      refetch && refetch();
                    } catch (error) {
                      showError(error);
                    }
                  }}
                >
                  <Pause className="mr-2 h-3.5 w-3.5 text-red-500" />
                  <span>{t('ExamSupervision.StopExam')}</span>
                </DropdownMenuItem>
              )}

              {exam.isCompleted && (
                <DropdownMenuItem
                  className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                  onClick={async () => {
                    try {
                      await monitorService.reAssignExams({
                        examId: exam.examId,
                        studentIds: exam.studentIds.map((student: string) => student),
                      });
                      showSuccess(t('ExamSupervision.ReAssignExamSuccess'));
                      refetch && refetch();
                    } catch (error) {
                      showError(error);
                    }
                  }}
                >
                  <Users className="mr-2 h-3.5 w-3.5 text-green-500" />
                  <span>{t('ExamSupervision.ReAssignExam')}</span>
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        <div className="flex items-center justify-between border-t border-gray-200 pt-4">
          <div className="flex items-center space-x-6 text-sm">
            <div className="flex items-center space-x-2">
              <div className="h-2 w-2 rounded-full bg-blue-500"></div>
              <span className="text-gray-600">{t('ExamSupervision.MaxCapacity')}:</span>
              <span className="font-medium">{exam.maxCapacity}</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="h-2 w-2 rounded-full bg-orange-500"></div>
              <span className="text-gray-600">{t('ExamSupervision.DoingExam')}:</span>
              <span className="font-medium text-orange-600">{exam.studentDoing}</span>
            </div>
            <div className="flex items-center space-x-2">
              <div className="h-2 w-2 rounded-full bg-emerald-500"></div>
              <span className="text-gray-600">{t('ExamSupervision.Completed')}:</span>
              <span className="font-medium text-emerald-600">{exam.studentCompleted}</span>
            </div>
          </div>
          <div className="text-xs text-gray-500">
            {convertUTCToVietnamTime(
              exam?.startTime,
              DateTimeFormat.DateTimeWithTimezone,
            )?.toString()}{' '}
            -{' '}
            {convertUTCToVietnamTime(
              exam?.endTime,
              DateTimeFormat.DateTimeWithTimezone,
            )?.toString()}
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default ExamCard;
