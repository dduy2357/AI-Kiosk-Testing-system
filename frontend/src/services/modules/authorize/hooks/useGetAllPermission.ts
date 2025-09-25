import cachedKeys from '@/consts/cachedKeys';
import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import authorizeService from '../../authorize/role.Service';
import {
  IPermissionsRequest,
  PermissionsList,
  ResponseGetListPermissions,
} from '../interfaces/permission.interface';

//* Check parse body request
const parseRequest = (filters: IPermissionsRequest) => {
  return cloneDeep({
    PageSize: filters?.PageSize || 50,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
    SortType: filters?.SortType || null,
  });
};

const requestAPI = authorizeService.getAllPermissions;

const useGetListPermission = (
  filters: IPermissionsRequest,
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
  const [data, setData] = useState<PermissionsList[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);
  const [loadingMore, setLoadingMore] = useState(false);

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
          setData([]);
          if (saveData) {
            save(cachedKeys.dataPermissions, []);
          }
          reject(error);
        }
      })();
    });
  }, [filters, isTrigger, saveData, save]);

  const checkConditionPass = useCallback(
    (response: ResponseGetListPermissions, options: { isLoadmore?: boolean } = {}) => {
      const { isLoadmore } = options;

      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
        save(cachedKeys.totalPagePermissionsCount, response?.data?.data.totalPage || 1);
        save(cachedKeys.totalPermissionsCount, response?.data?.data.total || 0);
        const result = isArray(response?.data?.data?.result) ? response?.data?.data?.result : [];
        save(cachedKeys.dataPermissions, result);
      }

      if (isArray(response?.data?.data?.result)) {
        if (isLoadmore) {
          setData((prev) => {
            if (filters.CurrentPage === 1) {
              return response?.data?.data?.result;
            }
            let nextPages = cloneDeep(prev);
            nextPages = [...(nextPages || []), ...(response?.data?.data?.result || [])];
            return nextPages;
          });
        } else {
          setData(response?.data?.data?.result);
        }
        setHasMore(response?.data?.data?.currentPage < response?.data?.data?.totalPage);
      } else {
        setData([]);
        if (saveData) {
          save(cachedKeys.dataPermissions, []);
        }
      }
    },
    [saveData, save, filters.CurrentPage],
  );

  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }

      setRefetching(true);
      setError(null); // Clear previous errors
      const response = await fetch();
      if (response) {
        checkConditionPass(response as ResponseGetListPermissions);
      }
      setRefetching(false);
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
        setData([]);
        if (saveData) {
          save(cachedKeys.dataPermissions, []);
        }
      }
    }
  }, [fetch, checkConditionPass, saveData, save]);

  useEffect(() => {
    save(refetchKey, refetch);
  }, [save, refetchKey, refetch]);

  useEffect(() => {
    signal.current = new AbortController();

    const fetchAPI = async () => {
      try {
        setLoading(true);
        setError(null); // Clear previous errors
        const response = await fetch();
        if (response) {
          checkConditionPass(response as ResponseGetListPermissions);
        }
      } catch (error: any) {
        if (!error.isCanceled) {
          showError(error);
          setData([]);
          if (saveData) {
            save(cachedKeys.dataPermissions, []);
          }
        }
      } finally {
        setLoading(false);
      }
    };

    const fetchMore = async () => {
      try {
        setLoadingMore(true);
        setError(null); // Clear previous errors
        const response = await fetch();
        if (response) {
          checkConditionPass(response as ResponseGetListPermissions, { isLoadmore: true });
        }
      } catch (error: any) {
        if (!error.isCanceled) {
          showError(error);
          setData([]);
          if (saveData) {
            save(cachedKeys.dataPermissions, []);
          }
        }
      } finally {
        setLoadingMore(false);
      }
    };

    if (isTrigger) {
      if (filters.CurrentPage !== undefined && filters.CurrentPage <= 1) {
        fetchAPI();
      } else {
        fetchMore();
      }
    }

    return () => {
      if (signal.current) {
        signal.current.abort();
      }
    };
  }, [isTrigger, fetch, checkConditionPass, filters.CurrentPage, saveData, save, filters]);

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
    loadingMore,
  };
};

export default useGetListPermission;
