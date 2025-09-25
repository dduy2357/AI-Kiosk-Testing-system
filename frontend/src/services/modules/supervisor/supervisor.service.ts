import { EXAM_SUPERVISOR_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { AxiosRequestConfig } from 'axios';
import {
  IListExamSupervisorRequest,
  ISupervisorAssign,
  ISupervisorRequest,
} from './interfaces/supervisor.interface';

class SupervisorService {
  getAllSupervisor(examId: string, config: AxiosRequestConfig) {
    return httpService.get(`${EXAM_SUPERVISOR_URL}/get-all?examId=${examId}`, config);
  }

  assignSupervisor(data: ISupervisorAssign) {
    return httpService.post(`${EXAM_SUPERVISOR_URL}/assign-supervisor`, data);
  }

  getExams(filter: IListExamSupervisorRequest, config: AxiosRequestConfig) {
    return httpService.get(
      `${EXAM_SUPERVISOR_URL}/get-exams?PageSize=${filter.PageSize}&CurrentPage=${filter.CurrentPage}&TextSearch=${filter.TextSearch}`,
      config,
    );
  }

  getSupervisors(filter: ISupervisorRequest, config: AxiosRequestConfig) {
    return httpService.get(
      `${EXAM_SUPERVISOR_URL}/get-supervisors?PageSize=${filter.PageSize}&CurrentPage=${filter.CurrentPage}&TextSearch=${filter.TextSearch}`,
      config,
    );
  }

  deleteSupervisor(data: ISupervisorAssign) {
    return httpService.post(`${EXAM_SUPERVISOR_URL}/Remove`, data);
  }
}

export default new SupervisorService();
