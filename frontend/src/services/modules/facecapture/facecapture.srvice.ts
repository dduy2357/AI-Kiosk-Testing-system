import { FACE_CAPTURE_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { IFaceCaptureRequest } from './interfaces/facecapture.interface';
import { AxiosRequestConfig } from 'axios';

class FaceCaptureService {
  addFaceCapture(data: FormData) {
    return httpService.post(`${FACE_CAPTURE_URL}/add`, data);
  }

  getListFaceCapture(filters?: IFaceCaptureRequest, config?: AxiosRequestConfig) {
    return httpService.get(
      `${FACE_CAPTURE_URL}/get-list?StudentExamId=${filters?.StudentExamId}&ExamId=${filters?.ExamId}&LogType=${filters?.LogType || ''}&PageSize=${filters?.PageSize || 10}&CurrentPage=${filters?.CurrentPage || 1}&TextSearch=${filters?.TextSearch || ''}`,
      config,
    );
  }

  downloadFaceCapture(studentExamId: string) {
    return httpService.get(`${FACE_CAPTURE_URL}/download-all-captures/${studentExamId}`, {
      responseType: 'blob',
    });
  }

  viewStatistic(studentExamId: string) {
    return httpService.post(`${FACE_CAPTURE_URL}/analyze-face-capture/${studentExamId}`, {});
  }
}

export default new FaceCaptureService();
