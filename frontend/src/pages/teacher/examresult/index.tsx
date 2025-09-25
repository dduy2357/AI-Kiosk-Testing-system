import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime } from '@/helpers/common';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import {
  GenericFilters,
  IValueFormPageHeader,
} from '@/pages/admin/manageuser/components/generic-filters';
import useGetAllTeacherExam from '@/services/teacherexam/hooks/useGetAllTeacherExam';
import {
  ITeacherExamRequest,
  TeacherExamList,
} from '@/services/teacherexam/interfaces/teacherexamm.interface';
import { useGet, useSave } from '@/stores/useStores';
import { formatDuration } from '@/utils/exam.utils';
import {
  BookOpen,
  BookOpenCheck,
  Calendar,
  CheckCircle,
  Eye,
  EyeOff,
  FileText,
  FolderSearch,
  Pause,
  Play,
  Square,
  Timer,
  Trophy,
  User,
  Users,
  XCircle,
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ExamHeader from '../examsupervision/components/ExamHeader';

const getExamTypeBadge = (examType: number) => {
  switch (examType) {
    case 0:
      return (
        <Badge variant="outline" className="bg-purple-50 text-purple-700">
          Tự luận
        </Badge>
      );
    case 1:
      return (
        <Badge variant="outline" className="bg-indigo-50 text-indigo-700">
          Trắc nghiệm
        </Badge>
      );
    case 2:
      return (
        <Badge variant="outline" className="bg-teal-50 text-teal-700">
          Đúng/Sai
        </Badge>
      );
    case 3:
      return (
        <Badge variant="outline" className="bg-yellow-50 text-yellow-700">
          Fill in the blank
        </Badge>
      );
    default:
      return <Badge variant="outline">Khác</Badge>;
  }
};

const getStatusBadge = (status: number) => {
  switch (status) {
    case 0:
      return (
        <Badge variant="secondary" className="bg-gray-100 text-gray-700">
          <Square className="mr-1 h-3 w-3" />
          Chưa bắt đầu
        </Badge>
      );
    case 1:
      return (
        <Badge variant="default" className="bg-green-100 text-green-700">
          <CheckCircle className="mr-1 h-3 w-3" />
          Hoàn thành
        </Badge>
      );
    case 2:
      return (
        <Badge variant="destructive" className="bg-red-100 text-red-700">
          <XCircle className="mr-1 h-3 w-3" />
          Đã hủy
        </Badge>
      );
    default:
      return <Badge variant="outline">Không xác định</Badge>;
  }
};

const getLiveStatusBadge = (liveStatus: number) => {
  switch (liveStatus) {
    case 0:
      return (
        <Badge variant="outline" className="bg-gray-50">
          <Square className="mr-1 h-3 w-3" />
          Chưa bắt đầu
        </Badge>
      );
    case 1:
      return (
        <Badge variant="default" className="bg-blue-100 text-blue-700">
          <Play className="mr-1 h-3 w-3" />
          Đang diễn ra
        </Badge>
      );
    case 2:
      return (
        <Badge variant="secondary" className="bg-orange-100 text-orange-700">
          <Pause className="mr-1 h-3 w-3" />
          Đã kết thúc
        </Badge>
      );
    default:
      return <Badge variant="outline">Không xác định</Badge>;
  }
};

const ExamResultTeacher = () => {
  //!State
  const navigate = useNavigate();
  const save = useSave();
  const dataStudentExamResult = useGet('dataStudentExamResult');
  const totalStudentExamResultCount = useGet('totalStudentExamResultCount');
  const totalPageStudentExamResultCount = useGet('totalPageStudentExamResultCount');
  const cachesFilterStudentExamResult = useGet('cachesFilterStudentExamResult');
  const [isTrigger, setIsTrigger] = useState(Boolean(!dataStudentExamResult));

  const { filters, setFilters } = useFiltersHandler<ITeacherExamRequest>({
    pageSize: cachesFilterStudentExamResult?.pageSize ?? 50,
    currentPage: cachesFilterStudentExamResult?.currentPage ?? 1,
    textSearch: cachesFilterStudentExamResult?.textSearch ?? '',
    status:
      cachesFilterStudentExamResult?.status !== undefined
        ? cachesFilterStudentExamResult.status
        : undefined,
    IsMyQuestion:
      cachesFilterStudentExamResult?.IsMyQuestion !== undefined
        ? cachesFilterStudentExamResult.IsMyQuestion
        : undefined,
    IsExamResult:
      cachesFilterStudentExamResult?.IsExamResult !== undefined
        ? cachesFilterStudentExamResult.IsExamResult
        : undefined,
  });

  const { data: studentExamResults, loading } = useGetAllTeacherExam(filters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchStudentExamResult,
    saveData: true,
  });

  useEffect(() => {
    if (studentExamResults && isTrigger) {
      save(cachedKeys.dataStudentExamResult, studentExamResults);
    }
  }, [studentExamResults, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? studentExamResults : dataStudentExamResult),
    [studentExamResults, dataStudentExamResult, isTrigger],
  );

  const columns = [
    {
      label: 'Tiêu đề',
      accessor: 'title',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="max-w-[200px]">
          <div className="mb-1 font-medium text-gray-900">{row.title}</div>
          <div className="line-clamp-2 text-sm text-gray-500">{row.description}</div>
        </div>
      ),
    },
    {
      label: 'Phòng thi',
      accessor: 'roomName',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="flex items-center">
          <Users className="mr-2 h-4 w-4 text-blue-500" />
          <span className="font-medium">{row.roomName}</span>
        </div>
      ),
    },
    {
      label: 'Câu hỏi & Điểm',
      accessor: 'questions',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="text-center">
          <div className="mb-1 flex items-center justify-center">
            <FileText className="mr-1 h-4 w-4 text-green-500" />
            <span className="font-medium">{row.totalQuestions} câu</span>
          </div>
          <div className="flex items-center justify-center">
            <Trophy className="mr-1 h-4 w-4 text-yellow-500" />
            <span className="text-sm text-gray-600">{row.totalPoints} điểm</span>
          </div>
        </div>
      ),
    },
    {
      label: 'Thời gian',
      accessor: 'duration',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="text-center">
          <div className="flex items-center justify-center">
            <Timer className="mr-1 h-4 w-4 text-orange-500" />
            <span className="font-medium">{formatDuration(row.duration)}</span>
          </div>
        </div>
      ),
    },
    {
      label: 'Thời gian thi',
      accessor: 'examTime',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="min-w-[140px]">
          <div className="mb-1 flex items-center">
            <Calendar className="mr-1 h-4 w-4 text-blue-500" />
            <span className="text-sm">Bắt đầu:</span>
          </div>
          <div className="mb-2 text-sm font-medium">
            {convertUTCToVietnamTime(
              row.startTime,
              DateTimeFormat.DateTimeWithTimezone,
            )?.toString()}
          </div>
          <div className="mb-1 flex items-center">
            <Calendar className="mr-1 h-4 w-4 text-red-500" />
            <span className="text-sm">Kết thúc:</span>
          </div>
          <div className="text-sm font-medium">
            {convertUTCToVietnamTime(row.endTime, DateTimeFormat.DateTimeWithTimezone)?.toString()}
          </div>
        </div>
      ),
    },
    {
      label: 'Người tạo',
      accessor: 'createdBy',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="flex items-center">
          <User className="mr-2 h-4 w-4 text-gray-500" />
          <span className="text-sm">{row.createdBy}</span>
        </div>
      ),
    },
    {
      label: 'Loại đề',
      accessor: 'examType',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="flex items-center justify-center">
          <BookOpen className="mr-2 h-4 w-4 text-purple-500" />
          {getExamTypeBadge(row.examType)}
        </div>
      ),
    },
    {
      label: 'Trạng thái',
      accessor: 'status',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="text-center">
          <div className="mb-2">{getStatusBadge(row.status)}</div>
          <div>{getLiveStatusBadge(row.liveStatus)}</div>
        </div>
      ),
    },
    {
      label: 'Hiển thị',
      accessor: 'display',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="text-center">
          <div className="mb-2 flex items-center justify-center">
            {row.isShowResult ? (
              <Eye className="mr-1 h-4 w-4 text-green-500" />
            ) : (
              <EyeOff className="mr-1 h-4 w-4 text-gray-400" />
            )}
            <span className="text-xs">Kết quả</span>
          </div>
          <div className="flex items-center justify-center">
            {row.isShowCorrectAnswer ? (
              <CheckCircle className="mr-1 h-4 w-4 text-green-500" />
            ) : (
              <XCircle className="mr-1 h-4 w-4 text-gray-400" />
            )}
            <span className="text-xs">Đáp án</span>
          </div>
        </div>
      ),
    },
    {
      label: 'Công cụ',
      accessor: 'Tools',
      sortable: false,
      Cell: (row: TeacherExamList) => (
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            className="border-blue-400 bg-transparent text-blue-500 hover:bg-blue-100"
            onClick={() => navigate(`${BaseUrl.ExamResultTeacher}/${row.examId}`)}
          >
            <FolderSearch className="mr-2 h-4 w-4" />
            Xem kết quả
          </Button>
        </div>
      ),
    },
  ];

  //!Functions
  const handleChangePageSize = useCallback(
    (size: number) => {
      setIsTrigger(true);
      setFilters((prev) => {
        const newParams = { ...prev, currentPage: 1, pageSize: size };
        save(cachedKeys.cachesFilterStudentExamResult, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleChangePage = useCallback(
    (page: number) => {
      setTimeout(() => {
        setFilters((prev) => {
          const newParams = { ...prev, currentPage: page };
          save(cachedKeys.cachesFilterStudentExamResult, newParams);
          return newParams;
        });
      }, 0);
    },
    [setFilters, save],
  );

  const handleSearch = useCallback(
    (value: IValueFormPageHeader) => {
      setIsTrigger(true);
      setFilters((prev) => {
        const newParams = { ...prev, textSearch: value.textSearch ?? '', currentPage: 1 };
        save(cachedKeys.cachesFilterStudentExamResult, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  //!Render
  return (
    <PageWrapper name="Kết quả thi" className="bg-white dark:bg-gray-900" isLoading={loading}>
      <div className="space-y-6">
        <ExamHeader
          title="Kết quả thi của sinh viên"
          subtitle="Xem kết quả các bài thi đã tạo"
          icon={<BookOpenCheck className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />
        <GenericFilters
          className="md:grid-cols-3 lg:grid-cols-5"
          searchPlaceholder="Tìm kiếm ứng dụng..."
          onSearch={handleSearch}
          initialSearchQuery={filters?.textSearch ?? ''}
          filters={[
            {
              key: 'IsMyQuestion',
              placeholder: 'Lọc đề theo người tạo',
              options: [
                { value: undefined, label: 'Tất cả' },
                { value: true, label: 'Đề của tôi' },
                { value: false, label: 'Không phải đề của tôi' },
              ],
            },
            {
              key: 'status',
              placeholder: 'Trạng thái bài thi',
              options: [
                { value: undefined, label: 'Tất cả' },
                { value: 0, label: 'Chưa bắt đầu' },
                { value: 1, label: 'Đã hoàn thành' },
                { value: 2, label: 'Đã hủy' },
              ],
            },
            {
              key: 'IsExamResult',
              placeholder: 'Kết quả bài thi',
              options: [
                { value: undefined, label: 'Tất cả' },
                { value: true, label: 'Có kết quả' },
                { value: false, label: 'Không có kết quả' },
              ],
            },
          ]}
          onFilterChange={(
            newFilters: Record<string, string | number | boolean | null | undefined>,
          ) => {
            setIsTrigger(true);
            setFilters((prev) => {
              const updatedFilters = {
                ...prev,
                ...newFilters,
              };
              save(cachedKeys.cachesFilterStudentExamResult, updatedFilters);
              return updatedFilters;
            });
          }}
        />
        <div className="rounded-lg bg-white p-4 shadow-sm">
          <MemoizedTablePaging
            columns={columns}
            data={dataMain ?? []}
            currentPage={filters?.currentPage ?? 1}
            currentSize={filters?.pageSize ?? 50}
            totalPage={totalPageStudentExamResultCount ?? 1}
            total={totalStudentExamResultCount ?? 0}
            loading={loading}
            handleChangePage={handleChangePage}
            handleChangeSize={handleChangePageSize}
          />
        </div>
      </div>
    </PageWrapper>
  );
};

export default ExamResultTeacher;
