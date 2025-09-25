import { useCallback, useEffect, useState } from 'react';
import manageExamService from '../manageExam.service';

const useGetViewGuideLine = (
  examId: string,
  options: {
    isTrigger?: boolean;
  } = {
    isTrigger: true,
  },
) => {
  const { isTrigger = true } = options;
  const [guideLine, setGuideLine] = useState<string>('');
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<string | null>(null);

  const fetchGuideLine = useCallback(async () => {
    if (!isTrigger || !examId) {
      return;
    }
    setLoading(true);
    try {
      const response = await manageExamService.viewGuideLines(examId);
      setGuideLine(response?.data?.guideLines);
    } catch (err) {
      setError('Failed to fetch guidelines');
    } finally {
      setLoading(false);
    }
  }, [examId, isTrigger]);

  useEffect(() => {
    if (examId) {
      fetchGuideLine();
    }
  }, [examId, fetchGuideLine]);

  return { guideLine, loading, error };
};

export default useGetViewGuideLine;
