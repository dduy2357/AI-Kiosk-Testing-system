import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { flatten, isArray, isEmpty } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  IListSupervisorResponse,
  ISupervisorRequest,
  ListSupervisor,
} from '../interfaces/supervisor.interface';
import supervisorService from '../supervisor.service';

const parseRequest = (filters: ISupervisorRequest) => {
  if (!filters || typeof filters.PageSize !== 'number' || typeof filters.CurrentPage !== 'number') {
    return { PageSize: 10, CurrentPage: 1, TextSearch: filters?.TextSearch || '' };
  }
  return cloneDeep({
    PageSize: filters.PageSize,
    CurrentPage: filters.CurrentPage,
    TextSearch: filters.TextSearch,
  });
};

const requestAPI = supervisorService.getSupervisors;

const useGetListSupervisors = (
  filters: ISupervisorRequest,
  options: { isTrigger?: boolean; refetchKey?: string } = {
    isTrigger: true,
    refetchKey: '',
  },
) => {
  const { isTrigger = true, refetchKey = '' } = options;
  const signal = useRef(new AbortController());
  const save = useSave();
  const [data, setData] = useState<ListSupervisor[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);

  const fetch = useCallback((): Promise<IListSupervisorResponse> | undefined => {
    if (!isTrigger) return;
    return new Promise<IListSupervisorResponse>((resolve, reject) => {
      (async () => {
        try {
          const nextFilters = parseRequest(filters);
          const response: IListSupervisorResponse = await requestAPI(nextFilters, {
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
    (response: IListSupervisorResponse, options: { isLoadmore?: boolean } = {}) => {
      const { isLoadmore } = options;
      if (isArray(response?.data?.data?.result)) {
        if (isLoadmore) {
          setData((prev) => [...prev, ...(response?.data?.data?.result || [])]);
        } else {
          setData(response?.data?.data?.result || []);
        }
        setHasMore(!isEmpty(response?.data?.data?.result));
      } else {
        setData([]);
      }
    },
    [],
  );

  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }
      setRefetching(true);
      const pageCount = filters?.PageSize || 1;
      let listRequest: Promise<IListSupervisorResponse>[] = [];
      for (let eachPage = 1; eachPage <= pageCount; eachPage++) {
        const nextFilters = parseRequest(filters);
        nextFilters.CurrentPage = eachPage;
        const request = requestAPI(nextFilters, {
          signal: signal.current.signal,
        });
        listRequest = [...listRequest, request];
      }
      const responses = await Promise.allSettled(listRequest);
      const allData = responses
        .filter((el) => el.status === 'fulfilled')
        .map(
          (el) =>
            (el as PromiseFulfilledResult<IListSupervisorResponse>).value?.data?.data?.result || [],
        );
      setData(flatten(allData));
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    } finally {
      setRefetching(false);
    }
  }, [filters]);

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
          checkConditionPass(response);
        }
      } catch (error: any) {
        if (!error.isCanceled) {
          showError(error);
          setError(error);
        }
      } finally {
        setLoading(false);
      }
    };

    const fetchMore = async () => {
      try {
        setLoadingMore(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response, { isLoadmore: true });
        }
      } catch (error: any) {
        if (!error.isCanceled) {
          showError(error);
          setError(error);
        }
      } finally {
        setLoadingMore(false);
      }
    };

    if (filters.CurrentPage === 1) {
      fetchAPI();
    } else {
      fetchMore();
    }

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [filters, fetch, checkConditionPass]);

  return {
    data,
    loading,
    error,
    refetch,
    refetching,
    loadingMore,
    hasMore,
    setData,
  };
};

export default useGetListSupervisors;
