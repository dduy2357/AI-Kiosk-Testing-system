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
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetListFeedback from '@/services/modules/feedback/hooks/useGetListFeedback';
import { IFeedbackRequest } from '@/services/modules/feedback/interfaces/feedback.interface';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { CalendarIcon, MessageSquare } from 'lucide-react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { GenericFilters } from '../manageuser/components/generic-filters';
import FeedbackCard from './components/FeedbackCard';
export default function FeedbackList() {
  const { t } = useTranslation('shared');
  const [filters, setFilters] = useState<IFeedbackRequest>({
    dateFrom: undefined,
    dateTo: undefined,
    pageSize: 5,
    currentPage: 1,
    textSearch: '',
  });

  const { data, totalPage, loading } = useGetListFeedback(filters, {});

  const currentPage = filters.currentPage;

  const handlePageChange = (page: number) => {
    if (page > 0 && page <= totalPage) {
      setFilters((prev) => ({
        ...prev,
        currentPage: page,
      }));
    }
  };

  const handlePageSizeChange = (value: string) => {
    const newPageSize = parseInt(value);
    setFilters((prev) => ({
      ...prev,
      pageSize: newPageSize,
      currentPage: 1,
    }));
  };

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
        <Select value={filters.pageSize.toString()} onValueChange={handlePageSizeChange}>
          <SelectTrigger className="w-[100px]">
            <SelectValue placeholder="Page size" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="5">5</SelectItem>
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
          aria-label="Go to previous page"
        >
          Previous
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
          aria-label="Go to next page"
        >
          Next
        </Button>
      </div>
    );
  };

  return (
    <PageWrapper
      name={t('AdminFeedback.Title')}
      className="bg-gradient-to-br from-slate-50 to-blue-50/30"
      isLoading={loading}
    >
      <div className="space-y-6">
        <ExamHeader
          title={t('AdminFeedback.Title')}
          subtitle={t('AdminFeedback.Subtitle')}
          icon={<MessageSquare className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />
        <div className="space-y-8 p-6">
          <GenericFilters
            className="md:grid-cols-3 lg:grid-cols-4"
            searchPlaceholder={t('AdminFeedback.Search')}
            onSearch={(e) =>
              setFilters((prev: IFeedbackRequest) => ({ ...prev, textSearch: e.textSearch ?? '' }))
            }
            initialSearchQuery={filters.textSearch}
            filters={[
              {
                key: 'dateFrom',
                placeholder: t('AdminFeedback.DateFrom'),
                component: (
                  <div className="flex items-center space-x-[30px]">
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          className="mt-1 w-full justify-start bg-transparent text-left font-normal"
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {filters.dateFrom
                            ? format(filters.dateFrom, 'dd/MM/yyyy', { locale: vi })
                            : t('AdminFeedback.DateFrom')}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={filters.dateFrom}
                          onSelect={(date) =>
                            date &&
                            setFilters((prev: IFeedbackRequest) => ({ ...prev, dateFrom: date }))
                          }
                          disabled={(date) => date > new Date()}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                ),
              },
              {
                key: 'dateTo',
                placeholder: t('AdminFeedback.DateTo'),
                component: (
                  <div className="flex items-center space-x-[30px]">
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          className="mt-1 w-full justify-start bg-transparent text-left font-normal"
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {filters.dateTo
                            ? format(filters.dateTo, 'dd/MM/yyyy', { locale: vi })
                            : t('AdminFeedback.DateTo')}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={filters.dateTo}
                          onSelect={(date) => {
                            if (date) {
                              setFilters((prev: IFeedbackRequest) => ({
                                ...prev,
                                dateTo: date,
                                currentPage: 1,
                              }));
                            }
                          }}
                          disabled={(date) =>
                            (filters.dateFrom &&
                              date < new Date(filters.dateFrom.getTime() + 24 * 60 * 60 * 1000)) ??
                            date > new Date(Date.now() + 24 * 60 * 60 * 1000)
                          }
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                ),
              },
            ]}
            onFilterChange={(
              newFilters: Record<string, string | number | boolean | null | undefined>,
            ) => {
              setFilters((prev) => {
                const updatedFilters = {
                  ...prev,
                  ...newFilters,
                  currentPage: 1, // Reset to page 1 on filter change
                };
                return updatedFilters;
              });
            }}
          />

          {data && data.length > 0 && <FeedbackCard feedbacks={data} />}

          {totalPage >= 1 && renderPagination()}
        </div>
      </div>
    </PageWrapper>
  );
}
