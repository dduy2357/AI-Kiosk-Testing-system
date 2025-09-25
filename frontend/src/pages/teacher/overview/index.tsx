import PageWrapper from '@/components/PageWrapper/PageWrapper';
import cachedKeys from '@/consts/cachedKeys';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import { UserStats } from '@/pages/admin/manageuser/components/user-stats';
import useGetListExamActivityLog from '@/services/modules/examactivitylog/hooks/useGetAllExamActivityLog';
import useGetListUserLog from '@/services/modules/log/hooks/useGetUserLogList';
import { useGet, useSave } from '@/stores/useStores';
import { FileText, HelpCircle, Home } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import ExamHeader from '../examsupervision/components/ExamHeader';
import LatestAlerts from './components/latest-alerts';
import QuickAccess from './components/quick-access';
import RecentActivities from './components/recent-activities';
import { useTranslation } from 'react-i18next';

const Overview = () => {
  //! State
  const { t } = useTranslation('shared');
  const defaultData = useGet('dataUserLog');
  const cachesFilterUserLog = useGet('cachesFilterUserLog');
  const [isTrigger] = useState(Boolean(!defaultData));
  const save = useSave();
  const { filters } = useFiltersHandler({
    PageSize: cachesFilterUserLog?.PageSize || 50,
    CurrentPage: cachesFilterUserLog?.CurrentPage || 1,
    TextSearch: cachesFilterUserLog?.TextSearch || '',
  });

  const { data: dataUserLog, loading } = useGetListUserLog(filters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchUserLog,
    saveData: true,
  });

  useEffect(() => {
    if (dataUserLog && isTrigger) {
      save(cachedKeys.dataUserLog, dataUserLog);
    }
  }, [dataUserLog, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? dataUserLog : defaultData) || [],
    [isTrigger, defaultData, dataUserLog],
  );

  const filterExamLog = useMemo(
    () => ({
      pageSize: 50,
      currentPage: 1,
      textSearch: '',
    }),
    [],
  );

  const { data: listExamLogActivity } = useGetListExamActivityLog(filterExamLog, {});

  const statItems = useMemo(() => {
    const totalRecentActivities = dataMain?.length;
    const totalWaitingActivities = listExamLogActivity?.length || 0;

    return [
      {
        title: t('Overview.RecentActivities'),
        value: totalRecentActivities,
        icon: <HelpCircle className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
      {
        title: t('Overview.WaitingActivities'),
        value: totalWaitingActivities,
        icon: <FileText className="h-6 w-6 text-green-500" />,
        bgColor: 'bg-green-50',
      },
    ];
  }, [dataMain, listExamLogActivity, t]);

  //! Functions

  //! Render
  return (
    <PageWrapper name="Tổng quan" className="bg-white dark:bg-gray-900" isLoading={loading}>
      <div className="space-y-6">
        <ExamHeader
          title={t('Overview.Title')}
          subtitle={t('Overview.Subtitle')}
          icon={<Home className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />

        <div className="space-y-8 p-6">
          <UserStats statItems={statItems} className="grid-cols-1 md:grid-cols-2 lg:grid-cols-2" />
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <RecentActivities dataUserLog={dataMain} />
            <LatestAlerts dataListExamLogActivity={listExamLogActivity} />
          </div>
          <QuickAccess />
        </div>
      </div>
    </PageWrapper>
  );
};

export default Overview;
