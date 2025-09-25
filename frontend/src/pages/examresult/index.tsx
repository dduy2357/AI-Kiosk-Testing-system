import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { DateTimeFormat } from '@/consts/dates';
import { convertUTCToVietnamTime, formatScore } from '@/helpers/common';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useGetAllHistoryExam from '@/services/modules/studentexam/hooks/useGetAllHistoryExam';
import {
  HistoryExamList,
  IHistoryExamRequest,
} from '@/services/modules/studentexam/interfaces/studentexam.interface';
import { useGet, useSave } from '@/stores/useStores';
import { formatDurationSecond } from '@/utils/exam.utils';
import {
  AlertCircle,
  Award,
  BookOpen,
  BookOpenCheck,
  Calendar,
  CheckCircle,
  FolderSearch,
  Timer,
  Trophy,
  XCircle,
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import {
  GenericFilters,
  IValueFormPageHeader,
} from '../admin/manageuser/components/generic-filters';
import ExamHeader from '../teacher/examsupervision/components/ExamHeader';

const getScoreColor = (score: number) => {
  if (score >= 9) return 'text-green-600';
  if (score >= 8) return 'text-blue-600';
  if (score >= 7) return 'text-yellow-600';
  if (score >= 5) return 'text-orange-600';
  return 'text-red-600';
};

const getScoreBadge = (score: number) => {
  if (score >= 9) {
    return (
      <Badge className="border-green-200 bg-green-100 text-green-800">
        <Award className="mr-1 h-3 w-3" />
        {'Excellent'}
      </Badge>
    );
  } else if (score >= 8) {
    return (
      <Badge className="border-blue-200 bg-blue-100 text-blue-800">
        <CheckCircle className="mr-1 h-3 w-3" />
        {'Good'}
      </Badge>
    );
  } else if (score >= 7) {
    return (
      <Badge className="border-yellow-200 bg-yellow-100 text-yellow-800">
        <AlertCircle className="mr-1 h-3 w-3" />
        {'Fair'}
      </Badge>
    );
  } else if (score >= 5) {
    return (
      <Badge className="border-orange-200 bg-orange-100 text-orange-800">
        <AlertCircle className="mr-1 h-3 w-3" />
        {'Average'}
      </Badge>
    );
  } else {
    return (
      <Badge className="border-red-200 bg-red-100 text-red-800">
        <XCircle className="mr-1 h-3 w-3" />
        {'Weak'}
      </Badge>
    );
  }
};

const ExamResult = () => {
  const { t } = useTranslation('shared');
  //!State
  const save = useSave();
  const dataStudentExamHistory = useGet('dataStudentExamHistory');
  const totalStudentExamHistoryCount = useGet('totalStudentExamHistoryCount');
  const totalPageStudentExamHistoryCount = useGet('totalPageStudentExamHistoryCount');
  const cachesFilterStudentExamHistory = useGet('cachesFilterStudentExamHistory');
  const navigate = useNavigate();
  const [isTrigger, setIsTrigger] = useState(Boolean(!dataStudentExamHistory));

  const { filters, setFilters } = useFiltersHandler<IHistoryExamRequest>({
    pageSize: cachesFilterStudentExamHistory?.pageSize ?? 50,
    currentPage: cachesFilterStudentExamHistory?.currentPage ?? 1,
    textSearch: cachesFilterStudentExamHistory?.textSearch ?? '',
  });

  const { data: historyExamData, loading } = useGetAllHistoryExam(filters, {
    isTrigger,
    refetchKey: cachedKeys.refetchStudentExamHistory,
    saveData: true,
  });

  useEffect(() => {
    if (historyExamData && isTrigger) {
      save(cachedKeys.dataStudentExamHistory, historyExamData);
    }
  }, [historyExamData, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? historyExamData : dataStudentExamHistory),
    [historyExamData, dataStudentExamHistory, isTrigger],
  );

  const columns = [
    {
      label: t('StudentExamResult.columns.examTitle'),
      accessor: 'title',
      sortable: false,
      Cell: (row: HistoryExamList) => (
        <div className="max-w-[250px]">
          <div className="mb-2 flex items-start">
            <BookOpen className="mr-2 mt-0.5 h-4 w-4 flex-shrink-0 text-blue-500" />
            <div className="line-clamp-2 font-medium text-gray-900">{row.examTitle}</div>
          </div>
          <div className="ml-6 text-xs text-gray-500">ID: {row.examId}</div>
        </div>
      ),
    },
    {
      label: t('StudentExamResult.columns.score'),
      accessor: 'score',
      sortable: false,
      Cell: (row: HistoryExamList) => (
        <div className="max-w-[100px]">
          <div className="mb-2 flex items-center justify-center">
            <Trophy className="mr-2 h-5 w-5 text-yellow-500" />
            <span className={`text-2xl font-bold ${getScoreColor(row.score)}`}>
              {formatScore(row.score)}
            </span>
          </div>
          <div className="flex justify-center">{getScoreBadge(formatScore(row.score))}</div>
        </div>
      ),
    },
    {
      label: t('StudentExamResult.columns.examDate'),
      accessor: 'examDate',
      sortable: false,
      Cell: (row: HistoryExamList) => (
        <div className="min-w-[120px] text-center">
          <div className="mb-1 flex items-center justify-center">
            <Calendar className="mr-2 h-4 w-4 text-blue-500" />
            <span className="font-medium">
              {convertUTCToVietnamTime(
                row.examDate,
                DateTimeFormat.DateTimeWithTimezone,
              )?.toString()}
            </span>
          </div>
        </div>
      ),
    },
    {
      label: t('StudentExamResult.columns.submitTime'),
      accessor: 'submitTime',
      sortable: false,
      Cell: (row: HistoryExamList) => (
        <div className="min-w-[140px] text-center">
          <div className="mb-1 flex items-center justify-center">
            <CheckCircle className="mr-2 h-4 w-4 text-green-500" />
            <span className="font-medium">
              {convertUTCToVietnamTime(
                row.submitTime,
                DateTimeFormat.DateTimeWithTimezone,
              )?.toString()}
            </span>
          </div>
        </div>
      ),
    },
    {
      label: t('StudentExamResult.columns.durationSpent'),
      accessor: 'durationSpent',
      sortable: false,
      Cell: (row: HistoryExamList) => (
        <div className="max-w-[140px] text-center">
          <div className="flex items-center justify-center">
            <Timer className="mr-2 h-4 w-4 text-orange-500" />
            <span className="text-lg font-medium">{formatDurationSecond(row.durationSpent)}</span>
          </div>
          <div className="mt-1 text-xs text-gray-500">
            {t('StudentExamResult.columns.durationSpentLabel')}
          </div>
        </div>
      ),
    },
    {
      label: t('StudentExamResult.columns.tools'),
      accessor: 'Tools',
      sortable: false,
      Cell: (row: HistoryExamList) => (
        <div className="flex justify-center">
          <Button
            variant="outline"
            size="sm"
            className="border-blue-400 bg-transparent text-blue-500 hover:bg-blue-100"
            onClick={() => navigate(`${BaseUrl.ExamResult}/${row.studentExamId}`)}
          >
            <FolderSearch className="mr-2 h-4 w-4" />
            {t('StudentExamResult.columns.viewResult')}
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
        save(cachedKeys.cachesFilterStudentExamHistory, newParams);
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
          save(cachedKeys.cachesFilterStudentExamHistory, newParams);
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
        save(cachedKeys.cachesFilterStudentExamHistory, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  //!Render
  return (
    <PageWrapper
      name={t('StudentExamResult.pageWrapper.name')}
      className="bg-white dark:bg-gray-900"
      isLoading={loading}
    >
      <div className="space-y-6 p-4">
        <ExamHeader
          title={t('StudentExamResult.examHeader.title')}
          subtitle={t('StudentExamResult.examHeader.subtitle')}
          icon={<BookOpenCheck className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />
        <GenericFilters
          className="md:grid-cols-3 lg:grid-cols-2"
          searchPlaceholder={t('StudentExamResult.genericFilters.searchPlaceholder')}
          onSearch={handleSearch}
          initialSearchQuery={filters?.textSearch ?? ''}
        />
        <div className="rounded-lg bg-white p-4 shadow-sm">
          <MemoizedTablePaging
            columns={columns}
            data={dataMain ?? []}
            currentPage={filters?.currentPage ?? 1}
            currentSize={filters?.pageSize ?? 50}
            totalPage={totalPageStudentExamHistoryCount ?? 1}
            total={totalStudentExamHistoryCount ?? 0}
            loading={loading}
            handleChangePage={handleChangePage}
            handleChangeSize={handleChangePageSize}
          />
        </div>
      </div>
    </PageWrapper>
  );
};

export default ExamResult;
