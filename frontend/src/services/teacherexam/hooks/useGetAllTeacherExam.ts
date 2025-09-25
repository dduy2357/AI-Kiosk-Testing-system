import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  ITeacherExamRequest,
  ResponseGetListTeacherExam,
  TeacherExamList,
} from '../interfaces/teacherexamm.interface';
import teacherexamService from '../teacherexam.service';

//* Check parse body request
const parseRequest = (filters: ITeacherExamRequest) => {
  return cloneDeep({
    pageSize: filters.pageSize || 10,
    currentPage: filters.currentPage || 1,
    status: filters.status !== undefined ? filters.status : undefined,
    IsMyQuestion: filters.IsMyQuestion !== undefined ? filters.IsMyQuestion : undefined,
    textSearch: filters.textSearch || '',
    IsExamResult: filters.IsExamResult !== undefined ? filters.IsExamResult : undefined,
  });
};

const requestAPI = teacherexamService.getAllTeacherExam;

const useGetAllTeacherExam = (
  filters: ITeacherExamRequest,
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
  //! State
  const { isTrigger = true, refetchKey = '', saveData = true } = options;
  const signal = useRef(new AbortController());
  const save = useSave();
  const [data, setData] = useState<TeacherExamList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);

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
    (response: ResponseGetListTeacherExam) => {
      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
        save(cachedKeys.totalPageStudentExamResultCount, response?.data?.data.totalPage || 1);
        save(cachedKeys.totalStudentExamResultCount, response?.data?.data.total || 0);
        save(cachedKeys.dataStudentExamResult, response?.data?.data?.result || []);
      }

      if (isArray(response?.data?.data?.result)) {
        // Always replace the data with the new page's data
        setData(response?.data?.data?.result);
        setHasMore(response?.data?.data?.currentPage < response?.data?.data?.totalPage);
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
        checkConditionPass(response as ResponseGetListTeacherExam);
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
          checkConditionPass(response as ResponseGetListTeacherExam);
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
  }, [isTrigger, fetch, checkConditionPass, filters.currentPage]);

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
  };
};

export default useGetAllTeacherExam;
