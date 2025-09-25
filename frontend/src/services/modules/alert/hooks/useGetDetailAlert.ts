import { useRef, useEffect, useState, useCallback } from 'react';
import { isEmpty } from 'lodash';

import { showError } from '@/helpers/toast';
import { errorHandler } from '@/helpers/errors';
import { AlertDetail, ResponseDetailAlert } from '../interfaces/alert.interface';
import alertService from '../alert.service';

/**
 * Please check:
 * - fetch()
 * - refetch()
 * - checkConditionPass()
 */
const useGetAlertDetail = (
  id: string,
  options: { isTrigger?: boolean; cachedKey?: string } = { isTrigger: true, cachedKey: '' },
) => {
  //! State
  const signal = useRef(new AbortController());
  const { isTrigger = true } = options;

  const [data, setData] = useState<AlertDetail>();
  const [isLoading, setLoading] = useState(false);
  const [isRefetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);

  //! Function
  const fetch: () => Promise<ResponseDetailAlert> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const response = await alertService.getAlertDetail(id);
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [id, isTrigger]);

  const checkConditionPass = useCallback((response: ResponseDetailAlert) => {
    //* Check condition of response here to set data
    if (!isEmpty(response?.data)) {
      setData(response?.data.data);
    }
  }, []);

  //* Refetch implicity (without changing loading state)
  const refetch = useCallback(async () => {
    try {
      setRefetching(true);
      signal.current = new AbortController();
      const response = await alertService.getAlertDetail(id);
      checkConditionPass(response);
    } catch (error: any) {
      showError(errorHandler(error));
    } finally {
      setRefetching(false);
    }
  }, [id, checkConditionPass]);

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

export default useGetAlertDetail;
