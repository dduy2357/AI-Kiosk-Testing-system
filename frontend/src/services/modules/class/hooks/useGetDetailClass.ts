import { useSave } from '@/stores/useStores';
import { isEmpty } from 'lodash';
import { useCallback, useEffect, useRef, useState } from 'react';

import { errorHandler } from '@/helpers/errors';
import { showError } from '@/helpers/toast';
import classService from '../class.service';
import { DetailClass, ResponseGetDetailClass } from '../interfaces/class.interface';

/**
 * Please check:
 * - fetch()
 * - refetch()
 * - checkConditionPass()
 */
const useGetDetailClass = (
  id: string | undefined,
  options: { isTrigger?: boolean; cachedKey?: string } = {
    isTrigger: true,
    cachedKey: '',
  },
) => {
  //! State
  const signal = useRef(new AbortController());
  const { isTrigger = true, cachedKey = '' } = options;

  const save = useSave();
  const [data, setData] = useState<DetailClass>();
  const [isLoading, setLoading] = useState(false);
  const [isRefetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);

  //! Function
  const fetch: () => Promise<ResponseGetDetailClass> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const response = await classService.getClassById(id as string);
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [id, isTrigger]);

  const checkConditionPass = useCallback(
    (response: ResponseGetDetailClass) => {
      //* Check condition of response here to set data
      if (!isEmpty(response?.data?.data)) {
        setData(response.data?.data);
        save('detailUser', response.data?.data);
      }
    },
    [save],
  );

  //* Refetch implicity (without changing loading state)
  const refetch = useCallback(async () => {
    try {
      setRefetching(true);
      signal.current = new AbortController();
      const response = await classService.getClassById(id as string);
      checkConditionPass(response);
    } catch (error: any) {
      showError(errorHandler(error));
    } finally {
      setRefetching(false);
    }
  }, [checkConditionPass, id]);

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

export default useGetDetailClass;
