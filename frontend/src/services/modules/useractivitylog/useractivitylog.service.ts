import { AxiosRequestConfig } from 'axios';
import {
  IUserActivityLogRequest,
  ResponseUserActivityLog,
} from './interfaces/useractivitylog.interface';
import httpService from '@/services/httpService';
import { ACTIVITY_LOG_URL } from '@/consts/apiUrl';

class UserActivityLog {
  getUserActivityLog(
    filter: IUserActivityLogRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseUserActivityLog> {
    const queryParams = new URLSearchParams();

    // Only add parameters that are not null or undefined
    if (filter?.logStatus != null) queryParams.append('logStatus', filter.logStatus.toString());
    if (filter?.ActionType != null) queryParams.append('ActionType', filter.ActionType);
    if (filter?.FromDate != null) queryParams.append('FromDate', filter.FromDate.toISOString());
    if (filter?.ToDate != null) queryParams.append('ToDate', filter.ToDate.toISOString());
    if (filter?.RoleEnum != null) queryParams.append('RoleEnum', filter.RoleEnum.toString());
    if (filter?.pageSize != null) queryParams.append('PageSize', filter.pageSize.toString());
    if (filter?.currentPage != null)
      queryParams.append('CurrentPage', filter.currentPage.toString());
    if (filter?.textSearch != null) queryParams.append('TextSearch', filter.textSearch);

    const queryString = queryParams.toString();
    const url = queryString
      ? `${ACTIVITY_LOG_URL}/get-user-log-list?${queryString}`
      : `${ACTIVITY_LOG_URL}/get-user-log-list`;
    return httpService.get(url, config);
  }

  getOneUserLog(logId: string) {
    return httpService.get(`${ACTIVITY_LOG_URL}/get-user-log?logId=${logId}`);
  }

  getExportUserActivityLog(listId: string[]) {
    return httpService.post(`${ACTIVITY_LOG_URL}/export-log`, listId);
  }
}

export default new UserActivityLog();
