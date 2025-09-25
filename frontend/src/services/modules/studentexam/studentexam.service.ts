import { STUDENT_EXAM_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { AxiosRequestConfig } from 'axios';
import {
  Answer,
  FormEssayValue,
  IHistoryExamRequest,
  ResponseGetListHistoryExam,
} from './interfaces/studentexam.interface';

class StudentExamService {
  getListExams(config: AxiosRequestConfig) {
    return httpService.get(`${STUDENT_EXAM_URL}/list-exams`, config);
  }

  accessExam(examId: string, otpCode: number) {
    return httpService.post(`${STUDENT_EXAM_URL}/access-exam`, { examId, otpCode });
  }

  getHistoryExams(
    filter: IHistoryExamRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseGetListHistoryExam> {
    return httpService.get(
      `${STUDENT_EXAM_URL}/history-exams?pageSize=${filter.pageSize}&currentPage=${filter.currentPage}&textSearch=${filter.textSearch}`,
      config,
    );
  }

  submitExam(examId: string, answers: Answer[], studentExamId: string) {
    return httpService.post(`${STUDENT_EXAM_URL}/submit-exam`, {
      examId,
      answers,
      studentExamId,
    });
  }

  saveAnswerTemporary(examId: string, answers: Answer[], studentExamId: string) {
    return httpService.post(`${STUDENT_EXAM_URL}/save-answer-temporary`, {
      examId,
      answers,
      studentExamId,
    });
  }

  examDetailById(examId: string) {
    return httpService.get(`${STUDENT_EXAM_URL}/exam-detail-by-id/${examId}`);
  }

  getSavedAnswers(examId: string) {
    return httpService.get(`${STUDENT_EXAM_URL}/get-saved-answers/${examId}`);
  }

  getEssayExam(studentExamId: string, examId: string) {
    return httpService.get(`${STUDENT_EXAM_URL}/essay-exam/${studentExamId}/${examId}`);
  }

  markEssay(data: FormEssayValue) {
    return httpService.post(`${STUDENT_EXAM_URL}/mark-essay`, data);
  }
}

export default new StudentExamService();
