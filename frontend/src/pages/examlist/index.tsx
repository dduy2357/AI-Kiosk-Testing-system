import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { StudentExamStatus } from '@/consts/common';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime } from '@/helpers/common';
import { showSuccess } from '@/helpers/toast';
import useToggleDialog from '@/hooks/useToggleDialog';
import httpService from '@/services/httpService';
import useGetListExamStudent from '@/services/modules/studentexam/hooks/useGetListExamStudent';
import type { StudentExamList } from '@/services/modules/studentexam/interfaces/studentexam.interface';
import studentexamService from '@/services/modules/studentexam/studentexam.service';
import {
  AlertCircle,
  BookOpen,
  Calendar,
  CheckCircle,
  Clock,
  Eye,
  FileText,
  Filter,
  MoreHorizontal,
  PlayCircle,
  Search,
  Timer,
  Trophy,
  XCircle,
} from 'lucide-react';
import moment from 'moment';
import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import DialogExamDetail from '../teacher/manageexam/dialogs/DialogExamDetail';
import DialogCameraCheck from './dialogs/DialogCameraCheck';
import DialogConfirmOTP, { type ConfirmOTPFormValues } from './dialogs/DialogConfirmIOTP';
import DialogShowGuideline from './dialogs/DialogShowGuideline';

declare global {
  interface Window {
    chrome?: {
      webview?: {
        postMessage(message: any): void;
      };
    };
  }
}
const ExamList = () => {
  const { t } = useTranslation('shared');
  const [selectedExam, setSelectedExam] = useState<StudentExamList | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const navigate = useNavigate();
  const [examId, setExamId] = useState<string | null>(null);

  const { data: examList, loading: isLoading } = useGetListExamStudent({
    isTrigger: true,
    refetchKey: cachedKeys.refetchExamStudent,
    saveData: false,
  });

  // Filter exams based on search term
  const filteredExams = useMemo(() => {
    if (!examList) return [];
    if (!searchTerm) return examList;

    return examList.filter((exam: any) =>
      exam.title.toLowerCase().includes(searchTerm.toLowerCase()),
    );
  }, [examList, searchTerm]);

  const getStatusBadge = (status: string) => {
    const statusConfig = {
      NotStarted: {
        color: 'bg-blue-100 text-blue-800 border-blue-200',
        icon: <Clock className="h-3 w-3" />,
        label: 'Not Started',
      },
      InProgress: {
        color: 'bg-orange-100 text-orange-800 border-orange-200',
        icon: <PlayCircle className="h-3 w-3" />,
        label: 'In Progress',
      },
      Submitted: {
        color: 'bg-purple-100 text-purple-800 border-purple-200',
        icon: <CheckCircle className="h-3 w-3" />,
        label: 'Submitted',
      },
      Failed: {
        color: 'bg-red-100 text-red-800 border-red-200',
        icon: <XCircle className="h-3 w-3" />,
        label: 'Failed',
      },
      Passed: {
        color: 'bg-green-100 text-green-800 border-green-200',
        icon: <Trophy className="h-3 w-3" />,
        label: 'Passed',
      },
    };

    const config = statusConfig[status as keyof typeof statusConfig];

    return (
      <Badge className={`${config.color} flex items-center gap-1 border font-medium`}>
        {config.icon}
        {config.label}
      </Badge>
    );
  };

  const [openAskConfirmOTP, toggleAskConfirmOTP, shouldRenderAskConfirmOTP] = useToggleDialog();
  const [openCameraCheck, toggleCameraCheck, shouldRenderCameraCheck] = useToggleDialog();
  const [openShowGuideline, toggleShowGuideline, shouldRenderShowGuideline] = useToggleDialog();
  const [openDialogViewExamDetail, toggleDialogViewExamDetail, shouldRenderDialogViewExamDetail] =
    useToggleDialog();

  const handleToggleDialogViewExamDetail = () => {
    toggleDialogViewExamDetail();
    if (examId) {
      setExamId(null);
    }
  };

  const handleExamClick = () => {
    toggleAskConfirmOTP();
  };

  const columns = [
    {
      label: t('ExamList.ExamTitle'),
      accessor: 'title',
      sortable: false,
      Cell: (row: StudentExamList) =>
        row?.status === StudentExamStatus.InProgress ||
        row.status === StudentExamStatus.NotStarted ? (
          <Button
            variant="destructive"
            className="font-semibold transition-colors hover:bg-red-600"
            onClick={() => {
              setSelectedExam(row);
              toggleShowGuideline();
            }}
          >
            <PlayCircle className="mr-2 h-4 w-4" />
            {row?.title}
          </Button>
        ) : (
          <div className="font-medium text-gray-900">{row?.title}</div>
        ),
    },
    {
      label: t('ExamList.StartTime'),
      accessor: 'startTime',
      sortable: false,
      Cell: (row: StudentExamList) => (
        <div className="flex items-center gap-2 text-gray-600">
          <Calendar className="h-4 w-4" />
          {convertUTCToVietnamTime(row?.startTime, DateTimeFormat.DayMonthYear)?.toString() ||
            'N/A'}
        </div>
      ),
    },
    {
      label: t('ExamList.EndTime'),
      accessor: 'endTime',
      sortable: false,
      Cell: (row: StudentExamList) => (
        <div className="flex items-center gap-2 text-gray-600">
          <Clock className="h-4 w-4" />
          {convertUTCToVietnamTime(row?.endTime, DateTimeFormat.DayMonthYear)?.toString() || 'N/A'}
        </div>
      ),
    },
    {
      label: t('ExamList.Duration'),
      accessor: 'duration',
      sortable: false,
      Cell: (row: StudentExamList) => (
        <div className="flex items-center gap-2 text-gray-600">
          <Timer className="h-4 w-4" />
          {`${row?.duration} minutes`}
        </div>
      ),
    },
    {
      label: t('ExamList.Status'),
      accessor: 'status',
      sortable: false,
      Cell: (row: StudentExamList) => getStatusBadge(row?.status),
    },
    {
      label: t('ExamList.Actions'),
      accessor: 'actions',
      width: 80,
      sortable: false,
      Cell: (row: StudentExamList) => (
        <div className="flex items-center justify-center">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="h-7 w-7 rounded p-0 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
              >
                <MoreHorizontal className="h-3.5 w-3.5 text-gray-600 dark:text-gray-400" />
                <span className="sr-only">{t('ExamList.Actions')}</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-40">
              <DropdownMenuItem
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                onClick={() => {
                  setExamId(row.examId);
                  toggleDialogViewExamDetail();
                }}
              >
                <Eye className="mr-2 h-3.5 w-3.5 text-blue-500" />
                <span>{t('ExamList.ViewDetail')}</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ];

  const handleConfirmOTP = async (values: ConfirmOTPFormValues) => {
    const response = await studentexamService.accessExam(
      selectedExam?.examId || '',
      Number(values.otpCode),
    );
    if (response) {
      httpService.saveStudentIdStorage(response.data.data.studentExamId);
    } else {
      throw new Error('Invalid OTP');
    }
    await new Promise((resolve) => setTimeout(resolve, 1000));
    toggleAskConfirmOTP();
    showSuccess('Exam access granted successfully!');
    if (selectedExam?.verifyCamera) {
      toggleCameraCheck();
    } else {
      navigate(`${BaseUrl.ExamList}/${selectedExam?.examId}`);
      setSelectedExam(null);
    }
  };

  const handleCameraSuccess = () => {
    navigate(`${BaseUrl.ExamList}/${selectedExam?.examId}`);
    setSelectedExam(null);
  };

  return (
    <PageWrapper name="Exam List" className="bg-white dark:bg-gray-900">
      <div className="bg-gradient-to-br from-blue-50 via-white to-indigo-50">
        <div className="component:ExamList mx-auto max-w-7xl space-y-8 p-6">
          {/* Header Section */}
          <div className="relative">
            <div className="absolute inset-0 rounded-2xl bg-gradient-to-r from-blue-600 to-indigo-600 opacity-10"></div>
            <Card className="relative border-0 bg-white/80 shadow-xl backdrop-blur-sm">
              <CardContent className="p-8">
                <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
                  <div className="space-y-2">
                    <h1 className="bg-gradient-to-r from-blue-600 to-indigo-600 bg-clip-text text-4xl font-bold text-transparent">
                      {t('ExamList.Title')}
                    </h1>
                    <p className="text-lg text-gray-600">{t('ExamList.Description')}</p>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <div className="text-2xl font-bold text-gray-900">
                        {moment().format('HH:mm')}
                      </div>
                      <div className="text-sm text-gray-500">{moment().format('MMM DD, YYYY')}</div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Search and Filter Section */}
          <Card className="border-0 shadow-lg">
            <CardContent className="p-6">
              <div className="flex flex-col items-center justify-between gap-4 sm:flex-row">
                <div className="relative max-w-md flex-1">
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
                  <Input
                    placeholder="Search exams..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="border-gray-200 pl-10 focus:border-blue-500 focus:ring-blue-500"
                  />
                </div>
                <div className="flex gap-2">
                  <Button variant="outline" size="sm" className="border-gray-200 hover:bg-gray-50">
                    <Filter className="mr-2 h-4 w-4" />
                    Filter
                  </Button>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Action Buttons */}
          <Card className="border-0 shadow-lg">
            <CardContent className="p-6">
              <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
                <Button
                  variant="outline"
                  size="lg"
                  className="group h-16 border-2 border-blue-200 transition-all hover:border-blue-300 hover:bg-blue-50"
                >
                  <div className="flex items-center gap-3">
                    <div className="rounded-lg bg-blue-100 p-2 transition-colors group-hover:bg-blue-200">
                      <FileText className="h-5 w-5 text-blue-600" />
                    </div>
                    <div className="text-left">
                      <div className="font-semibold text-gray-900">{t('ExamList.Note')}</div>
                      <div className="text-sm text-gray-500">{t('ExamList.Guidelines')}</div>
                    </div>
                  </div>
                </Button>

                <Button
                  variant="outline"
                  size="lg"
                  className="group h-16 border-2 border-green-200 transition-all hover:border-green-300 hover:bg-green-50"
                >
                  <div className="flex items-center gap-3">
                    <div className="rounded-lg bg-green-100 p-2 transition-colors group-hover:bg-green-200">
                      <Eye className="h-5 w-5 text-green-600" />
                    </div>
                    <div className="text-left">
                      <div className="font-semibold text-gray-900">{t('ExamList.AccessExam')}</div>
                      <div className="text-sm text-gray-500">
                        {t('ExamList.StartYourExamination')}
                      </div>
                    </div>
                  </div>
                </Button>

                <Button
                  variant="outline"
                  size="lg"
                  className="group h-16 border-2 border-purple-200 transition-all hover:border-purple-300 hover:bg-purple-50"
                >
                  <div className="flex items-center gap-3">
                    <div className="rounded-lg bg-purple-100 p-2 transition-colors group-hover:bg-purple-200">
                      <Clock className="h-5 w-5 text-purple-600" />
                    </div>
                    <div className="text-left">
                      <div className="font-semibold text-gray-900">{t('ExamList.CurrentTime')}</div>
                      <div className="text-sm text-gray-500">{moment().format('HH:mm')}</div>
                    </div>
                  </div>
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Important Notice */}
          <Card className="border-0 border-l-4 border-l-amber-500 bg-amber-50 shadow-lg">
            <CardContent className="p-6">
              <div className="flex items-start gap-4">
                <AlertCircle className="mt-0.5 h-6 w-6 text-amber-600" />
                <div>
                  <h3 className="mb-2 font-semibold text-amber-900">Important Exam Instructions</h3>
                  <ul className="space-y-1 text-sm text-amber-800">
                    <li>• Ensure stable internet connection before starting</li>
                    <li>• Camera and microphone access will be required</li>
                    <li>• Do not refresh or close the browser during exam</li>
                    <li>• Contact support if you encounter technical issues</li>
                  </ul>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Exam Table */}
          <Card className="border-0 shadow-xl">
            <CardHeader className="pb-4">
              <CardTitle className="flex items-center gap-2 text-xl">
                <BookOpen className="h-5 w-5 text-blue-600" />
                Examination Schedule
              </CardTitle>
              <Separator />
            </CardHeader>
            <CardContent className="p-6">
              {shouldRenderAskConfirmOTP && (
                <DialogConfirmOTP
                  isOpen={openAskConfirmOTP}
                  onSubmit={handleConfirmOTP}
                  toggle={toggleAskConfirmOTP}
                  selectedExam={selectedExam || undefined}
                />
              )}

              {shouldRenderCameraCheck && (
                <DialogCameraCheck
                  isOpen={openCameraCheck}
                  toggle={toggleCameraCheck}
                  onCameraSuccess={handleCameraSuccess}
                  examTitle={selectedExam?.title}
                />
              )}

              {shouldRenderShowGuideline && (
                <DialogShowGuideline
                  isOpen={openShowGuideline}
                  toggle={toggleShowGuideline}
                  selectedExam={selectedExam || undefined}
                  onSubmit={() => {
                    toggleShowGuideline();
                    handleExamClick();
                  }}
                />
              )}

              <div className="overflow-hidden rounded-lg border border-gray-200">
                <MemoizedTablePaging columns={columns} data={filteredExams} loading={isLoading} />
              </div>
            </CardContent>
          </Card>
          {shouldRenderDialogViewExamDetail && (
            <DialogExamDetail
              isOpen={openDialogViewExamDetail}
              toggle={handleToggleDialogViewExamDetail}
              examId={examId || ''}
            />
          )}

          {/* Footer */}
          <div className="py-8 text-center">
            <p className="text-sm text-gray-500">
              Need help? Contact our support team for assistance with your examinations.
            </p>
          </div>
        </div>
      </div>
    </PageWrapper>
  );
};

export default ExamList;
