import { useSave } from '@/stores/useStores';
import { cloneDeep, isEmpty } from 'lodash';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  IMonitorDetailRequest,
  MonitorDetailList,
  ResponseGetMonitorDetailList,
} from '../interfaces/monitorDetail.interface';
import monitorService from '../monitor.service';

/**
 * Please check:
 * - fetch()
 * - refetch()
 * - checkConditionPass()
 */

const parseRequest = (filters: IMonitorDetailRequest) => {
  return cloneDeep({
    PageSize: filters?.PageSize || 10,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
    StudentExamStatus: filters?.StudentExamStatus !== undefined ? filters.StudentExamStatus : null,
  });
};

const useGetMonitorDetail = (
  filters: IMonitorDetailRequest,
  id: string,
  options: { isTrigger?: boolean; cachedKey?: string } = { isTrigger: true, cachedKey: '' },
) => {
  //! State
  const signal = useRef(new AbortController());
  const { isTrigger = true, cachedKey = '' } = options;

  const save = useSave();
  const [data, setData] = useState<MonitorDetailList>();
  const [isLoading, setLoading] = useState(false);
  const [isRefetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);

  //! Function
  const fetch: () => Promise<ResponseGetMonitorDetailList> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const nextFilters = parseRequest(filters);
          const response = await monitorService.examMonitorDetail(nextFilters, id, {
            signal: signal.current.signal,
          });
          resolve(response);
        } catch (error) {
          setData(undefined);
          setError(error);
          reject(error);
        }
      })();
    });
  }, [id, isTrigger, filters]);

  const checkConditionPass = useCallback((response: ResponseGetMonitorDetailList) => {
    //* Check condition of response here to set data
    if (!isEmpty(response?.data?.data?.result)) {
      setData(response?.data.data.result);
    }
  }, []);

  //* Refetch implicity (without changing loading state)
  const refetch = useCallback(async () => {
    try {
      setRefetching(true);
      signal.current = new AbortController();
      const response = await monitorService.examMonitorDetail(filters, id, {
        signal: signal.current.signal,
      });
      checkConditionPass(response);
    } catch (error) {
      setData(undefined);
      // showError(errorHandler(error));
    } finally {
      setRefetching(false);
    }
  }, [id, filters, checkConditionPass]);

  useEffect(() => {
    if (cachedKey) {
      save(cachedKey, { refetch, data, isLoading, isRefetching });
    }
  }, [save, cachedKey, refetch, data, isLoading, isRefetching]);

  //* Refetch with changing loading state
  const refetchWithLoading = useCallback(
    async (shouldSetData: boolean) => {
      try {
        setLoading(true);
        signal.current = new AbortController();
        const response = await fetch();
        if (shouldSetData && response) {
          checkConditionPass(response);
        }
      } catch (error) {
        setError(error);
      } finally {
        setLoading(false);
      }
    },
    [fetch, checkConditionPass],
  );

  useEffect(() => {
    let shouldSetData = true;
    signal.current = new AbortController();

    (async () => {
      try {
        setLoading(true);
        const response = await fetch();
        if (shouldSetData && response) {
          checkConditionPass(response);
        }
      } catch (error) {
        setData(undefined);
        setError(error);
      } finally {
        setLoading(false);
      }
    })();

    return () => {
      shouldSetData = false;
      signal.current.abort();
    };
  }, [fetch, checkConditionPass]);

  return {
    data,
    isLoading,
    error,
    refetch,
    refetchWithLoading,
    isRefetching,
    setData,
  };
};

export default useGetMonitorDetail;
