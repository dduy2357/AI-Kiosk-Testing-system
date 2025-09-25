import { LOG_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import {
  IUserLogListRequest,
  WriteExamActivity,
  WriteUserActivity,
} from './interfaces/log.interface';
import { AxiosRequestConfig } from 'axios';

class LogService {
  writeUserActivity(body: WriteUserActivity) {
    return httpService.post(`${LOG_URL}/write-user-activity`, body);
  }

  writeExamActivity(body: WriteExamActivity) {
    return httpService.post(`${LOG_URL}/write-exam-activity`, body);
  }

  getUserLogList(filter: IUserLogListRequest, config: AxiosRequestConfig) {
    const params = new URLSearchParams({
      PageSize: (filter?.PageSize ?? 50).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
    });

    if (filter?.FromDate !== undefined) {
      params.append('FromDate', filter.FromDate.toISOString());
    }
    if (filter?.ToDate !== undefined) {
      params.append('ToDate', filter.ToDate.toISOString());
    }
    if (filter?.ActionType !== undefined) {
      params.append('ActionType', filter.ActionType);
    }
    if (filter?.LogStatus !== undefined) {
      params.append('LogStatus', filter.LogStatus.toString());
    }

    return httpService.get(`${LOG_URL}/get-user-log-list?${params.toString()}`, config);
  }
}

export default new LogService();
