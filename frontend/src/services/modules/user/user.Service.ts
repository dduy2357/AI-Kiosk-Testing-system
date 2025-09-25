import { USER_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { AxiosRequestConfig } from 'axios';
import { IUserRequest, ResponseGetListUser } from './interfaces/user.interface';
import { UserData } from '@/pages/admin/manageuser/dialogs/DialogPreCheckImportUser';

class UserService {
  getUserList(filter: IUserRequest, config?: AxiosRequestConfig): Promise<ResponseGetListUser> {
    return httpService.get(
      `${USER_URL}/get-list?pageSize=${filter.pageSize}&currentPage=${filter.currentPage}&textSearch=${filter.textSearch}&roleId=${filter.roleId || ''}&status=${filter.status || ''}&campusId=${filter.campusId || ''}&sortType=${filter.sortType || ''}`,
      config,
    );
  }

  createUser(body: FormData) {
    return httpService.post(`${USER_URL}/create`, body);
  }

  updateUser(body: FormData) {
    return httpService.post(`${USER_URL}/update`, body);
  }

  getDetailUser(userId: string, config?: AxiosRequestConfig) {
    return httpService.get(`${USER_URL}/get-by-id?userId=${userId}`, config);
  }

  importUser(file: FormData) {
    return httpService.post(`${USER_URL}/check-import-data`, file, {});
  }

  addAfterImportUser(file: UserData) {
    return httpService.post(`${USER_URL}/add-list-user`, file, {});
  }

  async exportUser(): Promise<void> {
    const response = await httpService.post(
      `${USER_URL}/export-data`,
      {},
      {
        responseType: 'blob',
      },
    );

    // Extract filename from Content-Disposition header, if available
    const contentDisposition = response.headers['content-disposition'];
    let filename = 'users_export.xlsx'; // Default filename
    if (contentDisposition) {
      const filenameMatch = contentDisposition.match(/filename="?(.+)"?/i);
      if (filenameMatch && filenameMatch[1]) {
        filename = filenameMatch[1];
      }
    }

    // Create a blob URL and trigger download
    const blob = new Blob([response.data], {
      type:
        response.headers['content-type'] ||
        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
    });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', filename);
    document.body.appendChild(link);
    link.click();

    // Clean up
    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  toggleActiveUser(userId: string) {
    return httpService.post(`${USER_URL}/toggle-active`, JSON.stringify(userId), {
      headers: {
        'Content-Type': 'application/json;odata.metadata=minimal;odata.streaming=true',
        Accept: '*/*',
      },
    });
  }
}

export default new UserService();
