import { AxiosRequestConfig } from 'axios';
import { IViolationRequest, ResponseViolation } from './interfaces/violation.interface';
import httpService from '@/services/httpService';
import { VIOLATION_URL } from '@/consts/apiUrl';

class ViolationService {
  getListViolation(
    filter: IViolationRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseViolation> {
    const queryParams = new URLSearchParams();

    // Only add parameters that are not null or undefined
    if (filter?.ExamId != null) queryParams.append('ExamId', filter.ExamId);
    if (filter?.StudentExamId != null) queryParams.append('StudentExamId', filter.StudentExamId);
    if (filter?.PageSize != null) queryParams.append('PageSize', filter.PageSize.toString());
    if (filter?.CurrentPage != null)
      queryParams.append('CurrentPage', filter.CurrentPage.toString());
    if (filter?.TextSearch != null) queryParams.append('TextSearch', filter.TextSearch);

    const queryString = queryParams.toString();
    const url = queryString
      ? `${VIOLATION_URL}/get-list?${queryString}`
      : `${VIOLATION_URL}/get-list`;

    return httpService.get(url, config);
  }

  getViolationDetail(id: string) {
    return httpService.get(`${VIOLATION_URL}/get-one/${id}`);
  }

  createViolation(formData: FormData) {
    return httpService.post(`${VIOLATION_URL}/create`, formData);
  }
}

export default new ViolationService();
