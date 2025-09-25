import { STUDENT_EXAM_URL, TEACHER_EXAM_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';

class ExamResultDetailService {
  getHistoryExamDetail(studentExamId: any) {
    return httpService.get(`${STUDENT_EXAM_URL}/history-exam-detail/${studentExamId}`);
  }

  getResultDetailForTeacher(examId: string) {
    return httpService.get(`${TEACHER_EXAM_URL}/${examId}/detail`);
  }
}

export default new ExamResultDetailService();
