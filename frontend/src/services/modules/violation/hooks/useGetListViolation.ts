import { showError } from '@/helpers/toast';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import { IViolationRequest, ResponseViolation, Violation } from '../interfaces/violation.interface';
import violationService from '../violation.service';

/********************************************************
 * SNIPPET GENERATED
 * GUIDE
 * Snippet for infinite scroll with page + rowsPerPage
 * Maybe you should check function:
 * - interface Request / Response
 * - parseRequest
 * - checkConditionPass
 * - fetch
 * - refetch
 ********************************************************/

//* Check parse body request
const parseRequest = (filters: IViolationRequest) => {
  return cloneDeep({
    ExamId: filters?.ExamId || '',
    PageSize: filters?.PageSize || 50,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
    StudentExamId: filters?.StudentExamId || '',
  });
};

const requestAPI = violationService.getListViolation;

const useGetListViolation = (
  filters: IViolationRequest,
  options: {
    isTrigger?: boolean;
  } = {
    isTrigger: true,
  },
) => {
  //! State
  const { isTrigger = true } = options;
  const signal = useRef(new AbortController());
  const [data, setData] = useState<Violation[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);

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
          setError(error);
          reject(error);
        }
      })();
    });
  }, [filters, isTrigger]);

  const checkConditionPass = useCallback((response: ResponseViolation) => {
    if (isArray(response?.data?.data?.result)) {
      setTotalPage(response?.data?.data?.totalPage || 1);
      setTotal(response?.data?.data?.total || 0);
      setData(response?.data?.data?.result);
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
        checkConditionPass(response as ResponseViolation);
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
          checkConditionPass(response as ResponseViolation);
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
  }, [isTrigger, fetch, checkConditionPass]); // Only re-run when isTrigger changes

  return {
    data,
    loading,
    error,
    refetch,
    refetching,
    setData,
    totalPage,
    total,
  };
};

export default useGetListViolation;
