import { useState } from 'react';
import { Search, Eye, FileText } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { IViolationRequest } from '@/services/modules/violation/interfaces/violation.interface';
import { useParams } from 'react-router-dom';
import useGetListViolation from '@/services/modules/violation/hooks/useGetListViolation';
import { convertUTCToVietnamTime } from '@/helpers/common';
import useToggleDialog from '@/hooks/useToggleDialog';
import { DateTimeFormat } from '@/consts/dates';
import DialogViolationDetail from '../dialogs/DialogViolationDetail';
import DialogCreateViolation from '../dialogs/DialogCreateViolation';
import { useTranslation } from 'react-i18next';

export default function ViolationList() {
  //!State
  const { t } = useTranslation('shared');
  const [searchTerm, setSearchTerm] = useState('');
  const { examId, studentExamId } = useParams();
  const [selectedVioId, setSelectVioId] = useState('');
  const [open, toggle, shouldRender] = useToggleDialog();
  const [isOpenCreateViolation, toggleOpenCreateViolation, shouldRenderOpenCreateViolation] =
    useToggleDialog();
  const [filters, setFilters] = useState<IViolationRequest>({
    ExamId: examId,
    StudentExamId: studentExamId,
    PageSize: 5,
    CurrentPage: 1,
    TextSearch: '',
  });

  const { data: violationList, totalPage, refetch } = useGetListViolation(filters, {});

  const currentPage = filters.CurrentPage;

  const handleToggleShowDetail = () => {
    toggle();
  };

  //!Functions
  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchTerm(e.target.value);
    setFilters((prev) => ({
      ...prev,
      TextSearch: e.target.value,
      CurrentPage: 1,
    }));
  };

  const handlePageChange = (page: number) => {
    if (page > 0 && page <= totalPage) {
      setFilters((prev) => ({
        ...prev,
        CurrentPage: page,
      }));
    }
  };

  //!Render
  return (
    <div className="mx-auto max-w-7xl p-6">
      <div className="mb-6">
        <div className="mb-5 flex w-full items-center justify-between">
          <h1 className="mb-4 text-2xl font-bold text-gray-900">
            {t('ExamSupervision.ViolationListTitle')}
          </h1>
          <Button
            className="bg-blue-600 hover:bg-blue-700"
            onClick={() => toggleOpenCreateViolation()}
          >
            <FileText className="mr-2 h-4 w-4" />
            {t('ExamSupervision.ReportViolation')}
          </Button>
        </div>
        <div className="mb-6 flex flex-col gap-4 sm:flex-row">
          <div className="relative flex-1">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
            <Input
              placeholder={t('ExamSupervision.SearchPlaceholder')}
              value={searchTerm}
              onChange={handleSearch}
              className="pl-10"
            />
          </div>
        </div>
      </div>

      <div className="overflow-hidden rounded-lg border">
        <Table>
          <TableHeader>
            <TableRow className="bg-gray-50">
              <TableHead className="w-16 text-center font-semibold">
                {t('ExamSupervision.STT')}
              </TableHead>
              <TableHead className="font-semibold">{t('ExamSupervision.StudentName')}</TableHead>
              <TableHead className="font-semibold">{t('ExamSupervision.StudentCode')}</TableHead>
              <TableHead className="font-semibold">{t('ExamSupervision.ViolationType')}</TableHead>
              <TableHead className="font-semibold">{t('ExamSupervision.Content')}</TableHead>
              <TableHead className="font-semibold">{t('ExamSupervision.ReportDate')}</TableHead>
              <TableHead className="font-semibold">{t('ExamSupervision.Reporter')}</TableHead>
              <TableHead className="w-20 text-center font-semibold">
                {t('ExamSupervision.Detail')}
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {violationList?.map((violation, index) => (
              <TableRow key={violation?.violationId} className="hover:bg-gray-50">
                <TableCell className="text-center font-medium">
                  {(filters.CurrentPage - 1) * filters.PageSize + index + 1}
                </TableCell>
                <TableCell className="font-medium">{violation?.studentName}</TableCell>
                <TableCell className="text-center font-medium">{violation?.studentCode}</TableCell>
                <TableCell>{violation?.violationName}</TableCell>
                <TableCell>{violation?.message}</TableCell>
                <TableCell>
                  {convertUTCToVietnamTime(
                    violation?.createdAt,
                    DateTimeFormat?.DateTime,
                  )?.toString()}
                </TableCell>
                <TableCell>{violation?.creatorName}</TableCell>
                <TableCell className="text-center">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 w-8 p-0"
                    onClick={() => {
                      setSelectVioId(violation?.violationId);
                      toggle();
                    }}
                  >
                    <Eye className="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      <div className="mt-4 text-sm text-gray-600">
        Hiện thị {violationList.length} trong {totalPage} trang. Số hàng:
        <select
          value={filters.PageSize}
          onChange={(e) =>
            setFilters((prev) => ({ ...prev, PageSize: Number(e.target.value), CurrentPage: 1 }))
          }
          className="ml-2 rounded border px-2 py-1"
        >
          <option value="5">5</option>
          <option value="10">10</option>
          <option value="20">20</option>
          <option value="50">50</option>
        </select>
      </div>

      {totalPage > 1 && (
        <div className="mt-4 flex justify-center">
          <Button
            variant="outline"
            onClick={() => handlePageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="mr-2"
          >
            Previous
          </Button>
          {Array.from({ length: totalPage }, (_, i) => i + 1).map((page) => (
            <Button
              key={page}
              variant={currentPage === page ? 'default' : 'outline'}
              onClick={() => handlePageChange(page)}
              className="mx-1"
            >
              {page}
            </Button>
          ))}
          <Button
            variant="outline"
            onClick={() => handlePageChange(currentPage + 1)}
            disabled={currentPage === totalPage}
            className="ml-2"
          >
            Next
          </Button>
        </div>
      )}

      {shouldRender && (
        <DialogViolationDetail
          isOpen={open}
          toggle={handleToggleShowDetail}
          violationId={selectedVioId}
        />
      )}
      {shouldRenderOpenCreateViolation && (
        <DialogCreateViolation
          isOpen={isOpenCreateViolation}
          toggle={toggleOpenCreateViolation}
          studentExamId={studentExamId}
          refetch={refetch}
        />
      )}
    </div>
  );
}
