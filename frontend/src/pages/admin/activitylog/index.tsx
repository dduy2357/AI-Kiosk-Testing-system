import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { RoleEnum } from '@/consts/common';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetAllUserActivityLog from '@/services/modules/useractivitylog/hooks/useGetAllUserActivityLog';
import { IUserActivityLogRequest } from '@/services/modules/useractivitylog/interfaces/useractivitylog.interface';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { CalendarIcon, ClipboardList } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { GenericFilters } from '../manageuser/components/generic-filters';
import { ActivityList } from './components/activity-list';
import { ActivityStats } from './components/activity-stats';
import { ExportReportModal } from './pages/export-report-modal';

export default function ActivityLogDashboard() {
  const { t } = useTranslation('shared');
  const [totalActivities, setTotalActivities] = useState(0);
  const [isExportModalOpen, setIsExportModalOpen] = useState(false);

  const filter30days = useMemo(() => {
    const thirtyDaysAgo = new Date();
    thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
    return {
      pageSize: 1000,
      currentPage: 1,
      FromDate: thirtyDaysAgo,
      ToDate: new Date(),
      RoleEnum: null,
    };
  }, []);

  const [filtersUserActivityLog, setFiltersUserActivityLog] = useState<IUserActivityLogRequest>({
    FromDate: undefined,
    ToDate: undefined,
    RoleEnum: null,
    pageSize: 10,
    currentPage: 1,
    textSearch: '',
  });

  const { data: activities, totalPage } = useGetAllUserActivityLog(filtersUserActivityLog);
  const { data: statsData } = useGetAllUserActivityLog(filter30days);

  useEffect(() => {
    if (statsData) {
      setTotalActivities(statsData.length);
    }
  }, [statsData]);

  const activeUsers = statsData
    ? [...new Set(statsData.map((activity) => activity.userCode))].length
    : 0;

  const stats = {
    totalActivities: totalActivities,
    activeUsers: activeUsers,
  };

  const currentPage = filtersUserActivityLog.currentPage;

  // Handle page change
  const handlePageChange = (page: number) => {
    if (page > 0 && page <= totalPage) {
      setFiltersUserActivityLog((prev) => ({
        ...prev,
        currentPage: page,
      }));
    }
  };

  // Handle page size change
  const handlePageSizeChange = (value: string) => {
    const newPageSize = parseInt(value);
    setFiltersUserActivityLog((prev) => ({
      ...prev,
      pageSize: newPageSize,
      currentPage: 1, // Reset to first page when changing pageSize
    }));
  };

  // Render pagination with truncation
  const renderPagination = () => {
    const maxPagesToShow = 5; // Number of pages to show around the current page
    const pages: (number | string)[] = [];

    // Calculate the range of pages to display
    const startPage = Math.max(2, currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(totalPage - 1, startPage + maxPagesToShow - 1);

    // Always include the first page
    pages.push(1);

    // Add ellipsis if there's a gap between the first page and startPage
    if (startPage > 2) {
      pages.push('...');
    }

    // Add pages in the calculated range
    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    // Add ellipsis if there's a gap between endPage and the last page
    if (endPage < totalPage - 1) {
      pages.push('...');
    }

    // Always include the last page if totalPage > 1
    if (totalPage > 1) {
      pages.push(totalPage);
    }

    return (
      <div className="mt-4 flex items-center justify-center gap-2">
        <Select
          value={filtersUserActivityLog.pageSize.toString()}
          onValueChange={handlePageSizeChange}
        >
          <SelectTrigger className="w-[100px]">
            <SelectValue placeholder={t('ActivityLog.pageSizePlaceholder')} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="10">10</SelectItem>
            <SelectItem value="20">20</SelectItem>
            <SelectItem value="50">50</SelectItem>
            <SelectItem value="100">100</SelectItem>
          </SelectContent>
        </Select>
        <Button
          variant="outline"
          onClick={() => handlePageChange(currentPage - 1)}
          disabled={currentPage === 1}
          aria-label={t('ActivityLog.previous')}
        >
          {t('ActivityLog.previous')}
        </Button>
        {pages.map((page, index) =>
          page === '...' ? (
            <span key={`ellipsis-${index}`} className="px-3 py-1">
              ...
            </span>
          ) : (
            <Button
              key={page}
              variant={currentPage === page ? 'default' : 'outline'}
              onClick={() => handlePageChange(Number(page))}
              aria-label={`Go to page ${page}`}
            >
              {page}
            </Button>
          ),
        )}
        <Button
          variant="outline"
          onClick={() => handlePageChange(currentPage + 1)}
          disabled={currentPage === totalPage}
          aria-label={t('ActivityLog.next')}
        >
          {t('ActivityLog.next')}
        </Button>
      </div>
    );
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <PageWrapper name={t('ActivityLog.title')} className="bg-white dark:bg-gray-900">
        <div className="space-y-6">
          <ExamHeader
            title={t('ActivityLog.title')}
            subtitle={t('ActivityLog.subtitle')}
            icon={<ClipboardList className="h-8 w-8 text-white" />}
            className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
          />
          <div className="space-y-8 p-6">
            <ActivityStats stats={stats} />
            <GenericFilters
              className="md:grid-cols-3 lg:grid-cols-5"
              searchPlaceholder={t('ActivityLog.searchPlaceholder')}
              onSearch={(e) => {
                setFiltersUserActivityLog((prev: IUserActivityLogRequest) => ({
                  ...prev,
                  textSearch: e.textSearch ?? '',
                }));
              }}
              initialSearchQuery={filtersUserActivityLog.textSearch}
              filters={[
                {
                  key: 'FromDate',
                  placeholder: t('ActivityLog.fromDatePlaceholder'),
                  component: (
                    <div className="flex items-center space-x-[30px]">
                      <Popover>
                        <PopoverTrigger asChild>
                          <Button
                            variant="outline"
                            className="mt-1 w-full justify-start bg-transparent text-left font-normal"
                          >
                            <CalendarIcon className="mr-2 h-4 w-4" />
                            {filtersUserActivityLog.FromDate
                              ? format(filtersUserActivityLog.FromDate, 'dd/MM/yyyy', {
                                  locale: vi,
                                })
                              : t('ActivityLog.selectDate')}
                          </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="start">
                          <Calendar
                            mode="single"
                            selected={filtersUserActivityLog.FromDate}
                            onSelect={(date) =>
                              date &&
                              setFiltersUserActivityLog((prev: IUserActivityLogRequest) => ({
                                ...prev,
                                FromDate: date,
                              }))
                            }
                            disabled={(date) =>
                              (filtersUserActivityLog.ToDate &&
                                date >
                                  new Date(
                                    filtersUserActivityLog.ToDate.getTime() - 24 * 60 * 60 * 1000,
                                  )) ||
                              date > new Date()
                            }
                            initialFocus
                          />
                        </PopoverContent>
                      </Popover>
                    </div>
                  ),
                },
                {
                  key: 'ToDate',
                  placeholder: t('ActivityLog.toDatePlaceholder'),
                  component: (
                    <div className="flex items-center space-x-[30px]">
                      <Popover>
                        <PopoverTrigger asChild>
                          <Button
                            variant="outline"
                            className="mt-1 w-full justify-start bg-transparent text-left font-normal"
                          >
                            <CalendarIcon className="mr-2 h-4 w-4" />
                            {filtersUserActivityLog.ToDate
                              ? format(filtersUserActivityLog.ToDate, 'dd/MM/yyyy', { locale: vi })
                              : t('ActivityLog.selectDate')}
                          </Button>
                        </PopoverTrigger>
                        <PopoverContent className="w-auto p-0" align="start">
                          <Calendar
                            mode="single"
                            selected={filtersUserActivityLog.ToDate}
                            onSelect={(date) =>
                              date &&
                              setFiltersUserActivityLog((prev: IUserActivityLogRequest) => ({
                                ...prev,
                                ToDate: date,
                              }))
                            }
                            disabled={(date) =>
                              (filtersUserActivityLog.FromDate &&
                                date <
                                  new Date(
                                    filtersUserActivityLog.FromDate.getTime() + 24 * 60 * 60 * 1000,
                                  )) ||
                              date > new Date(Date.now() + 24 * 60 * 60 * 1000)
                            }
                            initialFocus
                          />
                        </PopoverContent>
                      </Popover>
                    </div>
                  ),
                },
                {
                  key: 'RoleEnum',
                  placeholder: t('ActivityLog.allUsers'),
                  component: (
                    <Select
                      value={filtersUserActivityLog.RoleEnum?.toString() ?? '0'}
                      onValueChange={(value) => {
                        setFiltersUserActivityLog((prev) => ({
                          ...prev,
                          RoleEnum: Number(value) === 0 ? null : Number(value),
                          currentPage: 1,
                        }));
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder={t('ActivityLog.allUsers')} />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="0">{t('ActivityLog.allUsers')}</SelectItem>
                        <SelectItem value={RoleEnum.Administrator.toString()}>
                          {t('ActivityLog.admin')}
                        </SelectItem>
                        <SelectItem value={RoleEnum.Lecture.toString()}>
                          {t('ActivityLog.lecture')}
                        </SelectItem>
                        <SelectItem value={RoleEnum.Supervisor.toString()}>
                          {t('ActivityLog.supervisor')}
                        </SelectItem>
                        <SelectItem value={RoleEnum.Student.toString()}>
                          {t('ActivityLog.student')}
                        </SelectItem>
                      </SelectContent>
                    </Select>
                  ),
                },
              ]}
              onFilterChange={(
                newFilters: Record<string, string | number | boolean | null | undefined>,
              ) => {
                setFiltersUserActivityLog((prev) => ({
                  ...prev,
                  ...newFilters,
                  currentPage: 1,
                }));
              }}
              exportButtonText={t('ActivityLog.exportButton')}
              onExport={() => setIsExportModalOpen(true)}
            />
            <ActivityList activities={activities} />
            {totalPage >= 1 && renderPagination()}
          </div>
        </div>
      </PageWrapper>
      <ExportReportModal isOpen={isExportModalOpen} onClose={() => setIsExportModalOpen(false)} />
    </div>
  );
}
