import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import { IMonitorRequest, MonitorList, MonitorListResponse } from '../interfaces/monitor.interface';
import monitorService from '../monitor.service';

const parseRequest = (filters: IMonitorRequest) => {
  return cloneDeep({
    PageSize: filters?.PageSize || 10,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
    SubjectId: filters?.SubjectId || '',
    ExamStatus: filters?.ExamStatus !== undefined ? filters?.ExamStatus : undefined,
  });
};

const requestAPI = monitorService.getListMonitor;

const useGetListMonitor = (
  filters: IMonitorRequest,
  options: {
    isTrigger?: boolean;
    refetchKey?: string;
    saveData?: boolean;
  } = {
    isTrigger: true,
    refetchKey: '',
    saveData: true,
  },
) => {
  const { isTrigger = true, refetchKey = '', saveData = true } = options;
  const signal = useRef(new AbortController());
  const save = useSave();
  const [data, setData] = useState<MonitorList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);

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
    (response: MonitorListResponse) => {
      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
        save(cachedKeys.totalPageMonitorCount, response?.data?.data.totalPage || 1);
        save(cachedKeys.totalMonitorCount, response?.data?.data.total || 0);
        save(cachedKeys.dataMonitor, response?.data?.data?.result || []);
      }

      // Always replace data with the new page's results
      if (isArray(response?.data?.data?.result)) {
        setData(response?.data?.data?.result); // Replace, don't append
      } else {
        setData([]); // Clear data if response is empty
      }
    },
    [saveData, save],
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
        checkConditionPass(response as MonitorListResponse);
      }
      setRefetching(false);
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
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
          checkConditionPass(response as MonitorListResponse);
        }
      } catch (error) {
        setData([]);
        showError(error);
      } finally {
        setLoading(false);
      }
    };

    fetchAPI();

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [isTrigger, fetch, checkConditionPass, filters.CurrentPage]); // Added filters.CurrentPage to trigger fetch on page change

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

export default useGetListMonitor;
