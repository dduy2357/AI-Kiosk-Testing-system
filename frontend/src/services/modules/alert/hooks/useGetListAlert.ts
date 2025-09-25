import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import alertService from '../alert.service';
import { IAlertRequest, ListAlert, ResponseAlertList } from '../interfaces/alert.interface';

//* Check parse body request
const parseRequest = (filters: IAlertRequest) => {
  return cloneDeep({
    PageSize: filters?.PageSize || 50,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
  });
};

const requestAPI = alertService.getAlert;

const useGetListAlert = (
  filters: IAlertRequest,
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
  //! State
  const { isTrigger = true, refetchKey = '', saveData = true } = options;
  const signal = useRef(new AbortController());
  const save = useSave();
  const [data, setData] = useState<ListAlert[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);
  const [loadingMore, setLoadingMore] = useState(false);

  //! Function
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
    (response: ResponseAlertList, options: { isLoadmore?: boolean } = {}) => {
      const { isLoadmore } = options;

      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
        save(cachedKeys.totalPageAlertCount, response?.data?.data.totalPage || 1);
        save(cachedKeys.totalAlertCount, response?.data?.data.total || 0);
        save(cachedKeys.dataAlert, response?.data?.data?.result || []);
      }

      if (isArray(response?.data?.data?.result)) {
        if (isLoadmore) {
          setData((prev) => {
            if (filters.CurrentPage === 1) {
              return response?.data?.data?.result;
            }
            let nextPages = cloneDeep(prev);
            nextPages = [...(nextPages || []), ...(response?.data?.data?.result || [])];
            return nextPages;
          });
        } else {
          setData(response?.data?.data?.result);
        }
        setHasMore(response?.data?.data?.currentPage < response?.data?.data?.totalPage);
      }
    },
    [saveData, save, filters.CurrentPage],
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
        checkConditionPass(response as ResponseAlertList);
      }
      setRefetching(false);
    } catch (error) {
      setData([]);
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
          checkConditionPass(response as ResponseAlertList);
        }
      } catch (error) {
        setData([]);
      } finally {
        setLoading(false);
      }
    };

    const fetchMore = async () => {
      try {
        setLoadingMore(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response as ResponseAlertList, { isLoadmore: true });
        }
      } catch (error) {
        showError(error);
      } finally {
        setLoadingMore(false);
      }
    };

    if (filters.CurrentPage !== undefined && filters.CurrentPage <= 1) {
      fetchAPI();
    } else {
      fetchMore();
    }

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [isTrigger, fetch, checkConditionPass, filters.CurrentPage]);

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
  };
};

export default useGetListAlert;
