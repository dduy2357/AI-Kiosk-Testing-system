import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import { StudentExamList, StudentExamResponse } from '../interfaces/studentexam.interface';
import studentexamService from '../studentexam.service';

const useGetListExamStudent = (
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
  const [data, setData] = useState<StudentExamList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);

  const requestAPI = studentexamService.getListExams;

  //! Function
  const fetch = useCallback(() => {
    if (!isTrigger) {
      return Promise.resolve(null);
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const response = await requestAPI({ signal: signal.current.signal });
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [isTrigger, requestAPI]);

  const checkConditionPass = useCallback(
    (response: StudentExamResponse, options: { isLoadmore?: boolean } = {}) => {
      const { isLoadmore } = options;

      if (saveData) {
        save(cachedKeys.dataExamStudent, response?.data?.data || []);
      }

      if (isArray(response?.data?.data)) {
        if (isLoadmore) {
          setData((prev) => {
            let nextData = cloneDeep(prev);
            nextData = [...(nextData || []), ...(response?.data?.data || [])];
            return nextData;
          });
        } else {
          setData(response?.data?.data);
        }
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
        checkConditionPass(response as StudentExamResponse);
      }
      setRefetching(false);
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    }
  }, [fetch, checkConditionPass]);

  useEffect(() => {
    if (refetchKey) {
      save(refetchKey, refetch);
    }
  }, [save, refetchKey, refetch]);

  useEffect(() => {
    signal.current = new AbortController();

    const fetchAPI = async () => {
      try {
        setLoading(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response as StudentExamResponse);
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
  }, [isTrigger, fetch, checkConditionPass]);

  //! Render
  return {
    data,
    loading,
    error,
    refetch,
    refetching,
    setData,
  };
};

export default useGetListExamStudent;
