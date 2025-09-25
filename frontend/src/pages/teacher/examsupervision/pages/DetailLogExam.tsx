import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import {
  User,
  FileText,
  AlertTriangle,
  Clock,
  Monitor,
  RefreshCw,
  Eye,
  Settings,
  ZoomIn,
  ImageIcon,
} from 'lucide-react';
import { useParams } from 'react-router-dom';
import useGetDetailExamActivityLog from '@/services/modules/examactivitylog/hooks/useGetDetailExamActivityLog';
import { LogType } from '@/consts/common';
import { useTranslation } from 'react-i18next';

export default function ExamActivityLogDetail() {
  //!State
  const { t } = useTranslation('shared');
  const [isExpanded, setIsExpanded] = useState(false);
  const { logId } = useParams();
  const { data: examLogDetail } = useGetDetailExamActivityLog(logId);
  const [isScreenshotModalOpen, setIsScreenshotModalOpen] = useState(false);
  let parsedMetadata: any = {};

  // Chỉ thực hiện parsing khi examLogDetail tồn tại và metadata không phải undefined/null
  if (examLogDetail && examLogDetail.metadata != null) {
    try {
      if (!Array.isArray(examLogDetail.metadata)) {
        parsedMetadata = JSON.parse(examLogDetail.metadata);
      } else {
        parsedMetadata = examLogDetail.metadata.map((item) => {
          return typeof item === 'string' ? JSON.parse(item) : item;
        });
      }
    } catch (error) {
      console.error('Error parsing metadata:', error);

      try {
        if (!Array.isArray(examLogDetail.metadata)) {
          const cleanMetadata = examLogDetail.metadata.replace(/\\\\\\\\/g, '\\\\');
          parsedMetadata = JSON.parse(cleanMetadata);
        }
      } catch (secondError) {
        console.error('Second parsing attempt failed:', secondError);

        if (
          !Array.isArray(examLogDetail.metadata) &&
          typeof examLogDetail.metadata === 'string' &&
          examLogDetail.metadata.includes('ProcessName')
        ) {
          const processNameMatch = examLogDetail.metadata.match(/"ProcessName":"([^"]+)"/);
          const processIdMatch = examLogDetail.metadata.match(/"ProcessId":(\d+)/);
          const filePathMatch = examLogDetail.metadata.match(/"FilePath":"([^"]+)"/);

          if (processNameMatch && processIdMatch && filePathMatch) {
            parsedMetadata = {
              ProcessName: processNameMatch[1],
              ProcessId: Number.parseInt(processIdMatch[1]),
              FilePath: filePathMatch[1].replace(/\\\\/g, '\\'),
            };
          }
        }
      }
    }
  } else if (examLogDetail && examLogDetail.metadata === null) {
    parsedMetadata = {};
  }

  const getActionTypeBadge = (actionType: any, logType: any) => {
    switch (logType) {
      case LogType.Info:
        return <Badge className="border-blue-200 bg-blue-100 text-blue-800">{actionType}</Badge>;
      case LogType.Warning:
        return (
          <Badge className="border-yellow-200 bg-yellow-100 text-yellow-800">{actionType}</Badge>
        );
      case LogType.Violation:
        return <Badge className="border-red-200 bg-red-100 text-red-800">{actionType}</Badge>;
      case LogType.Critical:
        return (
          <Badge className="border-orange-200 bg-orange-100 text-orange-800">{actionType}</Badge>
        );
      default:
        return <Badge variant="outline">{actionType}</Badge>;
    }
  };

  //!Function
  const getTitleForLog = (logType: any) => {
    switch (logType) {
      case LogType.Info:
        return (
          <h3 className="text-lg font-semibold text-blue-900">
            {t('ExamSupervision.Information')}
          </h3>
        );
      case LogType.Warning:
        return (
          <h3 className="text-lg font-semibold text-yellow-900">{t('ExamSupervision.Warning')}</h3>
        );
      case LogType.Violation:
        return (
          <h3 className="text-lg font-semibold text-red-900">{t('ExamSupervision.Violation')}</h3>
        );
      case LogType.Critical:
        return (
          <h3 className="text-lg font-semibold text-orange-900">{t('ExamSupervision.Critical')}</h3>
        );
      default:
        return <h3>{t('ExamSupervision.Default')}</h3>;
    }
  };

  const getLogTypeBadge = (logType: number | undefined) => {
    switch (logType) {
      case LogType.Info:
        return (
          <Badge className="bg-blue-100 text-blue-800">{t('ExamSupervision.Information')}</Badge>
        );
      case LogType.Warning:
        return (
          <Badge className="bg-yellow-100 text-yellow-800">{t('ExamSupervision.Warning')}</Badge>
        );
      case LogType.Violation:
        return <Badge className="bg-red-100 text-red-800">{t('ExamSupervision.Violation')}</Badge>;
      case LogType.Critical:
        return <Badge className="bg-red-100 text-red-800">{t('ExamSupervision.Critical')}</Badge>;
      default:
        return <Badge variant="outline">{t('ExamSupervision.Default')}</Badge>;
    }
  };

  const getRiskLevel = (actionType: any, metadata: any) => {
    if (actionType === 'ProcessDetected') {
      const filePath = metadata.FilePath ?? '';
      if (filePath.includes('office') || filePath.includes('word') || filePath.includes('excel')) {
        return {
          level: t('ExamSupervision.High'),
          color: 'bg-red-100 text-red-800 border-red-200',
          description: t('ExamSupervision.HighRiskDescription'),
        };
      }
    }
    return {
      level: t('ExamSupervision.Low'),
      color: 'bg-green-100 text-green-800 border-green-200',
      description: t('ExamSupervision.LowRiskDescription'),
    };
  };

  const riskInfo = getRiskLevel(examLogDetail?.actionType, parsedMetadata);

  const formatDateTime = (dateString: any) => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / (1000 * 60));

    let timeAgo = '';
    if (diffMins < 1) timeAgo = t('ExamSupervision.JustNow');
    else if (diffMins < 60) timeAgo = `${diffMins} ${t('ExamSupervision.MinutesAgo')}`;
    else if (diffMins < 1440)
      timeAgo = `${Math.floor(diffMins / 60)} ${t('ExamSupervision.HoursAgo')}`;
    else timeAgo = `${Math.floor(diffMins / 1440)} ${t('ExamSupervision.DaysAgo')}`;

    return {
      date: date.toLocaleDateString('vi-VN'),
      time: date.toLocaleTimeString('vi-VN'),
      full: date.toLocaleString('vi-VN'),
      timeAgo: timeAgo,
    };
  };

  const dateTime = formatDateTime(examLogDetail?.createdAt);
  const hasViolation = examLogDetail?.logType === LogType.Violation || false;
  const handleScreenshotClick = () => {
    setIsScreenshotModalOpen(true);
  };
  const closeScreenshotModal = () => {
    setIsScreenshotModalOpen(false);
  };

  //!Render
  return (
    <div className="min-h-screen bg-gray-50">
      <div className="border-b border-gray-200 bg-white p-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">
                {t('ExamSupervision.LogDetailTitle')}
              </h1>
              <p className="mt-1 text-gray-600">ID: {examLogDetail?.examLogId}</p>
            </div>
            {getLogTypeBadge(examLogDetail?.logType)}
          </div>
          <div className="flex items-center space-x-3">
            <Button variant="outline" size="sm" onClick={() => window.location.reload()}>
              <RefreshCw className="mr-2 h-4 w-4" />
              {t('ExamSupervision.Refresh')}
            </Button>
          </div>
        </div>
      </div>

      <div className="space-y-6 p-6">
        <Card className={`border-l-4 ${hasViolation ? 'border-l-red-500 bg-red-50' : null}`}>
          <CardContent className="p-6">
            <div className="flex items-start space-x-4">
              <div
                className={`flex h-12 w-12 items-center justify-center rounded-full ${
                  hasViolation ? 'bg-red-100' : 'bg-gray-100'
                }`}
              >
                {hasViolation ? <AlertTriangle className={`text-red-600} h-6 w-6`} /> : null}
              </div>
              <div className="flex-1">
                <div className="mb-2 flex items-center space-x-3">
                  {getTitleForLog(examLogDetail?.logType)}
                  {getActionTypeBadge(examLogDetail?.actionType, examLogDetail?.logType)}
                  <Badge className={riskInfo.color}>{riskInfo.level}</Badge>
                </div>
                <p className={`mb-2 ${hasViolation ? 'text-red-800' : null}`}>
                  {examLogDetail?.description}
                </p>
                <p className={`text-sm ${hasViolation ? 'text-red-700' : null}`}>
                  {riskInfo.description}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="grid grid-cols-3 gap-6">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <User className="h-5 w-5" />
                <span>{t('ExamSupervision.StudentInformation')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="flex items-center space-x-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-100">
                  <User className="h-5 w-5 text-blue-600" />
                </div>
                <div>
                  <p className="font-medium text-gray-900">{examLogDetail?.fullName}</p>
                  <p className="text-sm text-gray-500">{t('ExamSupervision.Student')}</p>
                </div>
                <div>
                  <br />
                  <p className="text-sm text-gray-500">
                    {t('ExamSupervision.StudentCode')}: {examLogDetail?.userCode}
                  </p>
                </div>
              </div>

              <Separator />

              <div className="space-y-3 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.BrowserInfo')}:</span>
                  <span className="font-medium">{examLogDetail?.browserInfo}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.IP')}:</span>
                  <span className="font-medium">{examLogDetail?.ipAddress}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.Device')}:</span>
                  <span className="font-medium">{examLogDetail?.deviceUsername}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.DeviceID')}:</span>
                  <span className="font-medium">{examLogDetail?.deviceId}</span>
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <FileText className="h-5 w-5" />
                <span>{t('ExamSupervision.LogDetails')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3 text-sm">
                <div>
                  <span className="mb-1 block text-gray-500">
                    {t('ExamSupervision.ActionType')}:
                  </span>
                  {getActionTypeBadge(examLogDetail?.actionType, examLogDetail?.logType)}
                </div>
                <div>
                  <span className="mb-1 block text-gray-500">
                    {t('ExamSupervision.Description')}:
                  </span>
                  <p className="text-gray-900">{examLogDetail?.description}</p>
                </div>
                <div>
                  <span className="mb-1 block text-gray-500">{t('ExamSupervision.Severity')}:</span>
                  {getLogTypeBadge(examLogDetail?.logType)}
                </div>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <Clock className="h-5 w-5" />
                <span>{t('ExamSupervision.Timestamp')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-3 text-sm">
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.Date')}:</span>
                  <span className="font-medium">{dateTime.date}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.Time')}:</span>
                  <span className="font-medium">{dateTime.time}</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-gray-500">{t('ExamSupervision.TimeAgo')}:</span>
                  <span className="font-medium text-orange-600">{dateTime.timeAgo}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {examLogDetail?.screenshotPath && (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <ImageIcon className="h-5 w-5" />
                <span>{t('ExamSupervision.ScreenshotEvidence')}</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="flex items-center justify-between">
                  <p className="text-sm text-gray-600">
                    {t('ExamSupervision.ScreenshotAutoCapture')}
                  </p>
                  <Button variant="outline" size="sm" onClick={handleScreenshotClick}>
                    <ZoomIn className="mr-2 h-4 w-4" />
                    {t('ExamSupervision.FullScreenView')}
                  </Button>
                </div>

                <div className="relative">
                  <button
                    type="button"
                    onClick={handleScreenshotClick}
                    className="mx-auto block w-full max-w-2xl rounded-lg border border-gray-200 shadow-sm transition-shadow hover:shadow-md focus:outline-none"
                  >
                    <img
                      src={examLogDetail?.screenshotPath}
                      alt="Bằng chứng giám sát (ảnh chụp màn hình)"
                      className="w-full rounded-lg object-contain"
                      crossOrigin="anonymous"
                    />
                  </button>

                  <div className="absolute right-2 top-2">
                    <Badge className="border-red-200 bg-red-100 text-red-800">
                      {t('ExamSupervision.ScreenshotEvidence')}
                    </Badge>
                  </div>
                </div>

                <div className="text-center text-xs text-gray-500">
                  {t('ExamSupervision.ClickToView')} • {t('ExamSupervision.CaptureTime')}:{' '}
                  {dateTime.full}
                </div>
              </div>
            </CardContent>
          </Card>
        )}

        {examLogDetail ? (
          Array.isArray(parsedMetadata) ? (
            // Trường hợp parsedMetadata là mảng
            parsedMetadata.map(
              (item, index) =>
                item.ProcessName && (
                  <Card key={index}>
                    <CardHeader>
                      <CardTitle className="flex items-center space-x-2">
                        <Monitor className="h-5 w-5" />
                        <span>
                          {t('ExamSupervision.ProcessInfo')} #{index + 1}
                        </span>
                      </CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="grid grid-cols-2 gap-6">
                        <div className="space-y-4">
                          <div>
                            <span className="mb-1 block text-sm text-gray-500">
                              {t('ExamSupervision.ProcessName')}:
                            </span>
                            <div className="flex items-center space-x-2">
                              <p className="font-medium text-gray-900">{item.ProcessName}</p>
                              <Badge variant="outline">PID: {item.ProcessId}</Badge>
                            </div>
                          </div>
                        </div>
                        <div>
                          <span className="mb-1 block text-sm text-gray-500">
                            {t('ExamSupervision.FullPath')}:
                          </span>
                          <div className="rounded-lg bg-gray-100 p-3">
                            <p className="break-all font-mono text-xs text-gray-800">
                              {item.FilePath?.replace(/\\\\/g, '\\')}
                            </p>
                          </div>
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ),
            )
          ) : (
            // Trường hợp parsedMetadata là object đơn
            parsedMetadata.ProcessName && (
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center space-x-2">
                    <Monitor className="h-5 w-5" />
                    <span>{t('ExamSupervision.ProcessInfo')}</span>
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-2 gap-6">
                    <div className="space-y-4">
                      <div>
                        <span className="mb-1 block text-sm text-gray-500">
                          {t('ExamSupervision.ProcessName')}:
                        </span>
                        <div className="flex items-center space-x-2">
                          <p className="font-medium text-gray-900">{parsedMetadata.ProcessName}</p>
                          <Badge variant="outline">PID: {parsedMetadata.ProcessId}</Badge>
                        </div>
                      </div>
                    </div>
                    <div>
                      <span className="mb-1 block text-sm text-gray-500">
                        {t('ExamSupervision.FullPath')}:
                      </span>
                      <div className="rounded-lg bg-gray-100 p-3">
                        <p className="break-all font-mono text-xs text-gray-800">
                          {parsedMetadata.FilePath?.replace(/\\\\/g, '\\')}
                        </p>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            )
          )
        ) : (
          <p>Loading...</p>
        )}

        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <CardTitle className="flex items-center space-x-2">
                <Settings className="h-5 w-5" />
                <span>{t('ExamSupervision.Metadata')}</span>
              </CardTitle>
              <Button variant="outline" size="sm" onClick={() => setIsExpanded(!isExpanded)}>
                <Eye className="mr-2 h-4 w-4" />
                {isExpanded ? t('ExamSupervision.Collapse') : t('ExamSupervision.Expand')}
              </Button>
            </div>
          </CardHeader>
          {isExpanded && (
            <CardContent>
              <div className="space-y-4">
                <div>
                  <span className="mb-2 block text-sm font-medium text-gray-700">
                    {t('ExamSupervision.MetadataDetails')}:
                  </span>
                  <pre className="overflow-x-auto rounded-lg bg-gray-100 p-4 text-xs">
                    {Array.isArray(parsedMetadata)
                      ? JSON.stringify(parsedMetadata, null, 2)
                      : JSON.stringify([parsedMetadata], null, 2)}
                  </pre>
                </div>
                <div>
                  <span className="mb-2 block text-sm font-medium text-gray-700">
                    {t('ExamSupervision.LogEntry')}:
                  </span>
                  <pre className="overflow-x-auto rounded-lg bg-gray-100 p-4 text-xs">
                    {JSON.stringify(examLogDetail, null, 2)}
                  </pre>
                </div>
              </div>
            </CardContent>
          )}
        </Card>

        {/* Actions */}
        {/* <div className="flex justify-center space-x-4">
          <Button variant="outline" className="bg-transparent px-8">
            {t('ExamSupervision.MarkAsRead')}
          </Button>
          <Button className="bg-red-600 px-8 hover:bg-red-700">
            {t('ExamSupervision.ReportViolation')}
          </Button>
          <Button variant="outline" className="bg-transparent px-8">
            {t('ExamSupervision.ContactCandidate')}
          </Button>
        </div> */}
      </div>
      {isScreenshotModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-75 p-4">
          <div className="relative max-h-full max-w-6xl">
            <Button
              variant="ghost"
              size="sm"
              className="absolute -top-12 right-0 text-white hover:text-gray-300"
              onClick={closeScreenshotModal}
            >
              <span className="text-lg">✕</span> {t('Close')}
            </Button>
            <img
              src={examLogDetail?.screenshotPath}
              alt="Screenshot evidence - Full size"
              className="max-h-full max-w-full rounded-lg object-contain"
              crossOrigin="anonymous"
            />
            <div className="absolute bottom-0 left-0 right-0 rounded-b-lg bg-black bg-opacity-50 p-4 text-white">
              <p className="text-sm">
                <strong>Screenshot:</strong> {examLogDetail?.description}
              </p>
              <p className="mt-1 text-xs text-gray-300">
                {t('ExamSupervision.Time')}: {dateTime.full} • ID: {examLogDetail?.examLogId}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
