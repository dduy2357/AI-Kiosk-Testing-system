import { AxiosRequestConfig } from 'axios';
import { ClassList, IClassRequest, ResponseGetListClass } from './interfaces/class.interface';
import httpService from '@/services/httpService';
import { CLASS_URL } from '@/consts/apiUrl';

class ClassService {
  getAllClasses(filter: IClassRequest, config: AxiosRequestConfig): Promise<ResponseGetListClass> {
    // Xây dựng các tham số cơ bản
    const params = new URLSearchParams({
      PageSize: (filter?.PageSize ?? 10).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
      FromDate: filter?.FromDate?.toISOString() || '',
      ToDate: filter?.ToDate?.toISOString() || '',
    });

    // Chỉ thêm IsActive nếu không phải null hoặc undefined
    if (filter?.IsActive !== null && filter?.IsActive !== undefined) {
      params.append('IsActive', filter.IsActive.toString());
    }

    return httpService.get(`${CLASS_URL}/get-all?${params.toString()}`, config);
  }

  // Các hàm khác giữ nguyên
  getClassById(classId: string) {
    return httpService.get(`${CLASS_URL}/get-by-id/${classId}`);
  }

  createNewClass(body: ClassList) {
    return httpService.post(`${CLASS_URL}/create-update-class`, body);
  }

  updateClass(body: ClassList) {
    return httpService.post(`${CLASS_URL}/create-update-class`, body);
  }

  activeDeactiveClass(classId: string) {
    return httpService.post(`${CLASS_URL}/deactivate-class/${classId}`, {});
  }
}

export default new ClassService();
