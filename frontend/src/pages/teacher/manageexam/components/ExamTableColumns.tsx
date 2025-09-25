import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import BaseUrl from '@/consts/baseUrl';
import { ActiveStatusExamStudent, ExamLiveStatus, ActiveStatusExamTeacher } from '@/consts/common';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime } from '@/helpers/common';
import { showError } from '@/helpers/toast';
import httpService from '@/services/httpService';
import { ManageExamList } from '@/services/modules/manageexam/interfaces/manageExam.interface';
import manageExamService from '@/services/modules/manageexam/manageExam.service';
import {
  BookOpen,
  Calendar,
  Clock,
  Edit,
  Eye,
  HelpCircle,
  Key,
  MoreHorizontal,
  Shield,
  Users,
  ToggleLeft,
} from 'lucide-react';
import moment from 'moment';
import { useNavigate } from 'react-router-dom';

interface GetColumnsParams {
  navigate: ReturnType<typeof useNavigate>;
  setExamId: (id: string) => void;
  toggleDialogViewExamDetail: () => void;
  toggleDialogAssignOtpToExamTeacher: () => void;
  dataOtps: any[];
  setViewingOtp: (otp: any) => void;
  roleId: number;
  handleToggleDetailExam: (row: ManageExamList) => void;
  refreshExamList: () => void;
}

export const getManageExamColumns = ({
  navigate,
  setExamId,
  toggleDialogViewExamDetail,
  toggleDialogAssignOtpToExamTeacher,
  dataOtps,
  setViewingOtp,
  roleId,
  handleToggleDetailExam,
  refreshExamList,
}: GetColumnsParams) => [
  {
    label: 'Title',
    accessor: 'title',
    sortable: false,
    Cell: (row: ManageExamList) => (
      <div className="flex items-center space-x-3">
        <div className="flex-shrink-0">
          <BookOpen className="h-5 w-5 text-blue-500" />
        </div>
        <div className="min-w-0 flex-1">
          <p className="cursor-pointer truncate text-sm font-medium text-blue-600 hover:text-blue-800">
            {row.title.toUpperCase()}
          </p>
        </div>
      </div>
    ),
  },
  {
    label: 'Room',
    accessor: 'roomName',
    sortable: false,
    Cell: (row: ManageExamList) => (
      <div className="flex items-center space-x-2">
        <Users className="h-4 w-4 text-gray-400" />
        <span className="text-sm font-medium text-gray-900">{row.roomName}</span>
      </div>
    ),
  },
  {
    label: 'Questions',
    accessor: 'totalQuestions',
    sortable: false,
    Cell: (row: ManageExamList) => (
      <div className="flex items-center space-x-2">
        <HelpCircle className="h-4 w-4 text-orange-400" />
        <Badge variant="secondary" className="border-orange-200 bg-orange-50 text-orange-700">
          {row.totalQuestions} questions
        </Badge>
      </div>
    ),
  },
  {
    label: 'Duration',
    accessor: 'duration',
    sortable: false,
    Cell: (row: ManageExamList) => (
      <div className="flex items-center space-x-2">
        <Clock className="h-4 w-4 text-purple-400" />
        <Badge variant="outline" className="border-purple-200 text-purple-700">
          {row.duration} minutes
        </Badge>
      </div>
    ),
  },
  {
    label: 'Start Time',
    accessor: 'startTime',
    sortable: false,
    Cell: (row: ManageExamList) => {
      const vietnamStartTime = convertUTCToVietnamTime(
        row?.startTime,
        DateTimeFormat.DateTimeWithTimezone,
      );
      return (
        <div className="flex items-center space-x-2">
          <Calendar className="h-4 w-4 text-green-400" />
          <div className="text-sm">
            <div className="font-medium text-gray-900">
              {typeof vietnamStartTime === 'string'
                ? vietnamStartTime
                : vietnamStartTime?.format(DateTimeFormat.DateTimeWithTimezone)}
            </div>
          </div>
        </div>
      );
    },
  },
  {
    label: 'End Time',
    accessor: 'endTime',
    sortable: false,
    Cell: (row: ManageExamList) => {
      const vietnamEndTime = convertUTCToVietnamTime(
        row?.endTime,
        DateTimeFormat.DateTimeWithTimezone,
      );
      return (
        <div className="flex items-center space-x-2">
          <Calendar className="h-4 w-4 text-red-400" />
          <div className="text-sm">
            <div className="font-medium text-gray-900">
              {typeof vietnamEndTime === 'string'
                ? vietnamEndTime
                : vietnamEndTime?.format(DateTimeFormat.DateTimeWithTimezone)}
            </div>
          </div>
        </div>
      );
    },
  },
  {
    label: 'Status',
    accessor: 'status',
    sortable: false,
    Cell: (row: ManageExamList) => {
      const status = row?.status;

      const statusConfig = {
        [ActiveStatusExamStudent.Published]: {
          color: 'bg-green-100 text-green-800 border-green-200',
          icon: <Shield className="h-3 w-3" />,
          label: 'Published',
        },
        [ActiveStatusExamStudent.Cancelled]: {
          color: 'bg-red-100 text-red-800 border-red-200',
          icon: <Shield className="h-3 w-3" />,
          label: 'Cancelled',
        },
        [ActiveStatusExamStudent.Draft]: {
          color: 'bg-yellow-100 text-yellow-800 border-yellow-200',
          icon: <Clock className="h-3 w-3" />,
          label: 'Draft',
        },
      };

      const config = statusConfig[status as keyof typeof statusConfig];

      return config ? (
        <Badge variant="outline" className={`${config.color} flex items-center space-x-1`}>
          {config.icon}
          <span className="text-xs font-medium">{config.label}</span>
        </Badge>
      ) : null;
    },
  },
  {
    label: 'Live Satus',
    accessor: 'liveStatus',
    sortable: false,
    Cell: (row: ManageExamList) => {
      const liveStatusKey = Object.keys(ExamLiveStatus).find(
        (key) => ExamLiveStatus[key as keyof typeof ExamLiveStatus] === row.liveStatus,
      ) as keyof typeof ExamLiveStatus | undefined;

      const statusConfig = {
        Inactive: {
          color: 'bg-gray-100 text-gray-800 border-gray-200',
          icon: <Shield className="h-3 w-3" />,
          label: 'Inactive',
        },
        Upcoming: {
          color: 'bg-blue-100 text-blue-800 border-blue-200',
          icon: <Calendar className="h-3 w-3" />,
          label: 'Upcoming',
        },
        Ongoing: {
          color: 'bg-green-100 text-green-800 border-green-200',
          icon: <Clock className="h-3 w-3" />,
          label: 'Ongoing',
        },
        Completed: {
          color: 'bg-purple-100 text-purple-800 border-purple-200',
          icon: <Shield className="h-3 w-3" />,
          label: 'Completed',
        },
      };

      if (!liveStatusKey || !statusConfig[liveStatusKey]) {
        return null;
      }

      const config = statusConfig[liveStatusKey];

      return (
        <Badge variant="outline" className={`${config.color} flex items-center space-x-1`}>
          {config.icon}
          <span className="text-xs font-medium">{config.label}</span>
        </Badge>
      );
    },
  },
  {
    label: 'View Detail',
    accessor: 'viewDetail',
    sortable: false,
    Cell: (row: ManageExamList) => (
      <div className="flex justify-center">
        <Button
          variant="outline"
          size="sm"
          className="border-blue-200 bg-blue-50 text-blue-700 hover:border-blue-300 hover:bg-blue-100"
          onClick={() => {
            setExamId(row.examId);
            toggleDialogViewExamDetail();
          }}
        >
          <Eye className="mr-1 h-4 w-4" />
          View Detail
        </Button>
      </div>
    ),
  },
  {
    label: 'OTP',
    accessor: 'otp',
    sortable: false,
    Cell: (row: ManageExamList) => {
      const vietnamEndTime = row.endTime ? convertUTCToVietnamTime(row.endTime) : null;
      const currentTime = moment();
      const isPublished = row?.status === ActiveStatusExamStudent.Published;
      const isExamOngoing =
        vietnamEndTime && moment.isMoment(vietnamEndTime)
          ? vietnamEndTime.isAfter(currentTime)
          : false;
      const otpExpired = httpService.getOtpExpiredStorage(row.examId);
      const isOtpValid = otpExpired && moment(otpExpired).isAfter(moment());
      const canCreateOtp = isPublished && isExamOngoing && !isOtpValid;

      return (
        <div className="flex justify-center">
          {canCreateOtp && (
            <Button
              variant="outline"
              size="sm"
              className="border-blue-200 bg-blue-50 text-blue-700 hover:border-blue-300 hover:bg-blue-100"
              onClick={() => {
                setExamId(row.examId);
                setViewingOtp(null);
                toggleDialogAssignOtpToExamTeacher();
              }}
            >
              <Key className="mr-1 h-4 w-4" />
              Assign OTP
            </Button>
          )}
        </div>
      );
    },
  },
  {
    label: 'View OTP',
    accessor: 'viewOtp',
    sortable: false,
    Cell: (row: ManageExamList) => {
      const otpExpired = httpService.getOtpExpiredStorage(row.examId);
      const isOtpValid = otpExpired && moment(otpExpired).isAfter(moment());
      const otpData = dataOtps.find((otp) => otp.examId === row.examId);

      return (
        <div className="flex justify-center">
          {isOtpValid && otpData && (
            <Button
              variant="outline"
              size="sm"
              className="border-purple-200 bg-purple-50 text-purple-700 hover:border-purple-300 hover:bg-purple-100"
              onClick={() => {
                setExamId(row.examId);
                setViewingOtp(otpData);
                toggleDialogAssignOtpToExamTeacher();
              }}
            >
              <Eye className="mr-1 h-4 w-4" />
              View OTP
            </Button>
          )}
        </div>
      );
    },
  },
  {
    label: 'Actions',
    accessor: 'actions',
    width: 80,
    sortable: false,
    Cell: (row: ManageExamList) => (
      <div className="flex items-center justify-center">
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              className="h-7 w-7 rounded p-0 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
            >
              <MoreHorizontal className="h-3.5 w-3.5 text-gray-600 dark:text-gray-400" />
              <span className="sr-only">Open actions menu</span>
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-40">
            <DropdownMenuItem
              className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
              onClick={() => {
                handleToggleDetailExam(row);
              }}
            >
              <Eye className="mr-2 h-3.5 w-3.5 text-blue-600 dark:text-blue-400" />
              <span>Assign Exam</span>
            </DropdownMenuItem>
            {row?.status === ActiveStatusExamStudent.Draft && (
              <DropdownMenuItem
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                onClick={() => {
                  navigate(
                    `${
                      roleId === 4 ? BaseUrl.AdminAddNewExam : BaseUrl.AddNewExamLecture
                    }/${row.examId}`,
                  );
                }}
              >
                <Edit className="mr-2 h-3.5 w-3.5 text-amber-500" />
                <span>Edit</span>
              </DropdownMenuItem>
            )}
            {/* New DropdownMenuItem for Changing Status */}
            {row?.status === ActiveStatusExamStudent.Draft && (
              <DropdownMenuItem
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                onClick={async () => {
                  try {
                    await manageExamService.changeExamStatus(
                      row.examId,
                      ActiveStatusExamTeacher.Published,
                    );
                    refreshExamList();
                  } catch (error) {
                    showError(error);
                  }
                }}
              >
                <ToggleLeft className="mr-2 h-3.5 w-3.5 text-green-600 dark:text-green-400" />
                <span>Publish</span>
              </DropdownMenuItem>
            )}
            {row?.status === ActiveStatusExamStudent.Draft && (
              <DropdownMenuItem
                className="cursor-pointer text-xs hover:bg-gray-50 dark:hover:bg-gray-800"
                onClick={async () => {
                  try {
                    await manageExamService.changeExamStatus(
                      row.examId,
                      ActiveStatusExamTeacher.Finished,
                    );
                    refreshExamList();
                  } catch (error) {
                    showError(error);
                  }
                }}
              >
                <ToggleLeft className="mr-2 h-3.5 w-3.5 text-purple-600 dark:text-purple-400" />
                <span>Finish</span>
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    ),
  },
];
