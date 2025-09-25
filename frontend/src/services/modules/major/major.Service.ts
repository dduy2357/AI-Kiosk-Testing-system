import { MAJOR_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';

class MajorService {
  getMajorList() {
    return httpService.get(`${MAJOR_URL}`);
  }
}

export default new MajorService();
