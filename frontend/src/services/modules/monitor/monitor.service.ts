import { MONITOR_URL } from '@/consts/apiUrl';
import { AssignMoreTimeValues } from '@/pages/teacher/examsupervision/dialogs/DialogAssignMoreTime';
import httpService from '@/services/httpService';
import { AxiosRequestConfig } from 'axios';
import { IMonitorRequest } from './interfaces/monitor.interface';
import { IMonitorDetailRequest } from './interfaces/monitorDetail.interface';

interface ReAssignExamInterface {
  examId: string;
  studentId: string;
}

interface ReAssignExamInterfaces {
  examId: string;
  studentIds: string[];
}

interface FinishStudentExamInterface {
  examId: string;
  studentExamId: string;
}

interface FinishExamInterface {
  examId: string;
}

class MonitorService {
  getListMonitor(filter?: IMonitorRequest, config?: AxiosRequestConfig) {
    const queryParams = new URLSearchParams({
      PageSize: (filter?.PageSize ?? 50).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
      SubjectId: filter?.SubjectId || '',
    });

    if (filter?.ExamStatus !== undefined) {
      queryParams.append('ExamStatus', filter.ExamStatus.toString());
    }

    return httpService.get(`${MONITOR_URL}/exam-overview?${queryParams.toString()}`, config);
  }

  examMonitorDetail(filters: IMonitorDetailRequest, examId: string, config?: AxiosRequestConfig) {
    const params = new URLSearchParams({
      PageSize: (filters?.PageSize ?? 10).toString(),
      CurrentPage: (filters?.CurrentPage ?? 1).toString(),
      TextSearch: filters?.TextSearch || '',
    });

    if (filters?.StudentExamStatus !== null && filters?.StudentExamStatus !== undefined) {
      params.append('StudentExamStatus', filters.StudentExamStatus.toString());
    }

    return httpService.get(
      `${MONITOR_URL}/exam-monitor-detail?ExamId=${examId}&${params.toString()}`,
      config,
    );
  }

  assignMoreTime(data: AssignMoreTimeValues) {
    return httpService.post(`${MONITOR_URL}/add-student-extra-time`, data);
  }

  finishStudentExam(data: FinishStudentExamInterface) {
    return httpService.post(`${MONITOR_URL}/finish-student-exam`, data);
  }

  finishExam(data: FinishExamInterface) {
    return httpService.post(`${MONITOR_URL}/finish-exam`, data);
  }

  reAssignExam(data: ReAssignExamInterface) {
    return httpService.post(`${MONITOR_URL}/re-assign-student`, data);
  }

  reAssignExams(data: ReAssignExamInterfaces) {
    return httpService.post(`${MONITOR_URL}/re-assign-students`, data);
  }
}

export default new MonitorService();
