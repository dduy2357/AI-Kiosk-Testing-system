import { useCallback, useEffect, useState } from 'react';
import { EssayList } from '../interfaces/studentexam.interface';
import { showError } from '@/helpers/toast';
import studentexamService from '../studentexam.service';

const useGetListEssay = (studentExamId: string, examId: string) => {
  const [data, setData] = useState<EssayList | null>(null);
  const [loading, setLoading] = useState(false);

  const fetchEssayList = useCallback(async () => {
    setLoading(true);
    try {
      const response = await studentexamService.getEssayExam(studentExamId, examId);
      setData(response.data.data);
    } catch (error) {
      showError(error);
    } finally {
      setLoading(false);
    }
  }, [studentExamId, examId]);

  useEffect(() => {
    if (studentExamId && examId) {
      fetchEssayList();
    }
  }, [studentExamId, examId, fetchEssayList]);
  return { data, loading, refetch: fetchEssayList };
};

export default useGetListEssay;
