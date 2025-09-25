import { Button } from '@/components/ui/button';
import { TEACHER_EXAM_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import axios from 'axios';
import { BookOpen, Calendar, Clock, Download, FileText, Filter, Target, User } from 'lucide-react';

interface ExamData {
  title: string;
  subtitle: string;
  subject: string;
  date: string;
  duration: number;
  totalQuestions: number;
  maxScore: number;
  teacher: string;
}

interface ExamHeaderProps {
  examData: ExamData;
  examId: string;
}

export function ExamHeader({ examData, examId }: Readonly<ExamHeaderProps>) {
  const token = httpService.getTokenStorage();

  const getExportFile = async () => {
    try {
      const response = await axios.get(`${TEACHER_EXAM_URL}/exams/${examId}/export-results`, {
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
      let fileName = 'exam_results.xlsx';
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
    } catch (error) {
      console.error('Error exporting file:', error);
    }
  };
  return (
    <div className="border-b border-gray-200 bg-white p-6">
      <div className="mb-6 flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">{examData.title}</h1>
          <p className="mt-1 text-gray-600">{examData.subtitle}</p>
        </div>
        <div className="flex space-x-3">
          <Button variant="outline" className="flex items-center space-x-2 bg-transparent">
            <Filter className="h-4 w-4" />
            <span>Bộ lọc nâng cao</span>
          </Button>
          <Button
            onClick={getExportFile}
            className="flex items-center space-x-2 bg-black text-white"
          >
            <Download className="h-4 w-4" />
            <span>Xuất báo cáo</span>
          </Button>
        </div>
      </div>

      <div className="grid grid-cols-6 gap-6">
        <div className="flex items-center space-x-3">
          <BookOpen className="h-5 w-5 text-gray-500" />
          <div>
            <p className="text-sm text-gray-500">Môn học</p>
            <p className="font-medium">{examData.subject}</p>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <Calendar className="h-5 w-5 text-gray-500" />
          <div>
            <p className="text-sm text-gray-500">Ngày thi</p>
            <p className="font-medium">{examData.date}</p>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <Clock className="h-5 w-5 text-gray-500" />
          <div>
            <p className="text-sm text-gray-500">Thời gian</p>
            <p className="font-medium">{examData.duration}</p>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <FileText className="h-5 w-5 text-gray-500" />
          <div>
            <p className="text-sm text-gray-500">Số câu hỏi</p>
            <p className="font-medium">{examData.totalQuestions}</p>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <Target className="h-5 w-5 text-gray-500" />
          <div>
            <p className="text-sm text-gray-500">Tổng điểm</p>
            <p className="font-medium">{examData.maxScore}</p>
          </div>
        </div>

        <div className="flex items-center space-x-3">
          <User className="h-5 w-5 text-gray-500" />
          <div>
            <p className="text-sm text-gray-500">Giáo viên</p>
            <p className="font-medium">{examData.teacher}</p>
          </div>
        </div>
      </div>
    </div>
  );
}
