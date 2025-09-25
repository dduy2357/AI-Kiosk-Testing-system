import { AxiosRequestConfig } from 'axios';
import { IPermissionRequest } from './interfaces/permission.interface';
import httpService from '@/services/httpService';
import { AUTHORIZE_URL } from '@/consts/apiUrl';

export interface CreateUpdatePermissionData {
  categoryID?: string;
  description?: string;
}

class PermissionService {
  getAllPermission(filters?: IPermissionRequest, config?: AxiosRequestConfig) {
    return httpService.get(
      `${AUTHORIZE_URL}/get-all-category-permissions?PageSize=${filters?.PageSize}&CurrentPage=${filters?.CurrentPage}&TextSearch=${filters?.TextSearch}`,
      config,
    );
  }

  createUpdatePermission(data: CreateUpdatePermissionData) {
    return httpService.post(`${AUTHORIZE_URL}/create-update-category-permission`, {
      categoryID: data.categoryID,
      description: data.description,
    });
  }

  getDetailPermission(categoryId: string) {
    return httpService.get(`${AUTHORIZE_URL}/get-one-category-permission/${categoryId}`);
  }

  deletePermission(categoryId: string) {
    return httpService.delete(`${AUTHORIZE_URL}/delete-category-permission/${categoryId}`);
  }

  getDetailPermissionByCategoryID(categoryID: string) {
    return httpService.get(`${AUTHORIZE_URL}/get-one-category-permission/${categoryID}`);
  }
}

export default new PermissionService();
