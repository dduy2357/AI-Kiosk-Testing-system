import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { flatten, isArray, isEmpty } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  IListExamSupervisorRequest,
  IListExamSupervisorResponse,
  ListExamSupervisor,
} from '../interfaces/supervisor.interface';
import supervisorService from '../supervisor.service';

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
const parseRequest = (filters: IListExamSupervisorRequest) => {
  return cloneDeep({
    PageSize: filters.PageSize,
    CurrentPage: filters.CurrentPage,
    TextSearch: filters.TextSearch,
  });
};

const requestAPI = supervisorService.getExams;

const useGetListExams = (
  filters: IListExamSupervisorRequest,
  options: { isTrigger?: boolean; refetchKey?: string } = {
    isTrigger: true,
    refetchKey: '',
  },
) => {
  //! State
  const { isTrigger = true, refetchKey = '' } = options;
  const signal = useRef(new AbortController());
  const save = useSave();
  const [data, setData] = useState<ListExamSupervisor[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);

  //! Function
  const fetch: () => Promise<IListExamSupervisorResponse> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
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
    (response: IListExamSupervisorResponse, options: { isLoadmore?: boolean } = {}) => {
      const { isLoadmore } = options;

      //* Check condition of response here to set data
      if (isArray(response?.data?.data?.result)) {
        if (isLoadmore) {
          setData((prev) => {
            let nextPages = cloneDeep(prev);
            nextPages = [...(nextPages || []), ...(response?.data?.data.result || [])];
            return nextPages;
          });
        } else {
          setData(response?.data?.data.result || []);
        }

        setHasMore(!isEmpty(response?.data));
      }
    },
    [],
  );

  //* Refetch implicity (without changing loading state)
  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }

      setRefetching(true);
      const page = filters?.PageSize || 1;

      let listRequest: Promise<IListExamSupervisorResponse>[] = [];
      for (let eachPage = 0; eachPage < page; eachPage++) {
        const nextFilters = parseRequest(filters);
        nextFilters.PageSize = eachPage;

        const request = requestAPI(nextFilters, {
          signal: signal.current.signal,
        });

        listRequest = [...listRequest, request];
      }

      const responses = await Promise.allSettled(listRequest);
      const allData = responses.map((el) => {
        if (el.status === 'fulfilled') {
          return isArray(el?.value?.data) ? el?.value?.data : [];
        }

        return [];
      });
      setData(flatten(allData));
      setRefetching(false);
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    }
  }, [filters]);

  useEffect(() => {
    save(refetchKey, refetch);
  }, [save, refetchKey, refetch]);

  useEffect(() => {
    signal.current = new AbortController();

    //* Fetch initial API
    const fetchAPI = async () => {
      try {
        setLoading(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response);
          setLoading(false);
        }
      } catch (error) {
        showError(error);
      } finally {
        setLoading(false);
      }
    };

    //* Fetch more API
    const fetchMore = async () => {
      try {
        setLoadingMore(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response, { isLoadmore: true });
        }
      } catch (error) {
        showError(error);
      } finally {
        setLoadingMore(false);
      }
    };

    if (filters.PageSize !== undefined && filters.PageSize <= 0) {
      fetchAPI();
    } else {
      //* If page / offset > 0 -> fetch more
      fetchMore();
    }

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [filters.PageSize, fetch, checkConditionPass]);

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

export default useGetListExams;
