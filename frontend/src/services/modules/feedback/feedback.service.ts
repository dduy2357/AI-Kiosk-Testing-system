import { FEEDBACK_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { FeedbackResponse, IFeedbackForm, IFeedbackRequest } from './interfaces/feedback.interface';
import { AxiosRequestConfig } from 'axios';

class FeedbackService {
  getListFeedback(filter: IFeedbackRequest, config: AxiosRequestConfig): Promise<FeedbackResponse> {
    const queryParams = new URLSearchParams();

    // Only add parameters that are not null or undefined
    if (filter?.dateFrom != null) queryParams.append('dateFrom', filter.dateFrom.toISOString());
    if (filter?.dateTo != null) queryParams.append('dateTo', filter.dateTo.toISOString());
    if (filter?.pageSize != null) queryParams.append('PageSize', filter.pageSize.toString());
    if (filter?.currentPage != null)
      queryParams.append('CurrentPage', filter.currentPage.toString());
    if (filter?.textSearch != null) queryParams.append('TextSearch', filter.textSearch);

    const queryString = queryParams.toString();
    const url = queryString
      ? `${FEEDBACK_URL}/get-list?${queryString}`
      : `${FEEDBACK_URL}/get-list`;

    return httpService.get(url, config);
  }

  getFeedbackDetail(id: string) {
    return httpService.get(`${FEEDBACK_URL}/get-one/${id}`);
  }

  addFeedback(data: IFeedbackForm) {
    return httpService.post(`${FEEDBACK_URL}/create-update`, data);
  }
}

export default new FeedbackService();
