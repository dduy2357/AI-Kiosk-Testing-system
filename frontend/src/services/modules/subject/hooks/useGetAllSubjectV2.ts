import { showError } from '@/helpers/toast';
import { cloneDeep, isArray } from 'lodash';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  ISubjectRequest,
  ResponseGetListSubject,
  SubjectList,
} from '../interfaces/subject.interface';
import subjectService from '../subject.service';

const parseRequest = (filters: ISubjectRequest) => {
  return cloneDeep({
    pageSize: filters?.pageSize || 50,
    currentPage: filters?.currentPage || 1,
    textSearch: filters?.textSearch || '',
    status: filters?.status || true,
  });
};
const requestAPI = subjectService.getAllSubjects;
const useGetAllSubjectV2 = (
  filters: ISubjectRequest,
  options: {
    isTrigger?: boolean;
    isLoadmore?: boolean;
  } = {
    isTrigger: true,
    isLoadmore: false,
  },
) => {
  const { isTrigger = true, isLoadmore = false } = options;
  const signal = useRef(new AbortController());
  const [data, setData] = useState<SubjectList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);
  const [hasMore, setHasMore] = useState(false);
  const [error, setError] = useState<unknown>(null);

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

  const checkConditionPass = useCallback(
    (response: ResponseGetListSubject) => {
      if (isArray(response?.data?.data?.result)) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
      }
      if (isArray(response?.data?.data?.result)) {
        if (isLoadmore && filters.currentPage > 1) {
          setData((prevData) => [
            ...prevData,
            ...(Array.isArray(response?.data?.data?.result) ? response.data.data.result : []),
          ]);
        } else {
          setData(response?.data?.data?.result);
        }
        setHasMore(response?.data?.data?.currentPage < response?.data?.data?.totalPage);
      }
    },
    [filters.currentPage, isLoadmore],
  );

  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }

      setRefetching(true);
      const response = await fetch();
      if (response) {
        checkConditionPass(response as ResponseGetListSubject);
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
          checkConditionPass(response as ResponseGetListSubject);
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
    refetching,
    hasMore,
    setData,
    totalPage,
    total,
  };
};

export default useGetAllSubjectV2;
