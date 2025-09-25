import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { ActiveStatusExamStudent } from '@/consts/common';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime } from '@/helpers/common';
import type { DialogI } from '@/interfaces/common';
import useGetExamDetail from '@/services/modules/manageexam/hooks/useGetExamDetail';
import {
  AlertCircle,
  BookOpen,
  Calendar,
  CheckCircle,
  Clock,
  FileText,
  Settings,
  Target,
  Timer,
  Users,
  XCircle,
} from 'lucide-react';
import moment from 'moment';
import React from 'react';
import { useTranslation } from 'react-i18next';

interface DialogExamDetailProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  examId?: string;
}

const DialogExamDetail = (props: DialogExamDetailProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, examId } = props;

  const { data: examDetail, isLoading: isLoadingExamDetail } = useGetExamDetail(examId, {
    isTrigger: !!examId,
  });

  const getStatusConfig = (status: number) => {
    switch (status) {
      case ActiveStatusExamStudent.Published:
        return {
          label: t('ExamManagement.Published'),
          variant: 'default' as const,
          className: 'bg-green-100 text-green-800 hover:bg-green-100',
          icon: CheckCircle,
        };
      case ActiveStatusExamStudent.Draft:
        return {
          label: t('ExamManagement.Draft'),
          variant: 'secondary' as const,
          className: 'bg-yellow-100 text-yellow-800 hover:bg-yellow-100',
          icon: AlertCircle,
        };
      default:
        return {
          label: t('ExamManagement.Cancelled'),
          variant: 'destructive' as const,
          className: 'bg-red-100 text-red-800 hover:bg-red-100',
          icon: XCircle,
        };
    }
  };

  const statusConfig = examDetail ? getStatusConfig(examDetail.status) : null;
  const StatusIcon = statusConfig?.icon;

  if (isLoadingExamDetail) {
    return (
      <Dialog open={isOpen} onOpenChange={toggle}>
        <DialogContent className="max-w-4xl">
          <div className="flex h-64 items-center justify-center">
            <div className="flex items-center space-x-3">
              <div className="h-8 w-8 animate-spin rounded-full border-4 border-blue-200 border-t-blue-600"></div>
              <span className="text-lg text-gray-600">{t('ExamManagement.Loading')}</span>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    );
  }

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogContent className="max-h-[95vh] max-w-6xl overflow-y-auto">
        <DialogHeader className="space-y-4 pb-6">
          <div className="flex items-start justify-between">
            <div className="space-y-2">
              <DialogTitle className="flex items-center space-x-2 text-2xl font-bold text-gray-900">
                <BookOpen className="h-6 w-6 text-blue-600" />
                <span>{t('ExamManagement.ExamDetail')}</span>
              </DialogTitle>
              <p className="text-base text-gray-600">{t('ExamManagement.ExamDetailDescription')}</p>
            </div>
          </div>

          <div className="relative overflow-hidden rounded-xl border border-blue-200 bg-gradient-to-r from-blue-50 to-indigo-50 p-6 shadow-sm">
            <div className="absolute right-0 top-0 h-full w-32 bg-gradient-to-l from-blue-100/50 to-transparent"></div>
            <div className="relative">
              <h3 className="mb-3 line-clamp-2 text-xl font-bold text-blue-900">
                {examDetail?.title}
              </h3>
              <div className="flex flex-wrap items-center gap-4 text-sm">
                <span className="flex items-center space-x-2 rounded-full bg-white/70 px-3 py-1.5 text-blue-700 shadow-sm">
                  <Users className="h-4 w-4" />
                  <span className="font-medium">{examDetail?.roomName}</span>
                </span>
                <span className="flex items-center space-x-2 rounded-full bg-white/70 px-3 py-1.5 text-blue-700 shadow-sm">
                  <Calendar className="h-4 w-4" />
                  <span className="font-medium">
                    {convertUTCToVietnamTime(
                      examDetail?.startTime,
                      DateTimeFormat.DateTimeWithTimezone,
                    )?.toString()}
                  </span>
                </span>
                {statusConfig && StatusIcon && (
                  <Badge
                    variant={statusConfig.variant}
                    className={`${statusConfig.className} flex items-center space-x-1.5 px-3 py-1.5 shadow-sm`}
                  >
                    <StatusIcon className="h-3.5 w-3.5" />
                    <span className="font-medium">{statusConfig.label}</span>
                  </Badge>
                )}
              </div>
            </div>
          </div>
        </DialogHeader>

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-2 xl:grid-cols-3">
          {/* Thông tin cơ bản */}
          <Card className="group border-l-4 border-l-blue-500 transition-all duration-200 hover:shadow-lg">
            <CardHeader className="pb-4">
              <CardTitle className="flex items-center space-x-2 text-base font-semibold text-gray-800">
                <div className="rounded-lg bg-blue-100 p-2 transition-colors group-hover:bg-blue-200">
                  <FileText className="h-4 w-4 text-blue-600" />
                </div>
                <span>{t('ExamManagement.BasicInfo')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.ExamID')}:</span>
                  <span className="rounded bg-gray-100 px-2 py-1 text-xs font-semibold text-gray-900">
                    #{examDetail?.examId}
                  </span>
                </div>
                <Separator className="opacity-50" />
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.Class')}:</span>
                  <span className="font-medium text-gray-900">{examDetail?.roomName}</span>
                </div>
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.Subject')}:</span>
                  <span className="font-medium text-gray-900">{examDetail?.title}</span>
                </div>
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.Author')}:</span>
                  <span className="font-medium text-gray-900">{examDetail?.createdBy}</span>
                </div>
                <Separator className="opacity-50" />
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.CreatedAt')}:</span>
                  <span className="font-medium text-gray-900">
                    {convertUTCToVietnamTime(
                      examDetail?.startTime,
                      DateTimeFormat.DateTimeWithTimezone,
                    )?.toString()}
                  </span>
                </div>
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.UpdatedAt')}:</span>
                  <span className="font-medium text-gray-900">
                    {convertUTCToVietnamTime(
                      examDetail?.endTime,
                      DateTimeFormat.DateTimeWithTimezone,
                    )?.toString()}
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Thời gian & Cấu trúc */}
          <Card className="group border-l-4 border-l-green-500 transition-all duration-200 hover:shadow-lg">
            <CardHeader className="pb-4">
              <CardTitle className="flex items-center space-x-2 text-base font-semibold text-gray-800">
                <div className="rounded-lg bg-green-100 p-2 transition-colors group-hover:bg-green-200">
                  <Clock className="h-4 w-4 text-green-600" />
                </div>
                <span>{t('ExamManagement.TimeAndStructure')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.StartTime')}:</span>
                  <span className="rounded bg-green-50 px-2 py-1 text-xs font-semibold text-green-700">
                    {moment(examDetail?.startTime).format('HH:mm')}
                  </span>
                </div>
                <div className="flex items-center justify-between py-1">
                  <span className="text-sm text-gray-600">{t('ExamManagement.EndTime')}:</span>
                  <span className="rounded bg-red-50 px-2 py-1 text-xs font-semibold text-red-700">
                    {moment(examDetail?.endTime).format('HH:mm')}
                  </span>
                </div>
                <Separator className="opacity-50" />
                <div className="flex items-center justify-between py-1">
                  <span className="flex items-center space-x-1 text-sm text-gray-600">
                    <Timer className="h-3 w-3" />
                    <span>{t('ExamManagement.Duration')}:</span>
                  </span>
                  <span className="font-medium text-gray-900">{examDetail?.duration} minutes</span>
                </div>
                <div className="flex items-center justify-between py-1">
                  <span className="flex items-center space-x-1 text-sm text-gray-600">
                    <FileText className="h-3 w-3" />
                    <span>{t('ExamManagement.TotalQuestions')}:</span>
                  </span>
                  <span className="font-medium text-gray-900">
                    {examDetail?.totalQuestions} questions
                  </span>
                </div>
                <div className="flex items-center justify-between py-1">
                  <span className="flex items-center space-x-1 text-sm text-gray-600">
                    <Target className="h-3 w-3" />
                    <span>{t('ExamManagement.MaxPoints')}:</span>
                  </span>
                  <span className="font-medium text-gray-900">
                    {examDetail?.totalPoints} points
                  </span>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Cài đặt */}
          <Card className="group border-l-4 border-l-purple-500 transition-all duration-200 hover:shadow-lg">
            <CardHeader className="pb-4">
              <CardTitle className="flex items-center space-x-2 text-base font-semibold text-gray-800">
                <div className="rounded-lg bg-purple-100 p-2 transition-colors group-hover:bg-purple-200">
                  <Settings className="h-4 w-4 text-purple-600" />
                </div>
                <span>{t('ExamManagement.Settings')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3">
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-600">{t('ExamManagement.ShowResults')}:</span>
                  <div className="flex items-center space-x-2">
                    {examDetail?.isShowResult ? (
                      <CheckCircle className="h-4 w-4 text-green-600" />
                    ) : (
                      <XCircle className="h-4 w-4 text-red-600" />
                    )}
                    <span
                      className={`rounded px-2 py-1 text-xs font-medium ${
                        examDetail?.isShowResult
                          ? 'bg-green-100 text-green-800'
                          : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {examDetail?.isShowResult ? 'Open' : 'Closed'}
                    </span>
                  </div>
                </div>
                <Separator className="opacity-50" />
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-600">
                    {t('ExamManagement.ShowCorrectAnswers')}:
                  </span>
                  <div className="flex items-center space-x-2">
                    {examDetail?.isShowCorrectAnswer ? (
                      <CheckCircle className="h-4 w-4 text-green-600" />
                    ) : (
                      <XCircle className="h-4 w-4 text-red-600" />
                    )}
                    <span
                      className={`rounded px-2 py-1 text-xs font-medium ${
                        examDetail?.isShowCorrectAnswer
                          ? 'bg-green-100 text-green-800'
                          : 'bg-red-100 text-red-800'
                      }`}
                    >
                      {examDetail?.isShowCorrectAnswer ? 'Open' : 'Closed'}
                    </span>
                  </div>
                </div>
                <Separator className="opacity-50" />
                <div className="flex items-center justify-between py-2">
                  <span className="text-sm text-gray-600">{t('ExamManagement.AllowRetake')}:</span>
                  <div className="flex items-center space-x-2">
                    <XCircle className="h-4 w-4 text-red-600" />
                    <span className="rounded bg-red-100 px-2 py-1 text-xs font-medium text-red-800">
                      {t('Close')}
                    </span>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="mt-8 flex items-center justify-between border-t pt-6">
          <div className="text-sm text-gray-500">
            {t('ExamManagement.LastUpdated')}: {moment().format('DD/MM/YYYY HH:mm')}
          </div>
          <div className="flex space-x-3">
            <Button
              onClick={toggle}
              className="bg-blue-600 px-6 transition-colors hover:bg-blue-700"
            >
              {t('Close')}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default React.memo(DialogExamDetail);
