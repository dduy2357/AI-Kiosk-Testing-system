import { AxiosRequestConfig } from 'axios';
import { IAlertRequest } from './interfaces/alert.interface';
import httpService from '@/services/httpService';
import { NOTIFICATION_URL } from '@/consts/apiUrl';
import { SendAlertValues } from '@/pages/teacher/examsupervision/dialogs/DialogSendAlert';

class AlertService {
  getAlert(filter?: IAlertRequest, config?: AxiosRequestConfig) {
    return httpService.get(
      `${NOTIFICATION_URL}/GetAlert?${filter?.PageSize}&${filter?.CurrentPage}&${filter?.TextSearch}`,
      config,
    );
  }

  markAsRead(id: string) {
    return httpService.post(`${NOTIFICATION_URL}/MarkAsRead?id=${id}`, {});
  }

  getAlertDetail(id: string) {
    return httpService.get(`${NOTIFICATION_URL}/GetDetail/${id}`);
  }

  sendAlert(values: SendAlertValues) {
    return httpService.post(`${NOTIFICATION_URL}/Create`, values);
  }

  markAllAsRead() {
    return httpService.post(`${NOTIFICATION_URL}/MarkAllAsRead`, {});
  }

  deleteAlert(ids: string[]) {
    const queryString = ids.map((id) => `ids=${encodeURIComponent(id)}`).join('&');
    return httpService.delete(`${NOTIFICATION_URL}/Delete?${queryString}`);
  }
}

export default new AlertService();
