import { showError } from '@/helpers/toast';
import { cloneDeep, isArray } from 'lodash';
import { useCallback, useEffect, useState } from 'react';
import bankquestionService from '../bankquestion.Service';
import { BankQuestionData, IBankQuestionRequest } from '../interfaces/bankquestion.interface';

const parseRequest = (filters: IBankQuestionRequest) => {
  return cloneDeep({
    pageSize: filters?.pageSize || 50,
    currentPage: filters?.currentPage || 1,
    textSearch: filters?.textSearch || '',
    IsMyQuestion: filters?.IsMyQuestion || false,
  });
};

const usePaginationBankQuestionList = (
  filters: IBankQuestionRequest,
  options: {
    isTrigger?: boolean;
  } = {
    isTrigger: true,
  },
) => {
  const { isTrigger = true } = options;

  const [data, setData] = useState<BankQuestionData | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const fetchData = useCallback(async () => {
    if (!isTrigger) {
      return;
    }
    try {
      setLoading(true);
      const nextFilters = parseRequest(filters);
      const response = await bankquestionService.getAllBankQuestions(nextFilters, {});

      if (isArray(response?.data?.data)) {
        setData(response.data.data);
      }
    } catch (error) {
      setError(error);
      showError(error);
    } finally {
      setLoading(false);
    }
  }, [filters, isTrigger]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  return { data, loading, error, refetch: fetchData };
};

export default usePaginationBankQuestionList;
