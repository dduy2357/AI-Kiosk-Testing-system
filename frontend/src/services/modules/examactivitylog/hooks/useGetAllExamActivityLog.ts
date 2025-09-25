import { showError } from '@/helpers/toast';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import examactivitylogService from '../examactivitylog.service';
import {
  IExamActivityLogRequest,
  IListExamActivityLog,
  ResponseExamActivityLog,
} from '../interfaces/examactivitylog.interface';

//* Check parse body request
const parseRequest = (filters: IExamActivityLogRequest) => {
  return cloneDeep({
    studentExamId: filters?.studentExamId || '',
    pageSize: filters?.pageSize || 5,
    currentPage: filters?.currentPage || 1,
    textSearch: filters?.textSearch || '',
  });
};
const requestAPI = examactivitylogService.getListExamActivityLog;
const useGetListExamActivityLog = (
  filters: IExamActivityLogRequest,
  options: {
    isTrigger?: boolean;
  } = {
      isTrigger: true,
    },
) => {
  //! State
  const { isTrigger = true } = options;

  const [data, setData] = useState<IListExamActivityLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const signal = useRef(new AbortController());
  const [totalPage, setTotalPage] = useState<number>(1);
  const [refetching, setRefetching] = useState(false);
  const [total, setTotal] = useState<number>(0);
  const [loadingMore] = useState(false);

  //! Function
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
          setTotalPage(1);
          setTotal(0);
          setError(error);
          reject(error);
        }
      })();
    });
  }, [filters, isTrigger]);

  const checkConditionPass = useCallback((response: ResponseExamActivityLog) => {
    if (isArray(response?.data?.data?.result)) {
      setTotalPage(response.data.data.totalPage || 1);
      setData(response.data.data.result);
      setTotal(response.data.data.total || 0);
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
        checkConditionPass(response as ResponseExamActivityLog);
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
          checkConditionPass(response as ResponseExamActivityLog);
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
    setData,
    totalPage,
    total,
    loadingMore,
    refetching,
    refetch
  };
};

export default useGetListExamActivityLog;
