import { useState, useEffect } from 'react';
import { ExamHeader } from '../components/logComponent/exam-header';
import { ExamActivity } from '../components/logComponent/exam-activity';
import { useParams } from 'react-router-dom';
import useGetListExamActivityLog from '@/services/modules/examactivitylog/hooks/useGetAllExamActivityLog';
import { SystemInfo } from '../components/logComponent/system-info';
import { showError, showSuccess } from '@/helpers/toast';
import { ACTIVITY_LOG_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import axios from 'axios';
import { IExamActivityLogRequest } from '@/services/modules/examactivitylog/interfaces/examactivitylog.interface';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

export default function ExamActivityLog() {
  const [timeRemaining, setTimeRemaining] = useState(0); // 45 minutes in seconds
  const [isMonitoring] = useState(true);
  const { studentExamId } = useParams();
  const [isExporting, setIsExporting] = useState(false);
  const token = httpService.getTokenStorage();

  const [filterExamLog, setFilterExamLog] = useState<IExamActivityLogRequest>({
    studentExamId: studentExamId,
    pageSize: 10,
    currentPage: 1,
    textSearch: '',
  });

  const { data: listExamLogActivity, totalPage } = useGetListExamActivityLog(filterExamLog, {});

  // Countdown timer
  useEffect(() => {
    if (timeRemaining > 0 && isMonitoring) {
      const timer = setTimeout(() => setTimeRemaining(timeRemaining - 1), 1000);
      return () => clearTimeout(timer);
    }
  }, [timeRemaining, isMonitoring]);

  const studentInfo = {
    name: listExamLogActivity[0]?.fullName ?? '',
    studentId: listExamLogActivity[0]?.userCode ?? '',
  };

  const handleExportLog = async () => {
    setIsExporting(true);
    try {
      const response = await axios.post(
        `${ACTIVITY_LOG_URL}/export-exam-log?studentExamId=${studentExamId}`,
        {},
        {
          responseType: 'blob',
          headers: {
            Authorization: `Bearer ${token}`,
          },
        },
      );
      const url = window.URL.createObjectURL(
        new Blob([response.data], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }),
      );
      const link = document.createElement('a');
      link.href = url;
      const contentDisposition = response.headers['content-disposition'];
      let fileName = 'activity_exam_log.xlsx';
      if (contentDisposition) {
        const fileNameMatch = contentDisposition.match(/filename="(.+)"/);
        if (fileNameMatch && fileNameMatch[1]) {
          fileName = fileNameMatch[1];
        }
      }
      link.setAttribute('download', fileName);
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      showSuccess('Xuất báo cáo thành công!');
    } catch (error) {
      console.error('Error exporting file:', error);
      showError('Lỗi khi xuất báo cáo. Vui lòng thử lại sau.');
    } finally {
      setIsExporting(false);
    }
  };
  const currentPage = filterExamLog.currentPage;
  const handlePageChange = (page: number) => {
    if (page > 0 && page <= totalPage) {
      setFilterExamLog((prev) => ({
        ...prev,
        currentPage: page,
      }));
    }
  };

  const handlePageSizeChange = (value: string) => {
    const newPageSize = parseInt(value);
    setFilterExamLog((prev) => ({
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
        <Select value={filterExamLog.pageSize.toString()} onValueChange={handlePageSizeChange}>
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
    <div className="min-h-screen bg-gray-50">
      <ExamHeader studentInfo={studentInfo} onExport={handleExportLog} isExporting={isExporting} />

      <div className="p-6">
        <div className="mt-6 grid grid-cols-3 gap-6">
          <div className="col-span-2 space-y-6">
            <ExamActivity activities={listExamLogActivity} />
            {totalPage >= 1 && renderPagination()}
          </div>
          <div>
            <SystemInfo examLogId={listExamLogActivity[0]?.examLogId} />
          </div>
        </div>
      </div>
    </div>
  );
}
