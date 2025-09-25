import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
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
    status: filters?.status !== undefined ? filters.status : undefined,
  });
};

const requestAPI = subjectService.getAllSubjects;

const useGetListSubject = (
  filters: ISubjectRequest,
  options: {
    isTrigger?: boolean;
    refetchKey?: string;
    saveData?: boolean;
    isLoadmore?: boolean;
  } = {
    isTrigger: true,
    refetchKey: '',
    saveData: true,
    isLoadmore: false,
  },
) => {
  const { isTrigger = true, refetchKey = '', saveData = true, isLoadmore = false } = options;
  const signal = useRef(new AbortController());
  const save = useSave();
  const [data, setData] = useState<SubjectList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);
  const [loadingMore, setLoadingMore] = useState(false);

  const fetch = useCallback(() => {
    if (!isTrigger) {
      return Promise.resolve(null);
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
      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
        save(cachedKeys.totalPageSubjectCount, response?.data?.data.totalPage || 1);
        save(cachedKeys.totalSubjectCount, response?.data?.data.total || 0);
        save(cachedKeys.dataSubject, response?.data?.data?.result || []);
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
    [saveData, save, filters.currentPage, isLoadmore],
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
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    } finally {
      setRefetching(false);
    }
  }, [fetch, checkConditionPass]);

  const fetchMore = useCallback(async () => {
    try {
      setLoadingMore(true);
      const response = await fetch();
      if (response) {
        checkConditionPass(response as ResponseGetListSubject);
      }
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    } finally {
      setLoadingMore(false);
    }
  }, [fetch, checkConditionPass]);

  useEffect(() => {
    save(refetchKey, refetch);
  }, [save, refetchKey, refetch]);

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
        setData([]);
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
    hasMore,
    setData,
    totalPage,
    total,
    loadingMore,
    fetchMore,
  };
};

export default useGetListSubject;
