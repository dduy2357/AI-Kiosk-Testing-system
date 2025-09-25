import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Progress } from '@/components/ui/progress';
import BaseUrl from '@/consts/baseUrl';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime, formatScore } from '@/helpers/common';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import {
  GenericFilters,
  IValueFormPageHeader,
} from '@/pages/admin/manageuser/components/generic-filters';
import { UserStats } from '@/pages/admin/manageuser/components/user-stats';
import httpService from '@/services/httpService';
import useGetListMonitorDetail from '@/services/modules/monitor/hooks/useGetDetailMonitor';
import { Student } from '@/services/modules/monitor/interfaces/monitorDetail.interface';
import monitorService from '@/services/modules/monitor/monitor.service';
import {
  Activity,
  AlertTriangle,
  Ban,
  BarChart,
  BookOpen,
  Calendar,
  CheckCircle,
  Clock,
  Eye,
  Globe,
  Mail,
  MessageSquare,
  Monitor,
  MoreHorizontal,
  Pause,
  Plus,
  Shield,
  ShieldAlert,
  TrendingUp,
  Trophy,
  Users,
  XCircle,
} from 'lucide-react';
import { useCallback, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ExamHeader from '../components/ExamHeader';
import DialogAssignMoreTime, { AssignMoreTimeValues } from '../dialogs/DialogAssignMoreTime';
import DialogCreateViolation from '../dialogs/DialogCreateViolation';
import { useTranslation } from 'react-i18next';
import DialogStatistic from '../dialogs/DialogStatistic';

const DetailExamSupervision = () => {
  //!State
  const { t } = useTranslation('shared');
  const roleId = Number(httpService.getUserStorage()?.roleId) ?? 0;
  const navigate = useNavigate();
  const { examId } = useParams();
  const [isOpenAssignMoreTime, toggleOpenAssignMoreTime, shouldRenderOpenAssignMoreTime] =
    useToggleDialog();
  const [isOpenCreateViolation, toggleOpenCreateViolation, shouldRenderOpenCreateViolation] =
    useToggleDialog();
  const [row, setRow] = useState<Student | null>(null);
  const handleToggleAssignMoreTime = useCallback(() => {
    toggleOpenAssignMoreTime();
  }, [toggleOpenAssignMoreTime]);
  const [isReAssignLoading, setIsReAssignLoading] = useState(false);
  const [isOpenViewStatistic, toggleOpenViewStatistic, shouldRenderViewStatistic] =
    useToggleDialog();
  const [studentExamId, setStudentExamId] = useState<string | null>(null);

  const { filters, setFilters } = useFiltersHandler({
    ExamId: examId ?? '',
    PageSize: 10,
    CurrentPage: 1,
    TextSearch: '',
    StudentExamStatus: undefined,
  });

  const {
    data: dataMonitorDetail,
    isLoading: isLoadingMonitorDetail,
    refetch,
  } = useGetListMonitorDetail(filters, examId ?? '', {
    isTrigger: true,
  });

  const dataStudents = useMemo(() => {
    return dataMonitorDetail?.students ?? [];
  }, [dataMonitorDetail]);

  const statItems = useMemo(() => {
    const totalConnections = dataStudents?.length ?? 0;
    const activeConnections =
      dataStudents?.filter((item: any) => item.studentExamStatus === 1).length ?? 0;
    const studentsParticipating =
      dataStudents?.filter((item: any) => item.studentExamStatus === 1).length ?? 0;
    const completedStudents =
      dataStudents?.filter((item: any) => item.studentExamStatus === 2).length ?? 0;
    const notStartedStudents =
      dataStudents?.filter((item: any) => item.studentExamStatus === 0).length ?? 0;

    // Advanced calculations
    const totalWarnings =
      dataStudents?.reduce((sum: number, student: any) => sum + (student.warningCount ?? 0), 0) ??
      0;
    const totalViolations =
      dataStudents?.reduce((sum: number, student: any) => sum + (student.violationCount ?? 0), 0) ??
      0;
    const studentsWithScores = dataStudents?.filter((item: any) => item.score !== null) ?? [];
    const averageScore =
      studentsWithScores.length > 0
        ? studentsWithScores.reduce((sum: number, student: any) => sum + student.score, 0) /
          studentsWithScores.length
        : 0;

    const totalQuestions = dataStudents?.[0]?.totalQuestions ?? 0;
    const totalAnsweredQuestions =
      dataStudents?.reduce(
        (sum: number, student: any) => sum + (student.answeredQuestions ?? 0),
        0,
      ) ?? 0;
    const averageProgress =
      totalConnections > 0 && totalQuestions > 0
        ? (totalAnsweredQuestions / (totalConnections * totalQuestions)) * 100
        : 0;

    const capacityUsage =
      dataMonitorDetail && dataMonitorDetail.maxCapacity > 0
        ? (totalConnections / dataMonitorDetail.maxCapacity) * 100
        : 0;

    return [
      {
        title: t('ExamSupervision.TotalStudents'),
        value: totalConnections,
        subtitle: `/${dataMonitorDetail?.maxCapacity} sức chứa`,
        icon: <Users className="h-6 w-6 text-emerald-600" />,
        bgColor: 'bg-gradient-to-br from-emerald-50 to-emerald-100',
        progress: capacityUsage,
        trend:
          dataMonitorDetail?.maxCapacity !== undefined &&
          totalConnections > dataMonitorDetail.maxCapacity * 0.8
            ? 'high'
            : 'normal',
      },
      {
        title: t('ExamSupervision.OnGoing'),
        value: activeConnections,
        subtitle: `${studentsParticipating} ${t('ExamSupervision.StudentsParticipating')}`,
        icon: <Activity className="h-6 w-6 text-blue-600" />,
        bgColor: 'bg-gradient-to-br from-blue-50 to-blue-100',
        progress: totalConnections > 0 ? (activeConnections / totalConnections) * 100 : 0,
        trend: 'normal',
      },
      {
        title: t('ExamSupervision.Completed'),
        value: completedStudents,
        subtitle: `${Math.round((completedStudents / totalConnections) * 100)}% ${t('ExamSupervision.CompletedSubtitle')}`,
        icon: <CheckCircle className="h-6 w-6 text-green-600" />,
        bgColor: 'bg-gradient-to-br from-green-50 to-green-100',
        progress: totalConnections > 0 ? (completedStudents / totalConnections) * 100 : 0,
        trend: 'positive',
      },
      {
        title: t('ExamSupervision.NoNotStartedExams'),
        value: notStartedStudents,
        subtitle: `${Math.round((notStartedStudents / totalConnections) * 100)}% ${t('ExamSupervision.NoNotStartedExams')}`,
        icon: <Clock className="h-6 w-6 text-orange-600" />,
        bgColor: 'bg-gradient-to-br from-orange-50 to-orange-100',
        progress: totalConnections > 0 ? (notStartedStudents / totalConnections) * 100 : 0,
        trend: notStartedStudents > totalConnections * 0.3 ? 'warning' : 'normal',
      },
      {
        title: t('ExamSupervision.AverageScore'),
        value: averageScore.toFixed(1),
        subtitle: `${studentsWithScores.length} ${t('ExamSupervision.ScoresGraded')}`,
        icon: <Trophy className="h-6 w-6 text-yellow-600" />,
        bgColor: 'bg-gradient-to-br from-yellow-50 to-yellow-100',
        progress: (averageScore / 10) * 100,
        trend: averageScore >= 7 ? 'positive' : averageScore >= 5 ? 'normal' : 'warning',
      },
      {
        title: t('ExamSupervision.AverageProgress'),
        value: `${Math.round(averageProgress) ?? 0}%`,
        subtitle: `${totalAnsweredQuestions}/${totalConnections * totalQuestions} ${t('ExamSupervision.Questions')}`,
        icon: <TrendingUp className="h-6 w-6 text-purple-600" />,
        bgColor: 'bg-gradient-to-br from-purple-50 to-purple-100',
        progress: averageProgress,
        trend: averageProgress >= 70 ? 'positive' : 'normal',
      },
      {
        title: t('ExamSupervision.Alerts'),
        value: totalWarnings,
        subtitle: `${dataStudents?.filter((s: any) => s.warningCount > 0).length} ${t('ExamSupervision.Students')} ${t('ExamSupervision.AlertsSubtitle')}`,
        icon: <AlertTriangle className="h-6 w-6 text-amber-600" />,
        bgColor: 'bg-gradient-to-br from-amber-50 to-amber-100',
        progress: totalConnections > 0 ? (totalWarnings / (totalConnections * 3)) * 100 : 0,
        trend: totalWarnings > totalConnections * 0.5 ? 'warning' : 'normal',
      },
      {
        title: t('ExamSupervision.Violations'),
        value: totalViolations,
        subtitle: `${dataStudents?.filter((s: any) => s.violinCount > 0).length} ${t('ExamSupervision.Students')}`,
        icon: <Shield className="h-6 w-6 text-red-600" />,
        bgColor: 'bg-gradient-to-br from-red-50 to-red-100',
        progress: totalConnections > 0 ? (totalViolations / totalConnections) * 100 : 0,
        trend: totalViolations > 0 ? 'danger' : 'positive',
      },
    ];
  }, [dataStudents, dataMonitorDetail, t]);

  const columns = [
    {
      label: t('ExamSupervision.StudentInfo'),
      accessor: 'userInfo',
      sortable: false,
      Cell: (row: Student) => (
        <div className="flex flex-col gap-2">
          <div className="flex items-center gap-2">
            <div className="rounded-full bg-blue-100 p-1.5 dark:bg-blue-900">
              <BookOpen className="h-4 w-4 text-blue-600 dark:text-blue-400" />
            </div>
            <span
              className="max-w-[200px] truncate text-sm font-semibold text-gray-900 dark:text-gray-100"
              title={row.fullName}
            >
              {row.fullName ?? 'N/A'}
            </span>
          </div>
          <div className="flex items-center gap-2">
            <ShieldAlert className="h-4 w-4 text-gray-500 dark:text-gray-400" color="red" />
            <p
              className="truncate text-sm font-medium text-gray-800 dark:text-gray-200"
              title={row.userCode}
            >
              {row.userCode ?? 'N/A'}
            </p>
          </div>
        </div>
      ),
    },
    {
      label: t('ExamSupervision.Email'),
      accessor: 'email',
      sortable: false,
      Cell: (row: Student) => (
        <div className="flex max-w-[200px] items-center gap-2">
          <Mail className="h-4 w-4 text-gray-500 dark:text-gray-400" color="blue" />
          <span
            className="truncate text-sm font-medium text-gray-800 dark:text-gray-200"
            title={row.email}
          >
            {row.email}
          </span>
        </div>
      ),
    },
    {
      label: t('ExamSupervision.ConnectionInfo'),
      accessor: 'connectionInfo',
      sortable: false,
      Cell: (row: Student) => (
        <div className="flex flex-col gap-2">
          <div className="flex items-center gap-2">
            <Globe className="h-4 w-4 text-gray-500 dark:text-gray-400" color="green" />
            <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
              {row.ipAddress ?? 'N/A'}
            </span>
          </div>
          <div className="flex max-w-[150px] items-center gap-2">
            <Monitor className="h-4 w-4 text-gray-500 dark:text-gray-400" />
            <span
              className="truncate text-xs text-gray-600 dark:text-gray-400"
              title={row.browserInfo}
            >
              {row.browserInfo ?? 'N/A'}
            </span>
          </div>
        </div>
      ),
    },
    {
      label: t('ExamSupervision.Time'),
      accessor: 'startTime',
      sortable: false,
      Cell: (row: Student) => (
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <Calendar className="h-4 w-4 text-green-500 dark:text-green-400" />
            <span className="text-xs text-gray-700 dark:text-gray-300">
              {row.startTime
                ? convertUTCToVietnamTime(row.startTime, DateTimeFormat.DateTime)?.toString()
                : 'N/A'}
            </span>
          </div>
          {row.submitTime ? (
            <div className="flex items-center gap-2">
              <CheckCircle className="h-4 w-4 text-blue-500 dark:text-blue-400" />
              <span className="text-xs text-gray-700 dark:text-gray-300">
                {convertUTCToVietnamTime(row.submitTime, DateTimeFormat.DateTime)?.toString()}
              </span>
            </div>
          ) : (
            <div className="flex items-center gap-2">
              <Clock className="h-4 w-4 text-orange-500 dark:text-orange-400" />
              <span className="text-xs text-orange-600 dark:text-orange-500">Chưa nộp</span>
            </div>
          )}
        </div>
      ),
    },
    {
      label: t('ExamSupervision.Status'),
      accessor: 'studentExamStatus',
      sortable: false,
      Cell: (row: Student) => {
        const statusConfig = {
          0: {
            label: t('ExamSupervision.NotStarted'),
            variant: 'secondary' as const,
            icon: Clock,
            color: 'text-gray-600 dark:text-gray-400',
          },
          1: {
            label: t('ExamSupervision.InProgress'),
            variant: 'default' as const,
            icon: MessageSquare,
            color: 'text-blue-600 dark:text-blue-400',
          },
          2: {
            label: t('ExamSupervision.Submitted'),
            variant: 'default' as const,
            icon: CheckCircle,
            color: 'text-green-600 dark:text-green-400',
          },
        };
        const config = statusConfig[row.studentExamStatus as keyof typeof statusConfig] ?? {
          label: 'Không rõ',
          variant: 'destructive' as const,
          icon: XCircle,
          color: 'text-red-600 dark:text-red-400',
        };
        const IconComponent = config.icon;
        return (
          <Badge variant={config.variant} className="flex items-center gap-1.5 text-xs">
            <IconComponent className={`h-3.5 w-3.5 ${config.color}`} />
            {config.label}
          </Badge>
        );
      },
    },
    {
      label: t('ExamSupervision.Progress'),
      accessor: 'totalQuestions',
      sortable: false,
      Cell: (row: Student) => {
        const progress =
          row.totalQuestions > 0 ? (row.answeredQuestions / row.totalQuestions) * 100 : 0;
        return (
          <div className="min-w-[140px] space-y-2">
            <div className="flex justify-between text-xs text-gray-600 dark:text-gray-400">
              <span>
                {row.answeredQuestions}/{row.totalQuestions}
              </span>
              <span>{Math.round(progress)}%</span>
            </div>
            <Progress value={progress} className="h-2 bg-gray-200 dark:bg-gray-700" />
          </div>
        );
      },
    },
    {
      label: t('ExamSupervision.Score'),
      accessor: 'score',
      sortable: false,
      Cell: (row: Student) => (
        <div className="flex items-center gap-2">
          <Trophy className="h-4 w-4 text-yellow-500 dark:text-yellow-400" />
          <span
            className={`text-sm font-semibold ${
              row.score !== null && row.score >= 8
                ? 'text-green-600 dark:text-green-400'
                : row.score !== null && row.score >= 5
                  ? 'text-yellow-600 dark:text-yellow-400'
                  : row.score !== null
                    ? 'text-red-600 dark:text-red-400'
                    : 'text-gray-400 dark:text-gray-500'
            }`}
          >
            {row.score !== null ? `${formatScore(row.score)}` : 'Chưa có'}
          </span>
        </div>
      ),
    },
    {
      label: t('ExamSupervision.WarningAndViolation'),
      accessor: 'warningCount',
      sortable: false,
      Cell: (row: Student) => (
        <div className="space-y-2">
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-4 w-4 text-yellow-500 dark:text-yellow-400" />
            <Badge
              variant="outline"
              className="border-yellow-300 text-yellow-700 dark:text-yellow-300"
            >
              {row.warningCount} cảnh báo
            </Badge>
          </div>
          <div className="flex items-center gap-2">
            <ShieldAlert className="h-4 w-4 text-red-500 dark:text-red-400" />
            <Badge
              variant="destructive"
              className="border-red-300 bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300"
            >
              {row.violinCount} vi phạm
            </Badge>
          </div>
        </div>
      ),
    },
    {
      label: t('ExamSupervision.Actions'),
      accessor: 'actions',
      width: 80,
      sortable: false,
      Cell: (row: Student) => (
        <div className="flex flex-col items-center justify-center">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="h-8 w-8 rounded-full p-0 hover:bg-gray-100 dark:hover:bg-gray-800"
              >
                <MoreHorizontal className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                <span className="sr-only">{t('ExamSupervision.OpenMenuActions')}</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent
              align="end"
              className="w-48 rounded-md bg-white shadow-lg dark:bg-gray-800"
            >
              <DropdownMenuItem
                className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                onClick={() => {
                  if (row) {
                    handleRowClick(row);
                  } else {
                    showError(t('ExamSupervision.CannotNavigate'));
                  }
                }}
              >
                <Eye className="h-4 w-4 text-blue-500 dark:text-blue-400" />
                {t('ExamSupervision.ViewDetails')}
              </DropdownMenuItem>
              <DropdownMenuItem
                className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                onClick={() => {
                  if (row?.studentExamId) {
                    navigate(
                      `${
                        roleId === 4
                          ? BaseUrl.AdminSupervision
                          : roleId === 3
                            ? BaseUrl.SupervisorExamSupervision
                            : BaseUrl.ExamSupervision
                      }/${examId}/examLog/${row?.studentExamId}`,
                    );
                  } else {
                    showError(t('ExamSupervision.CannotNavigate'));
                  }
                }}
              >
                <Activity className="h-4 w-4 text-blue-500 dark:text-blue-400" />
                {t('ExamSupervision.ActivityLog')}
              </DropdownMenuItem>
              <DropdownMenuItem
                className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                onClick={() => {
                  if (row?.studentExamId) {
                    navigate(
                      `${
                        roleId === 4
                          ? BaseUrl.AdminSupervision
                          : roleId === 3
                            ? BaseUrl.SupervisorExamSupervision
                            : BaseUrl.ExamSupervision
                      }/${examId}/violation/${row?.studentExamId}`,
                    );
                  } else {
                    showError(t('ExamSupervision.CannotNavigate'));
                  }
                }}
              >
                <Ban className="mr-2 h-3.5 w-3.5 text-blue-500" />
                <span>{t('ExamSupervision.ViolationHistory')}</span>
              </DropdownMenuItem>
              {row?.studentExamStatus === 1 && (
                <>
                  <DropdownMenuSeparator className="bg-gray-200 dark:bg-gray-700" />
                  <DropdownMenuItem
                    className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                    onClick={() => {
                      if (row.studentExamId) {
                        setRow(row);
                        toggleOpenAssignMoreTime();
                      } else {
                        showError(t('ExamSupervision.CannotNavigate'));
                      }
                    }}
                  >
                    <Plus className="h-4 w-4 text-blue-500 dark:text-blue-400" />
                    {t('ExamSupervision.GrantMoreTime')}
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                    onClick={async () => {
                      try {
                        setIsReAssignLoading(true);
                        await monitorService.finishStudentExam({
                          examId: examId ?? '',
                          studentExamId: row.studentExamId ?? '',
                        });
                        showSuccess(t('ExamSupervision.FinishExamSuccess'));
                        refetch();
                      } catch (error) {
                        showError(t('ExamSupervision.FinishExamError'));
                      } finally {
                        setIsReAssignLoading(false);
                      }
                    }}
                  >
                    <Pause className="h-4 w-4 text-red-500 dark:text-red-400" />
                    {t('ExamSupervision.FinishExam')}
                  </DropdownMenuItem>
                </>
              )}
              {row.studentExamStatus === 2 && (
                <>
                  <DropdownMenuItem
                    className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                    onClick={async () => {
                      try {
                        setIsReAssignLoading(true);
                        await monitorService.reAssignExam({
                          examId: examId ?? '',
                          studentId: row.userId,
                        });
                        showSuccess(t('ExamSupervision.ReAssignExamSuccess'));
                        refetch();
                      } catch (error) {
                        showError(t('ExamSupervision.ReAssignExamError'));
                      } finally {
                        setIsReAssignLoading(false);
                      }
                    }}
                  >
                    <BookOpen className="h-4 w-4 text-green-500 dark:text-green-400" />
                    {t('ExamSupervision.ReAssignExam')}
                  </DropdownMenuItem>

                  <DropdownMenuItem
                    className="flex cursor-pointer items-center gap-2 text-sm hover:bg-gray-50 dark:hover:bg-gray-700"
                    onClick={async () => {
                      setStudentExamId(row.studentExamId ?? null);
                      toggleOpenViewStatistic();
                    }}
                  >
                    <BarChart className="h-4 w-4 text-green-500 dark:text-green-400" />
                    {'View Statistics'}
                  </DropdownMenuItem>
                </>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
          <Button
            onClick={() => {
              if (row.studentExamId) {
                setRow(row);
                toggleOpenCreateViolation();
              } else {
                showError(t('ExamSupervision.CannotNavigate'));
              }
            }}
            variant="outline"
            className="border-red-500 hover:bg-red-50 hover:text-red-700"
          >
            {t('ExamSupervision.Violation')}
          </Button>
        </div>
      ),
    },
  ];

  //!Functions
  // const handleChangePage = useCallback(
  //   (page: number) => {
  //     setFilters((prev: any) => {
  //       const newParams = {
  //         ...prev,
  //         CurrentPage: page,
  //       };
  //       save(cachedKeys.cachesFilterMonitorDetail, newParams);
  //       return newParams;
  //     });
  //   },
  //   [setFilters, save],
  // );

  // const handleChangePageSize = useCallback(
  //   (size: number) => {
  //     setTrigger(true);
  //     setFilters((prev: any) => {
  //       const newParams = {
  //         ...prev,
  //         PageSize: size,
  //         CurrentPage: 1,
  //       };
  //       save(cachedKeys.cachesFilterMonitorDetail, newParams);
  //       return newParams;
  //     });
  //   },
  //   [setFilters, save],
  // );

  const handleAssignMoreTime = useCallback(
    async (values: AssignMoreTimeValues) => {
      if (!row?.studentExamId) {
        showError(t('ExamSupervision.NoStudentExamId'));
        return;
      }
      try {
        const payload = {
          studentExamId: row.studentExamId,
          extraMinutes: Number.parseInt(`${values.extraMinutes}`) ?? 0,
        };
        await monitorService.assignMoreTime(payload);
        toggleOpenAssignMoreTime();
        setRow(null); // Clear row after submission
        refetch();
        showSuccess(t('ExamSupervision.AssignMoreTimeSuccess'));
      } catch (error) {
        showError(error);
      }
    },
    [toggleOpenAssignMoreTime, refetch, row?.studentExamId, t],
  );

  const handleRowClick = useCallback(
    (row: Student) => {
      if (row?.studentExamId && examId) {
        navigate(
          `${
            roleId === 4
              ? BaseUrl.AdminSupervision
              : roleId === 3
                ? BaseUrl.SupervisorExamSupervision
                : BaseUrl.ExamSupervision
          }/${examId}/connection/${row?.studentExamId}`,
        );
      } else {
        showError(t('ExamSupervision.NoStudentExamId'));
      }
    },
    [navigate, examId, roleId, t],
  );

  const handleSearch = useCallback(
    (values: IValueFormPageHeader) => {
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          TextSearch: values?.textSearch ?? '',
          CurrentPage: 1,
        };
        return newParams;
      });
    },
    [setFilters],
  );

  //!Render
  return (
    <div className="relative">
      {/* Loading Overlay */}
      {isReAssignLoading && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
          <div className="h-12 w-12 animate-spin rounded-full border-b-2 border-t-2 border-blue-500"></div>
        </div>
      )}
      {/* Main Content with Conditional Blur */}
      <div className={`transition-all duration-300 ${isReAssignLoading ? 'blur-sm' : ''}`}>
        <PageWrapper name={t('ExamSupervision.DetailExamSupervision')}>
          <div className="space-y-6">
            <ExamHeader
              title={t('ExamSupervision.AllConnections')}
              subtitle={t('ExamSupervision.TrackAllConnections')}
              className="border-b border-white/20 bg-gradient-to-r from-green-600 to-blue-700 px-6 py-6 shadow-lg"
              icon={<Globe className="h-8 w-8 text-white" />}
            />
            <UserStats
              statItems={statItems}
              className="mt-2 grid-cols-1 md:grid-cols-2 lg:grid-cols-4"
            />
            <GenericFilters
              className="md:grid-cols-3"
              searchPlaceholder={t('ExamSupervision.SearchByNameOrCode')}
              onSearch={handleSearch}
              initialSearchQuery={filters?.TextSearch}
              filters={[
                {
                  key: 'StudentExamStatus',
                  placeholder: t('ExamSupervision.SelectStatus'),
                  options: [
                    { label: t('ExamSupervision.AllStatus'), value: undefined },
                    { label: t('ExamSupervision.NotStarted'), value: 0 },
                    { label: t('ExamSupervision.InProgress'), value: 1 },
                    { label: t('ExamSupervision.Submitted'), value: 2 },
                    { label: t('ExamSupervision.Cancelled'), value: 3 },
                  ],
                },
              ]}
              onFilterChange={(
                newFilters: Record<string, string | number | boolean | null | undefined>,
              ) => {
                setFilters((prev) => {
                  const updatedFilters = {
                    ...prev,
                    ...newFilters,
                  };
                  return updatedFilters;
                });
              }}
            />
            <MemoizedTablePaging
              columns={columns}
              data={dataStudents ?? []}
              currentPage={1}
              currentSize={50}
              totalPage={1}
              total={0}
              loading={isLoadingMonitorDetail}
              // handleChangePage={handleChangePage}
              // handleChangePageSize={handleChangePageSize}
            />
          </div>
          {shouldRenderOpenAssignMoreTime && (
            <DialogAssignMoreTime
              isOpen={isOpenAssignMoreTime}
              toggle={handleToggleAssignMoreTime}
              onSubmit={handleAssignMoreTime}
              row={row}
            />
          )}
          {shouldRenderOpenCreateViolation && (
            <DialogCreateViolation
              isOpen={isOpenCreateViolation}
              toggle={toggleOpenCreateViolation}
              row={row}
            />
          )}
          {shouldRenderViewStatistic && (
            <DialogStatistic
              isOpen={isOpenViewStatistic}
              toggle={toggleOpenViewStatistic}
              studentExamId={studentExamId}
            />
          )}
        </PageWrapper>
      </div>
    </div>
  );
};

export default DetailExamSupervision;
