import { AxiosRequestConfig } from 'axios';
import {
  ISubjectForm,
  ISubjectRequest,
  ResponseGetDetailSubject,
  ResponseGetListSubject,
} from './interfaces/subject.interface';
import httpService from '@/services/httpService';
import { SUBJECT_URL } from '@/consts/apiUrl';

class SubjectService {
  getAllSubjects(
    filter: ISubjectRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseGetListSubject> {
    const params = new URLSearchParams({
      PageSize: (filter?.pageSize ?? 50).toString(),
      CurrentPage: (filter?.currentPage ?? 1).toString(),
      TextSearch: filter?.textSearch || '',
    });

    if (filter?.status !== undefined) {
      params.append('status', filter.status.toString());
    }

    return httpService.get(`${SUBJECT_URL}/GetAllSubjects?${params.toString()}`, config);
  }

  createSubject(body: ISubjectForm) {
    return httpService.post(`${SUBJECT_URL}/CreateUpdateSubject`, body);
  }

  getSubjectById(id: string): Promise<ResponseGetDetailSubject> {
    return httpService.get(`${SUBJECT_URL}/GetSubjectById/${id}`);
  }

  updateSubject(body: ISubjectForm) {
    return httpService.post(`${SUBJECT_URL}/CreateUpdateSubject`, body);
  }

  changeActiveSubject(subjectId: string) {
    return httpService.post(`${SUBJECT_URL}/ChangeActivateSubject/${subjectId}`, {});
  }
}

export default new SubjectService();
