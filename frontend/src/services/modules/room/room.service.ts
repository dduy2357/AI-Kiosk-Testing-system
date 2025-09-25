import { AxiosRequestConfig } from 'axios';
import { IRoomRequest } from './interfaces/room.interface';
import httpService from '@/services/httpService';
import { ROOM_URL } from '@/consts/apiUrl';
import { RoomFormValues } from '@/pages/admin/manageroom/dialogs/DialogAddNewRoom';

class RoomService {
  getAllRooms(filter: IRoomRequest, config: AxiosRequestConfig) {
    const params = new URLSearchParams({
      PageSize: (filter?.PageSize ?? 10).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
    });

    if (filter?.IsActive !== null && filter?.IsActive !== undefined) {
      params.append('IsActive', filter.IsActive.toString());
    }

    return httpService.get(`${ROOM_URL}/GetAllRooms?${params.toString()}`, config);
  }

  getDetailRoom(roomId: string) {
    return httpService.get(`${ROOM_URL}/GetRoomById/${roomId}`);
  }

  createRoom(body: RoomFormValues) {
    return httpService.post(`${ROOM_URL}/CreateUpdateRoom`, body);
  }

  changeActiveRoom(roomId: string) {
    return httpService.post(`${ROOM_URL}/ChangeActivateRoom/${roomId}`, {});
  }
}

export default new RoomService();
