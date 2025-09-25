import { TEACHER_EXAM_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import {
  IAssignOtpToExam,
  IManageExamFormValue,
  IManageExamRequest,
} from './interfaces/manageExam.interface';
import { ActiveStatusExamTeacher } from '@/consts/common';

class ManageExamService {
  addNewExam(examData: IManageExamFormValue) {
    return httpService.post(`${TEACHER_EXAM_URL}/add`, examData, {});
  }

  assignOtpToExam(dataOtp: IAssignOtpToExam) {
    return httpService.post(`${TEACHER_EXAM_URL}/assign-otp`, dataOtp, {});
  }

  getManageExamList(filter: IManageExamRequest) {
    const params = new URLSearchParams({
      PageSize: (filter?.pageSize ?? 50).toString(),
      CurrentPage: (filter?.currentPage ?? 1).toString(),
      TextSearch: filter?.textSearch || '',
    });

    if (filter?.status !== undefined) {
      params.append('Status', filter.status.toString());
    }

    if (filter?.isMyQuestion !== undefined) {
      params.append('IsMyQuestion', filter.isMyQuestion.toString());
    }

    return httpService.get(`${TEACHER_EXAM_URL}/list?${params.toString()}`, {});
  }

  getExamDetail(ExamId: string) {
    return httpService.get(`${TEACHER_EXAM_URL}/${ExamId}/detail`, {});
  }

  updateExam(examData: IManageExamFormValue) {
    return httpService.put(`${TEACHER_EXAM_URL}/update`, examData, {});
  }

  viewGuideLines(examId: string) {
    return httpService.get(`${TEACHER_EXAM_URL}/guidelines/${examId}`, {});
  }

  changeExamStatus(examId: string, newStatus: ActiveStatusExamTeacher) {
    return httpService.post(`${TEACHER_EXAM_URL}/${examId}/status`, { newStatus }, {});
  }
}

export default new ManageExamService();
