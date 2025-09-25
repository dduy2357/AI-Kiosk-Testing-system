import { useEffect, useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Calendar } from '@/components/ui/calendar';
import { Popover, PopoverContent, PopoverTrigger } from '@/components/ui/popover';
import { CalendarIcon, Download, X, Loader2 } from 'lucide-react';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import useGetAllUserActivityLog from '@/services/modules/useractivitylog/hooks/useGetAllUserActivityLog';
import axios from 'axios';
import { ACTIVITY_LOG_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { showError, showSuccess } from '@/helpers/toast';
import { IUserActivityLogRequest } from '@/services/modules/useractivitylog/interfaces/useractivitylog.interface';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { RoleEnum } from '@/consts/common';

interface ExportReportModalProps {
  isOpen: boolean;
  onClose: () => void;
}

export function ExportReportModal({ isOpen, onClose }: Readonly<ExportReportModalProps>) {
  const [isLoading, setIsLoading] = useState(false);
  const [trigger, setTrigger] = useState(false);
  const token = httpService.getTokenStorage();
  const [dateRange, setDateRange] = useState<{
    from: Date | undefined;
    to: Date | undefined;
  }>({
    from: new Date(new Date().setDate(new Date().getDate() - 30)),
    to: new Date(),
  });

  const [filterByDay, setFilterByDay] = useState<IUserActivityLogRequest>({
    pageSize: 1000000,
    currentPage: 1,
    FromDate: dateRange.from,
    ToDate: dateRange.to,
    textSearch: '',
    RoleEnum: null,
  });

  const { data: listUserLog } = useGetAllUserActivityLog(filterByDay, { isTrigger: trigger });

  // Đồng bộ filterByDay với dateRange
  useEffect(() => {
    setFilterByDay((prev) => ({
      ...prev,
      FromDate: dateRange.from,
      ToDate: dateRange.to,
    }));
  }, [dateRange]);

  const handleExport = async () => {
    setIsLoading(true);
    setTrigger(true);
    const listLogId = listUserLog?.map((log) => log.logId) ?? [];

    if (listLogId.length === 0) {
      // showError("Không có dữ liệu để xuất báo cáo.");
      setIsLoading(false);
      return;
    }

    try {
      const response = await axios.post(`${ACTIVITY_LOG_URL}/export-log`, listLogId, {
        responseType: 'blob',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      const url = window.URL.createObjectURL(
        new Blob([response.data], {
          type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        }),
      );
      const link = document.createElement('a');
      link.href = url;
      const contentDisposition = response.headers['content-disposition'];
      let fileName = 'activity_log.xlsx';
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
      setIsLoading(false);
      onClose();
    }
    setTrigger(false);
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="max-h-[90vh] max-w-4xl overflow-y-auto">
        <DialogHeader className="border-b pb-4">
          <div className="flex items-center justify-between">
            <div>
              <DialogTitle className="text-xl font-semibold">Xuất báo cáo hoạt động</DialogTitle>
              <p className="mt-1 text-sm text-gray-500">
                Tùy chỉnh và xuất báo cáo theo nhu cầu của bạn
              </p>
            </div>
            <Button variant="ghost" size="sm" onClick={onClose}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        </DialogHeader>

        <div className="mt-6 grid grid-cols-2 gap-6">
          {/* Left Column - Configuration */}
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Bộ lọc</CardTitle>
                <p className="text-sm text-gray-500">Xuất báo cáo theo bộ lọc</p>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="gap-4">
                  <div>
                    <Label className="text-sm font-medium">Từ khóa</Label>
                    <Input
                      placeholder="Tìm theo tên, hành động..."
                      value={filterByDay.textSearch}
                      onChange={(e) =>
                        setFilterByDay((prev) => ({ ...prev, textSearch: e.target.value }))
                      }
                      className="pl-10"
                    />
                  </div>
                  <br />
                  <div>
                    <label className="mb-2 block text-sm font-medium text-gray-700">
                      Lịch sử hoạt động của
                    </label>
                    <Select
                      value={filterByDay.RoleEnum?.toString() ?? '0'}
                      onValueChange={(value) => {
                        setFilterByDay((prev) => ({
                          ...prev,
                          RoleEnum: Number(value) === 0 ? null : Number(value),
                        }));
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Tất cả người dùng" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="0">Tất cả người dùng</SelectItem>
                        <SelectItem value={RoleEnum.Administrator.toString()}>Admin</SelectItem>
                        <SelectItem value={RoleEnum.Lecture.toString()}>Lecture</SelectItem>
                        <SelectItem value={RoleEnum.Supervisor.toString()}>Supervisor</SelectItem>
                        <SelectItem value={RoleEnum.Student.toString()}>Student</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>
              </CardContent>
            </Card>
            <Card>
              <CardHeader>
                <CardTitle className="text-lg">Khoảng thời gian</CardTitle>
                <p className="text-sm text-gray-500">Chọn khoảng thời gian cần xuất báo cáo</p>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <Label className="text-sm font-medium">Từ ngày</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          className="mt-1 w-full justify-start bg-transparent text-left font-normal"
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {dateRange.from
                            ? format(dateRange.from, 'dd/MM/yyyy', { locale: vi })
                            : 'Chọn ngày'}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={dateRange.from}
                          onSelect={(date) => setDateRange((prev) => ({ ...prev, from: date }))}
                          disabled={(date) => date > new Date()}
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>

                  <div>
                    <Label className="text-sm font-medium">Đến ngày</Label>
                    <Popover>
                      <PopoverTrigger asChild>
                        <Button
                          variant="outline"
                          className="mt-1 w-full justify-start bg-transparent text-left font-normal"
                        >
                          <CalendarIcon className="mr-2 h-4 w-4" />
                          {dateRange.to
                            ? format(dateRange.to, 'dd/MM/yyyy', { locale: vi })
                            : 'Chọn ngày'}
                        </Button>
                      </PopoverTrigger>
                      <PopoverContent className="w-auto p-0" align="start">
                        <Calendar
                          mode="single"
                          selected={dateRange.to}
                          onSelect={(date) => setDateRange((prev) => ({ ...prev, to: date }))}
                          disabled={(date) =>
                            (dateRange.from &&
                              date < new Date(dateRange.from.getTime() + 24 * 60 * 60 * 1000)) ||
                            date > new Date(Date.now() + 24 * 60 * 60 * 1000)
                          }
                          initialFocus
                        />
                      </PopoverContent>
                    </Popover>
                  </div>
                </div>

                <div className="flex flex-wrap gap-2">
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      setDateRange({
                        from: new Date(new Date().setDate(new Date().getDate() - 7)),
                        to: new Date(),
                      })
                    }
                  >
                    7 ngày qua
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      setDateRange({
                        from: new Date(new Date().setDate(new Date().getDate() - 30)),
                        to: new Date(),
                      })
                    }
                  >
                    30 ngày qua
                  </Button>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={() =>
                      setDateRange({
                        from: new Date(new Date().setDate(new Date().getDate() - 90)),
                        to: new Date(),
                      })
                    }
                  >
                    3 tháng qua
                  </Button>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="space-y-6">
            <Card className="border-blue-200 bg-blue-50">
              <CardContent className="p-4">
                <h4 className="mb-2 font-medium text-blue-900">Tóm tắt báo cáo</h4>
                <div className="space-y-1 text-sm text-blue-800">
                  <p>
                    • Khoảng thời gian:{' '}
                    {dateRange.from && dateRange.to
                      ? `${format(dateRange.from, 'dd/MM/yyyy')} - ${format(dateRange.to, 'dd/MM/yyyy')}`
                      : 'Chưa chọn'}
                  </p>
                  <p>• Loại báo cáo: Chi tiết</p>
                  <p>• Định dạng: Excel</p>
                  <p>• Bộ lọc: </p>
                  {filterByDay.textSearch && (
                    <p>&nbsp;&nbsp;&nbsp;&nbsp; - Từ khóa: {filterByDay.textSearch}</p>
                  )}
                  {filterByDay.RoleEnum && (
                    <p>&nbsp;&nbsp;&nbsp;&nbsp; - Vai trò: {RoleEnum[filterByDay.RoleEnum]}</p>
                  )}
                </div>
              </CardContent>
            </Card>
          </div>
        </div>

        <div className="flex items-center justify-between border-t pt-6">
          <Button variant="outline" onClick={onClose}>
            Hủy bỏ
          </Button>
          <div className="flex space-x-3">
            <Button
              onClick={handleExport}
              className="bg-blue-600 hover:bg-blue-700"
              disabled={isLoading}
            >
              {isLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Download className="mr-2 h-4 w-4" />
              )}
              Xuất báo cáo
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
