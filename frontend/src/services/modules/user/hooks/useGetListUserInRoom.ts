import { showError } from '@/helpers/toast';
import { useSave } from '@/stores/useStores';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import {
  AllRoomUser,
  IUserInRoomRequest,
  ResponseAllRoomUser,
} from '../../userinroom/interfaces/userinroom.interface';
import userinroomService from '../../userinroom/userinroom.service';

/********************************************************
 * SNIPPET GENERATED
 * GUIDE
 * Snippet for infinite scroll with page + rowsPerPage
 * Maybe you should check function:
 * - interface Request / Response
 * - parseRequest
 * - checkConditionPass
 * - fetch
 * - refetch
 ********************************************************/

//* Check parse body request
const parseRequest = (filters: IUserInRoomRequest) => {
  return cloneDeep({
    RoomId: filters?.RoomId || '',
    PageSize: filters?.PageSize || 50,
    CurrentPage: filters?.CurrentPage || 1,
    TextSearch: filters?.TextSearch || '',
  });
};

const useGetListUserInRoom = (
  roomId: string,
  filters: IUserInRoomRequest,
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
  const [data, setData] = useState<AllRoomUser[]>([]);
  const [loading, setLoading] = useState(false);
  const [refetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [hasMore, setHasMore] = useState(false);
  const [totalPage, setTotalPage] = useState<number>(1);
  const [total, setTotal] = useState<number>(0);

  //! Function
  const fetch = useCallback(() => {
    if (!isTrigger) {
      return Promise.resolve(null); // Return null if not triggered
    }

    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const nextFilters = parseRequest(filters);

          const response = await userinroomService.getAllRoomUsers(roomId, nextFilters, {
            signal: signal.current.signal,
          });
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [filters, isTrigger, roomId]);

  const checkConditionPass = useCallback(
    (response: ResponseAllRoomUser) => {
      if (saveData) {
        setTotalPage(response?.data?.data?.totalPage || 1);
        setTotal(response?.data?.data?.total || 0);
      }

      if (isArray(response?.data?.data?.result)) {
        setData(response?.data?.data?.result);
        setHasMore(response?.data?.data?.currentPage < response?.data?.data?.totalPage);
      }
    },
    [saveData],
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
        checkConditionPass(response as ResponseAllRoomUser);
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
          checkConditionPass(response as ResponseAllRoomUser);
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
  }, [isTrigger, fetch, checkConditionPass]); // Only re-run when isTrigger changes

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
