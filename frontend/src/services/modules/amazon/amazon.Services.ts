import { AMAZON_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';

class AmazonService {
  uploadImg(img: FormData) {
    return httpService.post(`${AMAZON_URL}/upload`, img, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
  }
}
export default new AmazonService();
