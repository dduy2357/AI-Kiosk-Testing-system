import { useCallback, useEffect, useRef, useState } from 'react';

import { errorHandler } from '@/helpers/errors';
import { showError } from '@/helpers/toast';
import { isEmpty } from 'lodash';
import feedbackService from '../feedback.service';
import { FeedbackDetail, ResponseGetDetailFeedback } from '../interfaces/feedback.interface';

const useGetFeedbackDetail = (
  id: string | undefined,
  options: { isTrigger?: boolean } = {
    isTrigger: true,
  },
) => {
  //! State
  const { isTrigger = true } = options;
  const signal = useRef(new AbortController());
  const [data, setData] = useState<FeedbackDetail>();
  const [loading, setLoading] = useState(false);
  const [isRefetching, setRefetching] = useState(false);
  const [error, setError] = useState<unknown>(null);

  //! Function
  const fetch: () => Promise<ResponseGetDetailFeedback> | undefined = useCallback(() => {
    if (!isTrigger) {
      return;
    }
    return new Promise((resolve, reject) => {
      (async () => {
        try {
          const response = await feedbackService.getFeedbackDetail(id as string);
          resolve(response);
        } catch (error) {
          setError(error);
          reject(error);
        }
      })();
    });
  }, [id, isTrigger]);

  const checkConditionPass = useCallback((response: ResponseGetDetailFeedback) => {
    //* Check condition of response here to set data
    if (!isEmpty(response?.data?.data)) {
      setData(response.data?.data);
    }
  }, []);

  const refetch = useCallback(async () => {
    try {
      setRefetching(true);
      signal.current = new AbortController();
      const response = await feedbackService.getFeedbackDetail(id as string);
      checkConditionPass(response);
    } catch (error: any) {
      showError(errorHandler(error));
    } finally {
      setRefetching(false);
    }
  }, [checkConditionPass, id]);

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

  return { data, loading, error, refetch, refetchWithLoading, setData, isRefetching };
};

export default useGetFeedbackDetail;
