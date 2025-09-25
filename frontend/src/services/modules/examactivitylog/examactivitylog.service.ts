import { AxiosRequestConfig } from 'axios';
import {
  IExamActivityLogRequest,
  ResponseExamActivityLog,
} from './interfaces/examactivitylog.interface';
import httpService from '@/services/httpService';
import { ACTIVITY_LOG_URL } from '@/consts/apiUrl';

class ExamActivityLog {
  getListExamActivityLog(
    filter: IExamActivityLogRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseExamActivityLog> {
    const queryParams = new URLSearchParams();

    // Only add parameters that are not null or undefined
    if (filter?.studentExamId != null) queryParams.append('studentExamId', filter.studentExamId);
    if (filter?.pageSize != null) queryParams.append('PageSize', filter.pageSize.toString());
    if (filter?.currentPage != null)
      queryParams.append('CurrentPage', filter.currentPage.toString());
    if (filter?.textSearch != null) queryParams.append('TextSearch', filter.textSearch);

    const queryString = queryParams.toString();
    const url = queryString
      ? `${ACTIVITY_LOG_URL}/get-exam-log-list?${queryString}`
      : `${ACTIVITY_LOG_URL}/get-exam-log-list`;

    return httpService.get(url, config);
  }

  getOneExamLog(logId: string) {
    return httpService.get(`${ACTIVITY_LOG_URL}/get-exam-log?logId=${logId}`);
  }

  exportExamLog(logId: string[]) {
    return httpService.post(`${ACTIVITY_LOG_URL}/export-exam-log`, logId);
  }
}

export default new ExamActivityLog();
