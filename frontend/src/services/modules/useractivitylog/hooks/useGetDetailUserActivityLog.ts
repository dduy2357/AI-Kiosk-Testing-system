import { errorHandler } from '@/helpers/errors';
import { showError } from '@/helpers/toast';
import { isEmpty } from 'lodash';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  ResponseUserActivityLogDetail,
  UserActivityLogDetail,
} from '../interfaces/useractivitylog.interface';
import useractivitylogService from '../useractivitylog.service';

const useGetDetailUserActivityLog = (
  id: string | undefined,
  options: { isTrigger?: boolean } = {
    isTrigger: true,
  },
) => {
  const { isTrigger = true } = options;
  const signal = useRef(new AbortController());
  const [data, setData] = useState<UserActivityLogDetail>();
  const [loading, setLoading] = useState(false);
  const [isRefetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown | null>(null);

  const fetch: () => Promise<ResponseUserActivityLogDetail> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }
    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const response = await useractivitylogService.getOneUserLog(id as string);
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [id, isTrigger]);

  const checkConditionPass = useCallback((response: ResponseUserActivityLogDetail) => {
    //* Check condition of response here to set data
    if (!isEmpty(response?.data?.data)) {
      setData(response.data?.data);
    }
  }, []);

  const refetch = useCallback(async () => {
    try {
      setRefetching(true);
      signal.current = new AbortController();
      const response = await useractivitylogService.getOneUserLog(id as string);
      checkConditionPass(response);
    } catch (error: any) {
      showError(errorHandler(error));
    } finally {
      setRefetching(false);
    }
  }, [checkConditionPass, id]);

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

  return { data, loading, error, refetch, refetchWithLoading, setData, isRefetching };
};

export default useGetDetailUserActivityLog;
