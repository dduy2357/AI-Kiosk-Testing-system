import { showError } from '@/helpers/toast';
import { cloneDeep, isArray } from 'lodash';
import { useCallback, useEffect, useRef, useState } from 'react';
import bankquestionService from '../bankquestion.Service';
import {
  BankQuestionList,
  IBankQuestionRequest,
  ResponseGetListBankQuestion,
} from '../interfaces/bankquestion.interface';

const parseRequest = (filters: IBankQuestionRequest) => {
  return cloneDeep({
    pageSize: filters?.pageSize || 50,
    currentPage: filters?.currentPage || 1,
    textSearch: filters?.textSearch || '',
    IsMyQuestion: filters?.IsMyQuestion !== undefined ? filters?.IsMyQuestion : undefined,
    filterSubject: filters?.filterSubject || '',
    status: filters?.status || 1,
  });
};
const requestAPI = bankquestionService.getAllBankQuestions;
const useGetListBankQuestion = (
  filters: IBankQuestionRequest,
  options: {
    isTrigger?: boolean;
  } = {
    isTrigger: true,
  },
) => {
  const { isTrigger = true } = options;
  const signal = useRef(new AbortController());
  const [data, setData] = useState<BankQuestionList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [totalQuestionBanks, setTotalQuestionBanks] = useState<number>(0);
  const [totalQuestionsQB, setTotalQuestionsQB] = useState<number>(0);

  const fetch = useCallback(() => {
    if (!isTrigger) {
      return Promise.resolve(null); // Return null if not triggered
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const nextFilters = parseRequest(filters);
          const response = await requestAPI(nextFilters, {
            signal: signal.current.signal,
          });
          resolve(response);
        } catch (error) {
          setData([]);
          setError(error);
          reject(error);
        }
      })();
    });
  }, [filters, isTrigger]);

  const checkConditionPass = useCallback((response: ResponseGetListBankQuestion) => {
    if (isArray(response?.data?.data?.result)) {
      setTotalPage(response.data.data.totalPage || 1);
      setTotalQuestionBanks(response.data.data.totalQuestionBanks || 0);
      setTotalQuestionsQB(response.data.data.totalQuestionsQB || 0);
      setData(response.data.data.result);
    }
  }, []);

  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }

      setRefetching(true);
      const response = await fetch();
      if (response) {
        checkConditionPass(response as ResponseGetListBankQuestion);
      }
      setRefetching(false);
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    }
  }, [fetch, checkConditionPass]);

  useEffect(() => {
    signal.current = new AbortController();

    const fetchAPI = async () => {
      try {
        setLoading(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response as ResponseGetListBankQuestion);
        }
      } catch (error) {
        showError(error);
      } finally {
        setLoading(false);
      }
    };

    if (isTrigger) {
      fetchAPI();
    }

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [isTrigger, fetch, checkConditionPass]);

  return {
    data,
    loading,
    error,
    refetch,
    totalPage,
    totalQuestionBanks,
    refetching,
    totalQuestionsQB,
  };
};

export default useGetListBankQuestion;
