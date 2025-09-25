import { useCallback, useEffect, useState } from 'react';
import faceCaptureService from '../../facecapture/facecapture.srvice';
import { ListStatistic } from '../interfaces/monitor.interface';

const useGetStatistic = (
  studentExamId: string,
  options: {
    isTrigger?: boolean;
  } = {
    isTrigger: true,
  },
) => {
  const { isTrigger = true } = options;
  const [data, setData] = useState<ListStatistic[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  const fetch = useCallback(async () => {
    if (!isTrigger) return;
    try {
      setIsLoading(true);
      const response = await faceCaptureService.viewStatistic(studentExamId);
      setData(response?.data?.data || []);
    } catch (error) {
      setData([]);
    } finally {
      setIsLoading(false);
    }
  }, [studentExamId, isTrigger]);

  useEffect(() => {
    fetch();
  }, [studentExamId, fetch]);

  return { data, isLoading };
};

export default useGetStatistic;
