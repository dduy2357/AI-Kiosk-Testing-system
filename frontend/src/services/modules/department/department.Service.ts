import { DEPARTMENT_URL } from "@/consts/apiUrl";
import httpService from "@/services/httpService";

class DepartmentService {
  getListDepartments() {
    return httpService.get(`${DEPARTMENT_URL}`);
  }
}

export default new DepartmentService();
