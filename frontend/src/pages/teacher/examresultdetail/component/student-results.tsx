import { useState } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Badge } from '@/components/ui/badge';
import { Check, Search, X } from 'lucide-react';
import {
  ResultDetail,
  StudentResultDetail,
} from '@/services/modules/examresultdetail/interfaces/examresultdetail.interface';
import { essay } from '@/consts/common';
import { useNavigate } from 'react-router-dom';
import BaseUrl from '@/consts/baseUrl';

interface StudentResultsProps {
  data: StudentResultDetail[];
  questionType?: string;
  resultDetail?: ResultDetail;
}

export function StudentResults({
  data,
  questionType,
  resultDetail,
}: Readonly<StudentResultsProps>) {
  //!State
  const [searchTerm, setSearchTerm] = useState('');
  const [classFilter, setClassFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');
  const navigate = useNavigate();

  const filteredData = data.filter((student) => {
    const matchesSearch = student.fullname.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesClass = classFilter === 'all' || student.className === classFilter;
    const matchesStatus = statusFilter === 'all' || student.status === statusFilter;
    return matchesSearch && matchesClass && matchesStatus;
  });

  const getScoreColor = (score: number) => {
    if (score >= 9) return 'text-green-600 font-semibold';
    if (score >= 8) return 'text-blue-600 font-semibold';
    if (score >= 7) return 'text-yellow-600 font-semibold';
    if (score >= 6) return 'text-orange-600 font-semibold';
    return 'text-red-600 font-semibold';
  };

  const handleNextPage = (
    examType: number | undefined,
    examId: string | undefined,
    studentExamId: string | undefined,
  ) => {
    if (examType === 0) {
      navigate(`${BaseUrl.GradeEssay}/${examId}/${studentExamId}`);
    }
    if (examType === 1) {
      navigate(`${BaseUrl.TeacherExamResultDetail}/${studentExamId}`);
    }
  };

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle>Kết quả chi tiết học sinh</CardTitle>
          <div className="flex space-x-3">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
              <Input
                placeholder="Tìm kiếm học sinh..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-64 pl-10"
              />
            </div>
            <Select value={classFilter} onValueChange={setClassFilter}>
              <SelectTrigger className="w-32">
                <SelectValue placeholder="Tất cả lớp" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả lớp</SelectItem>
                <SelectItem value="12A1">12A1</SelectItem>
                <SelectItem value="12A2">12A2</SelectItem>
                <SelectItem value="12B1">12B1</SelectItem>
                <SelectItem value="12B2">12B2</SelectItem>
              </SelectContent>
            </Select>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-32">
                <SelectValue placeholder="Tất cả" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả</SelectItem>
                <SelectItem value="Hoàn thành">Hoàn thành</SelectItem>
                <SelectItem value="Chưa hoàn thành">Chưa hoàn thành</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="px-4 py-3 text-left font-medium text-gray-500">STT</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500">Họ và tên</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500">Lớp</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500">Điểm số</th>
                {questionType === essay ? (
                  <th className="px-4 py-3 text-left font-medium text-gray-500">Đã chấm</th>
                ) : null}
                <th className="px-4 py-3 text-left font-medium text-gray-500">Thời gian làm bài</th>
                <th className="px-4 py-3 text-left font-medium text-gray-500">Giờ nộp bài</th>
                {/* <th className="text-left py-3 px-4 font-medium text-gray-500">Xếp hạng</th> */}
                <th className="px-4 py-3 text-left font-medium text-gray-500">Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              {filteredData.map((student, index) => (
                <tr
                  key={index}
                  className="border-b border-gray-100 hover:cursor-pointer hover:bg-gray-50"
                  onClick={() =>
                    handleNextPage(
                      resultDetail?.examType,
                      resultDetail?.examId,
                      student.studentExamId,
                    )
                  }
                >
                  <td className="px-4 py-4 text-gray-900">{index + 1}</td>
                  <td className="px-4 py-4 font-medium text-gray-900">{student.fullname}</td>
                  <td className="px-4 py-4 text-gray-600">{student.className}</td>
                  <td className={`px-4 py-4 ${getScoreColor(student.score)}`}>{student.score}</td>
                  {questionType === essay ? (
                    <td className="px-4 py-4 text-gray-600">
                      {typeof student?.score === 'number' ? (
                        <span className="flex items-center text-green-600">
                          <Check className="mr-1 h-4 w-4" />
                        </span>
                      ) : (
                        <span className="flex items-center text-red-600">
                          <X className="h-4 w-4" />
                        </span>
                      )}
                    </td>
                  ) : null}

                  <td className="px-4 py-4 text-gray-600">{student.workingTime}</td>
                  <td className="px-4 py-4 text-gray-600">{student.submitTime}</td>
                  {/* <td className="py-4 px-4">{getRankBadge(student.rank)}</td> */}
                  <td className="px-4 py-4">
                    <Badge
                      variant="outline"
                      className="border-green-200 bg-green-50 text-green-600"
                    >
                      ✓ {student.status}
                    </Badge>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </CardContent>
    </Card>
  );
}
