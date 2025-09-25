import { AxiosRequestConfig } from 'axios';
import {
  ITeacherExamRequest,
  ResponseGetListTeacherExam,
} from './interfaces/teacherexamm.interface';
import httpService from '../httpService';
import { TEACHER_EXAM_URL } from '@/consts/apiUrl';

class TeacherExamService {
  getAllTeacherExam(
    filter: ITeacherExamRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseGetListTeacherExam> {
    const params = new URLSearchParams({
      pageSize: filter.pageSize.toString(),
      currentPage: filter.currentPage.toString(),
      textSearch: filter.textSearch || '',
    });

    if (filter.status !== undefined) {
      params.append('status', filter.status.toString());
    }
    if (filter.IsMyQuestion !== undefined) {
      params.append('IsMyQuestion', filter.IsMyQuestion.toString());
    }

    if (filter.IsExamResult !== undefined) {
      params.append('IsExamResult', filter.IsExamResult.toString());
    }

    return httpService.get(`${TEACHER_EXAM_URL}/list?${params.toString()}`, config);
  }

  getResultReport(examId: string) {
    return httpService.get(`${TEACHER_EXAM_URL}/${examId}/ResultReport`);
  }

  getResultDetail(examId: string) {
    return httpService.get(`${TEACHER_EXAM_URL}/${examId}/detail`);
  }
  getExportResult(examId: string) {
    return httpService.get(`${TEACHER_EXAM_URL}/exams/${examId}/export-results`);
  }
}

export default new TeacherExamService();
