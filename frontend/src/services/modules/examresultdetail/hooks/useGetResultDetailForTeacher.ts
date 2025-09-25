import { isEmpty } from 'lodash';
import { useCallback, useEffect, useRef, useState } from 'react';

import { errorHandler } from '@/helpers/errors';
import { showError } from '@/helpers/toast';
import teacherexamService from '@/services/teacherexam/teacherexam.service';
import { ResultDetailResponse } from '../interfaces/examresultdetail.interface';

/**
 * Please check:
 * - fetch()
 * - refetch()
 * - checkConditionPass()
 */
const useGetResultDetailForTeacher = (
  id: string | undefined,
  options: { isTrigger?: boolean } = {
    isTrigger: true,
  },
) => {
  //! State
  const signal = useRef(new AbortController());
  const { isTrigger = true } = options;

  const [data, setData] = useState<ResultDetailResponse>();
  const [isLoading, setLoading] = useState(false);
  const [isRefetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);

  //! Function
  const fetch: () => Promise<ResultDetailResponse> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const response = await teacherexamService.getResultDetail(id as string);
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [id, isTrigger]);

  const checkConditionPass = useCallback((response: ResultDetailResponse) => {
    //* Check condition of response here to set data
    if (!isEmpty(response)) {
      setData(response);
    }
  }, []);

  //* Refetch implicity (without changing loading state)
  const refetch = useCallback(async () => {
    try {
      setRefetching(true);
      signal.current = new AbortController();
      const response = await teacherexamService.getResultDetail(id as string);
      checkConditionPass(response);
    } catch (error: any) {
      showError(errorHandler(error));
    } finally {
      setRefetching(false);
    }
  }, [checkConditionPass, id]);

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

export default useGetResultDetailForTeacher;
