import { showError } from '@/helpers/toast';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  IUserInRoomRequest,
  ResponseUserInRoom,
  UserElement,
} from '../interfaces/userinroom.interface';
import userinroomService from '../userinroom.service';

const parseRequest = (filters: IUserInRoomRequest) => {
  return cloneDeep({
    RoomId: filters?.RoomId || '',
    Role: filters?.Role !== undefined ? filters.Role : null,
    PageSize: filters?.PageSize || 50,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
    Status: filters?.Status !== undefined ? filters.Status : null,
  });
};

const requestAPI = userinroomService.getUserInRoom;

const useGetListUserInRoom = (
  filters: IUserInRoomRequest,
  options: {
    isTrigger?: boolean;
    refetchKey?: string | null;
    isLoadmore?: boolean;
  } = {
    isTrigger: true,
    refetchKey: null,
    isLoadmore: false,
  },
) => {
  const { isTrigger = true } = options;
  const signal = useRef(new AbortController());
  const [data, setData] = useState<UserElement[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
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

  const checkConditionPass = useCallback((response: ResponseUserInRoom) => {
    setTotalPage(response?.data?.data?.totalPage || 1);
    setTotal(response?.data?.data?.total || 0);

    if (isArray(response?.data?.data?.result?.users)) {
      setData(response?.data?.data?.result?.users);
      setHasMore(response?.data?.data?.currentPage < response?.data?.data?.totalPage);
    }
  }, []);

  const refetch = useCallback(async () => {
    try {
      if (signal.current) {
        signal.current.abort();
        signal.current = new AbortController();
      }

      setRefetching(true);
      const response = await fetch();
      if (response) {
        checkConditionPass(response as ResponseUserInRoom);
      }
      setRefetching(false);
    } catch (error: any) {
      if (!error.isCanceled) {
        showError(error);
      }
    }
  }, [fetch, checkConditionPass]);

  useEffect(() => {
    signal.current = new AbortController();

    const fetchAPI = async () => {
      try {
        setLoading(true);
        const response = await fetch();
        if (response) {
          checkConditionPass(response as ResponseUserInRoom);
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
export default useGetListUserInRoom;
