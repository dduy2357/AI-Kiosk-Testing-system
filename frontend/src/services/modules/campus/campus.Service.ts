import { CAMPUS_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';

class CampusService {
  getCampusList() {
    return httpService.get(`${CAMPUS_URL}`);
  }
}

export default new CampusService();
