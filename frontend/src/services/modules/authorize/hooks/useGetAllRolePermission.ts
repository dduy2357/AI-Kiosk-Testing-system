import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { flatten, isArray, isEmpty } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  IPermissionRequest,
  PermissionList,
  ResponseGetListPermission,
} from '../interfaces/role.interface';
import roleService from '../role.Service';

const parseRequest = (filters: IPermissionRequest) => {
  return cloneDeep({
    pageSize: filters?.pageSize || 50,
    currentPage: filters?.currentPage || 1,
    textSearch: filters?.textSearch || '',
  });
};

const requestAPI = roleService.getAllRolePermissions;

const useGetAllRolePermission = (
  filters: IPermissionRequest,
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
  const [data, setData] = useState<PermissionList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [loadingMore, setLoadingMore] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);

  //! Function
  const fetch: () => Promise<ResponseGetListPermission> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const nextFilters = parseRequest(filters);
          const response = await requestAPI(nextFilters);

          resolve(response);
        } catch (err) {
          setError(err);
          reject(err);
        }
      })();
    });
  }, [filters, isTrigger]);

  const checkConditionPass = useCallback(
    (response: ResponseGetListPermission, options: { isLoadmore?: boolean } = {}) => {
      const { isLoadmore } = options;

      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.pageSize || 0);
        save(cachedKeys.totalPagePermissionCount, response?.data?.data.totalPage || 1);
        save(cachedKeys.totalPermissionCount, response?.data?.data?.pageSize || 0);
        save(cachedKeys.dataPermission, response?.data?.data?.result || []);
      }

      if (isArray(response?.data?.data?.result)) {
        if (isLoadmore) {
          setData((prev) => {
            if (filters.currentPage === 1) {
              return response?.data?.data?.result || [];
            }
            return [...prev, ...(response?.data?.data?.result || [])];
          });
        } else {
          setData(response?.data?.data?.result || []);
        }

        setHasMore(!isEmpty(response?.data));
      }
    },
    [filters.currentPage, saveData, save],
  );

  //* Refetch implicitly (without changing loading state)
  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }

      setRefetching(true);
      const page = filters?.currentPage || 1;

      let listRequest: Promise<ResponseGetListPermission>[] = [];
      for (let eachPage = 1; eachPage <= page; eachPage++) {
        // Start from 1
        const nextFilters = parseRequest(filters);
        nextFilters.currentPage = eachPage;

        const request = requestAPI(nextFilters);

        listRequest = [...listRequest, request];
      }

      const responses = await Promise.allSettled(listRequest);
      const allData = responses.map((el) => {
        if (el.status === 'fulfilled') {
          return isArray(el?.value?.data?.data?.result) ? el?.value?.data?.data?.result : [];
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

    if (filters.currentPage === 1) {
      // Only fetch for page 1 initially
      fetchAPI();
    } else if (filters.currentPage > 1) {
      // Fetch more for pages > 1
      fetchMore();
    }

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [filters.currentPage, fetch, checkConditionPass]);

  return {
    data,
    loading,
    error,
    refetch,
    refetching,
    loadingMore,
    hasMore,
    setData,
    totalPage,
    total,
  };
};

export default useGetAllRolePermission;
