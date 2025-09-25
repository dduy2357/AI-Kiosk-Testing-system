import { PROHIBITED_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { AxiosRequestConfig } from 'axios';
import { IProhibitedRequest } from './interfaces/prohibited.interface';

class ProhibitedService {
  getAllProhibited(filter: IProhibitedRequest, config: AxiosRequestConfig) {
    const params = new URLSearchParams({
      PageSize: (filter?.PageSize ?? 50).toString(),
      CurrentPage: (filter?.CurrentPage ?? 1).toString(),
      TextSearch: filter?.TextSearch || '',
    });

    if (filter?.IsActive !== undefined) {
      params.append('IsActive', filter.IsActive.toString());
    }
    if (filter?.RiskLevel !== undefined) {
      params.append('RiskLevel', filter.RiskLevel.toString());
    }
    if (filter?.Category !== undefined) {
      params.append('Category', filter.Category.toString());
    }
    if (filter?.TypeApp !== undefined) {
      params.append('TypeApp', filter.TypeApp.toString());
    }
    return httpService.get(`${PROHIBITED_URL}/GetAll?${params.toString()}`, config);
  }

  getOneProhibited(appId: string) {
    return httpService.get(`${PROHIBITED_URL}/GetOne/${appId}`);
  }

  deleteProhibited(appId: string[]) {
    return httpService.delete(`${PROHIBITED_URL}/DoRemove`, {
      data: appId,
    });
  }

  createUpdateProhibited(body: FormData) {
    return httpService.post(`${PROHIBITED_URL}/CreateUpdate`, body);
  }

  changeStatusProhibited(appId: string[]) {
    return httpService.post(`${PROHIBITED_URL}/ChangeActivate`, appId);
  }
}

export default new ProhibitedService();
