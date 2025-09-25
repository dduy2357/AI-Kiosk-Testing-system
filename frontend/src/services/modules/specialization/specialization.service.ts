import { SPECIALIZATION_URL } from "@/consts/apiUrl";
import httpService from "@/services/httpService";

class SpecializationService {
  getListSpecializations() {
    return httpService.get(`${SPECIALIZATION_URL}`);
  }
}

export default new SpecializationService();
