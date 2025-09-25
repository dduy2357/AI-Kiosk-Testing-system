import { POSITION_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';

class PositionService {
  getListPosition(departmentId: string) {
    return httpService.get(`${POSITION_URL}?departmentId=${departmentId}`);
  }
}

export default new PositionService();
