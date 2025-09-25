import { showError } from '@/helpers/toast';
import httpService from '@/services/httpService';
import { isArray } from 'lodash';
import cloneDeep from 'lodash/cloneDeep';
import { useCallback, useEffect, useRef, useState } from 'react';
import { IListUserActivityLog, IUserActivityLogRequest, ResponseUserActivityLog } from '../interfaces/useractivitylog.interface';
import useractivitylogService from '../useractivitylog.service';

//* Check parse body request
const parseRequest = (filters: IUserActivityLogRequest) => {
    return cloneDeep({
        FromDate: filters.FromDate,
        ToDate: filters.ToDate || undefined,
        RoleEnum: filters.RoleEnum || null,
        pageSize: filters?.pageSize || 5,
        currentPage: filters?.currentPage || 1,
        textSearch: filters?.textSearch || '',
    });
};
const requestAPI = useractivitylogService.getUserActivityLog;
const useGetAllUserActivityLog = (
    filters: IUserActivityLogRequest,
    options: {
        isTrigger?: boolean;
    } = {
            isTrigger: true,
        },
) => {
    //! State
    const { isTrigger = true } = options;

    const [data, setData] = useState<IListUserActivityLog[]>([]);
    const signal = useRef(new AbortController());
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<unknown>(null);
    const [refetching, setRefetching] = useState(false);
    const [totalPage, setTotalPage] = useState<number>(1);
    const [total, setTotal] = useState<number>(0);
    const token = httpService.getTokenStorage();
    const [loadingMore,] = useState(false);

    //! Function

    const fetch = useCallback(() => {
        if (!isTrigger) {
            return Promise.resolve(null); // Return null if not triggered
        }

        return new Promise((resolve, reject) => {
            (async () => {
                try {
                    const nextFilters = parseRequest(filters);
                    httpService.attachTokenToHeader(token);

                    const response = await requestAPI(nextFilters, {
                        signal: signal.current.signal,
                    });
                    resolve(response);
                } catch (error) {
                    setData([]);
                    setTotal(0);
                    setTotalPage(1);
                    setError(error);
                    reject(error);
                }
            })();
        });
    }, [filters, isTrigger, token]);

    const checkConditionPass = useCallback(
        (response: ResponseUserActivityLog) => {
            if (isArray(response?.data?.data?.result)) {
                setTotalPage(response.data.data.totalPage || 1);
                setData(response.data.data.result);
                setTotal(response.data.data.total || 0);
            }
        },
        [],
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
                checkConditionPass(response as ResponseUserActivityLog);
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
                    checkConditionPass(response as ResponseUserActivityLog);
                }
            } catch (error) {
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
        totalPage,
        total,
        loadingMore,

    };
};

export default useGetAllUserActivityLog;