import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { Badge } from '@/components/ui/badge';
import cachedKeys from '@/consts/cachedKeys';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useGetListMonitor from '@/services/modules/monitor/hooks/useGetListMonitor';
import type {
  IMonitorRequest,
  MonitorList,
} from '@/services/modules/monitor/interfaces/monitor.interface';
import { useGet, useSave } from '@/stores/useStores';
import { Monitor } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import CurrentExamDetails from './components/CurrentExamDetails';
import ExamHeader from './components/ExamHeader';
import ExamList from './components/ExamList';
import ExamStatistics from './components/ExamStatistics';
import { useTranslation } from 'react-i18next';

const ExamSupervision = () => {
  //!State
  const { t } = useTranslation('shared');
  const defaultData = useGet('dataMonitor');
  const cachedFilterMonitor = useGet('cachesFilterMonitor');
  const [isTrigger] = useState(Boolean(!defaultData));
  const save = useSave();
  const [selectedExam, setSelectedExam] = useState<MonitorList | null>(null);
  const [currentPage, setCurrentPage] = useState(cachedFilterMonitor?.CurrentPage ?? 1);

  const { filters, setFilters } = useFiltersHandler({
    PageSize: cachedFilterMonitor?.PageSize ?? 10,
    CurrentPage: currentPage,
    TextSearch: cachedFilterMonitor?.TextSearch ?? '',
    SubjectId: cachedFilterMonitor?.SubjectId ?? 0,
    ExamStatus: cachedFilterMonitor?.ExamStatus ?? undefined,
  });

  const stableFilters = useMemo(
    () => ({ ...filters, CurrentPage: currentPage }) as IMonitorRequest,
    [filters, currentPage],
  );

  const {
    data: dataMonitor,
    loading,
    refetch,
    totalPage,
  } = useGetListMonitor(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchMonitor,
    saveData: true,
  });

  useEffect(() => {
    if (dataMonitor && isTrigger) {
      const uniqueDataMonitor = Array.from(
        new Map(dataMonitor.map((exam) => [exam.examId, exam])).values(),
      );
      save(cachedKeys.dataMonitor, uniqueDataMonitor);
    }
  }, [dataMonitor, isTrigger, save]);

  useEffect(() => {
    if (dataMonitor && dataMonitor.length > 0 && !selectedExam) {
      const ongoingExam = dataMonitor.find((exam) => exam.status === 1);
      setSelectedExam(ongoingExam ?? dataMonitor[0]);
    }
  }, [dataMonitor, selectedExam]);

  const dataMain: MonitorList[] = useMemo(() => {
    const data = isTrigger ? dataMonitor : (defaultData ?? []);
    const uniqueData = Array.from(
      new Map((data as MonitorList[]).map((exam) => [exam.examId, exam])).values(),
    );
    return uniqueData as MonitorList[];
  }, [dataMonitor, defaultData, isTrigger]);

  const getStatusBadge = (status: number) => {
    switch (status) {
      case 0:
        return (
          <Badge variant="secondary" className="bg-gray-100 text-gray-700">
            {t('ExamSupervision.Title')}
          </Badge>
        );
      case 1:
        return (
          <Badge className="border-emerald-200 bg-emerald-100 text-emerald-700">
            {t('ExamSupervision.OnGoing')}
          </Badge>
        );
      case 2:
        return (
          <Badge variant="outline" className="border-blue-200 bg-blue-50 text-blue-700">
            {t('ExamSupervision.Ended')}
          </Badge>
        );
      default:
        return <Badge variant="secondary">{t('ExamSupervision.Unknown')}</Badge>;
    }
  };

  //!Functions
  const getRemainingTime = (endTime: Date) => {
    const now = new Date();
    const end = new Date(endTime);
    const diff = end.getTime() - now.getTime();
    if (diff <= 0) return t('ExamSupervision.TimeUp');
    const hours = Math.floor(diff / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    return `${hours}h ${minutes}m`;
  };

  const getCompletionPercentage = (exam: MonitorList) => {
    const total = exam.studentDoing + exam.studentCompleted;
    return total > 0 ? Math.round((exam.studentCompleted / total) * 100) : 0;
  };

  const handleSelectExam = (exam: MonitorList) => {
    setSelectedExam(exam);
  };

  const handlePageChange = (newPage: number) => {
    if (newPage >= 1 && newPage <= totalPage && newPage !== currentPage) {
      setCurrentPage(newPage);
      setFilters((prev: any) => ({ ...prev, CurrentPage: newPage }));
    }
  };

  //!Render
  return (
    <PageWrapper
      name={t('ExamSupervision.Title')}
      className="bg-gradient-to-br from-slate-50 to-blue-50/30"
      isLoading={loading}
    >
      <div className="space-y-6">
        <ExamHeader
          title={t('ExamSupervision.Title')}
          subtitle={t('ExamSupervision.Subtitle')}
          icon={<Monitor className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />
        <div className="space-y-8 p-6">
          <ExamStatistics dataMain={dataMain} />
          <div className="grid grid-cols-1 gap-8 lg:grid-cols-3">
            <div className="lg:col-span-2">
              <ExamList
                dataMain={dataMain}
                getStatusBadge={getStatusBadge}
                getRemainingTime={getRemainingTime}
                getCompletionPercentage={getCompletionPercentage}
                onSelectExam={handleSelectExam}
                refetch={refetch}
                currentPage={currentPage}
                totalPage={totalPage}
                onPageChange={handlePageChange}
                setFilters={setFilters}
              />
            </div>
            {dataMain.filter((exam) => exam.status)[0] && (
              <div className="sticky top-18 self-start lg:col-span-1">
                <CurrentExamDetails
                  currentExam={selectedExam ?? dataMain[0]}
                  getStatusBadge={getStatusBadge}
                  getRemainingTime={getRemainingTime}
                />
              </div>
            )}
          </div>
        </div>
      </div>
    </PageWrapper>
  );
};

export default ExamSupervision;
