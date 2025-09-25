import { AxiosRequestConfig } from 'axios';
import { IUserInRoomRequest, ResponseUserInRoom } from './interfaces/userinroom.interface';
import httpService from '@/services/httpService';
import { USER_IN_ROOM_URL } from '@/consts/apiUrl';

interface IStuentActiveDeactiveRequest {
  roomId: string;
  studentId: string[];
  status: 0 | 1;
}
class UserInRoomService {
  getUserInRoom(
    filter: IUserInRoomRequest,
    config: AxiosRequestConfig,
  ): Promise<ResponseUserInRoom> {
    const params = new URLSearchParams({
      RoomId: filter?.RoomId || '',
      PageSize: (filter?.PageSize ?? 50).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
    });

    if (filter?.Status !== null && filter?.Status !== undefined) {
      params.append('Status', filter.Status.toString());
    }

    if (filter?.Role !== null && filter?.Role !== undefined) {
      params.append('Role', filter.Role.toString());
    }

    return httpService.get(`${USER_IN_ROOM_URL}/GetUsersInRoom?${params}`, config);
  }

  addUserToRoom(roomId: string, body: string[]) {
    return httpService.post(`${USER_IN_ROOM_URL}/AddUserToRoom?roomId=${roomId}`, body);
  }

  removeUserFromRoom(roomId: string, body: string[]) {
    return httpService.delete(`${USER_IN_ROOM_URL}/RemoveUsersFromRoom?roomId=${roomId}`, {
      data: body,
    });
  }

  importUserToRoom(roomId: string, fileData: FormData) {
    return httpService.post(`${USER_IN_ROOM_URL}/Import?roomId=${roomId}`, fileData, {});
  }

  async exportUserInRoom(roomId: string): Promise<void> {
    const response = await httpService.get(`${USER_IN_ROOM_URL}/Export?roomId=${roomId}`, {
      responseType: 'blob',
    });

    const contentDisposition = response.headers['content-disposition'];
    let filename = 'users_in_room_export.xlsx';
    if (contentDisposition) {
      const filenameMatch = contentDisposition.match(/filename="?(.+)"?/i);
      if (filenameMatch && filenameMatch[1]) {
        filename = filenameMatch[1];
      }
    }

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

    document.body.removeChild(link);
    window.URL.revokeObjectURL(url);
  }

  changeActiveStudent(data: IStuentActiveDeactiveRequest) {
    return httpService.post(`${USER_IN_ROOM_URL}/change-active-student`, data);
  }

  getAllRoomUsers(
    roomId: string,
    filters?: IUserInRoomRequest,
    config?: AxiosRequestConfig,
  ): Promise<ResponseUserInRoom> {
    return httpService.get(
      `${USER_IN_ROOM_URL}/GetAllRoomUsers?RoomId=${roomId}&PageSize=${filters?.PageSize}&CurrentPage=${filters?.CurrentPage}&TextSearch=${filters?.TextSearch}`,
      config,
    );
  }
}

export default new UserInRoomService();
