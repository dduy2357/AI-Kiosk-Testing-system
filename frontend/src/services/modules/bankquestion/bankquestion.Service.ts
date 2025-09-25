import { QUESTION_BANK_URL } from '@/consts/apiUrl';
import httpService from '@/services/httpService';
import { AxiosRequestConfig } from 'axios';
import {
  IBankQuestionRequest,
  IQuestionBankForm,
  IShareBankQuestionForm,
} from './interfaces/bankquestion.interface';

class BankQuestionService {
  getAllBankQuestions(filter: IBankQuestionRequest, config: AxiosRequestConfig) {
    const params = new URLSearchParams({
      pageSize: filter.pageSize?.toString() || '50',
      currentPage: filter.currentPage?.toString() || '1',
      textSearch: filter.textSearch || '',
      filterSubject: filter.filterSubject || '',
    });

    if (filter.IsMyQuestion !== undefined) {
      params.append('IsMyQuestion', filter.IsMyQuestion.toString());
    }

    return httpService.get(`${QUESTION_BANK_URL}/get-list?${params.toString()}`, config);
  }

  getDetailBankQuestion(id: string) {
    return httpService.get(`${QUESTION_BANK_URL}/${id}/detail`);
  }

  getShareBankQuestion(data: IShareBankQuestionForm) {
    const transformedData = {
      ...data,
      accessMode: Number(data.accessMode),
    };
    return httpService.post(`${QUESTION_BANK_URL}/share`, transformedData);
  }

  addQuestionBank(values: IQuestionBankForm) {
    return httpService.post(`${QUESTION_BANK_URL}/add`, values);
  }

  toggleQuestionBank(questionBankId: string) {
    return httpService.put(`${QUESTION_BANK_URL}/status/${questionBankId}`, {});
  }
}
export default new BankQuestionService();
