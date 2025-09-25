import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { AlbumIcon, XCircle } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import examresultdetailService from '@/services/modules/examresultdetail/examresultdetail.service';
import { ExamInfo } from '@/services/modules/examresultdetail/interfaces/examresultdetail.interface';
import ExamInfoCard from '../components/ExamInfoCard';
import QuestionsList from '../components/QuestionsList';
import ScoreStatistics from '../components/ScoreStatistics';
import StudentInfoCard from '../components/StudentInfoCard';
import { showError } from '@/helpers/toast';

export default function ExamResultDetail() {
  const { t } = useTranslation('shared');
  const { studentExamId } = useParams<{ studentExamId?: string }>();
  const [examInfo, setExamInfo] = useState<ExamInfo | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    const getData = async () => {
      setLoading(true);
      setError(null);
      try {
        if (!studentExamId) throw new Error(t('StudentExamResultDetail.invalidExamId'));
        const response = await examresultdetailService.getHistoryExamDetail(studentExamId);
        if (mounted) {
          setExamInfo(response.data.data);
        }
      } catch (err) {
        showError(err);
      } finally {
        if (mounted) setLoading(false);
      }
    };

    getData();
    return () => {
      mounted = false;
    };
  }, [studentExamId, t]);

  if (error) {
    return (
      <PageWrapper name={t('StudentExamResultDetail.title')} isLoading={false}>
        <div className="flex min-h-screen items-center justify-center">
          <div className="text-center">
            <XCircle className="mx-auto h-12 w-12 text-red-500" />
            <p className="mt-2 text-gray-600">{error}</p>
          </div>
        </div>
      </PageWrapper>
    );
  }

  return (
    <PageWrapper name={t('StudentExamResultDetail.title')} isLoading={loading}>
      <ExamHeader
        title={t('StudentExamResultDetail.headerTitle')}
        subtitle={t('StudentExamResultDetail.headerSubtitle')}
        icon={<AlbumIcon className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
      />
      <div className="min-h-screen bg-gray-50 p-4">
        <div className="mx-auto max-w-full">
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
            {/* Student Information */}
            <div className="lg:col-span-1">
              <StudentInfoCard
                studentName={examInfo?.studentName}
                studentCode={examInfo?.studentCode}
              />
            </div>

            {/* Exam Information */}
            <div className="lg:col-span-2">
              <ExamInfoCard
                examTitle={examInfo?.examTitle}
                durationSpent={examInfo?.durationSpent}
                totalQuestions={examInfo?.totalQuestions}
                startTime={examInfo?.startTime}
                submitTime={examInfo?.submitTime}
              />
            </div>
          </div>

          {/* Score Section */}
          {examInfo && (
            <ScoreStatistics
              score={examInfo.score}
              totalCorrectAnswers={examInfo.totalCorrectAnswers}
              totalWrongAnswers={examInfo.totalWrongAnswers}
              totalQuestions={examInfo.totalQuestions}
              durationSpent={examInfo.durationSpent}
            />
          )}

          {/* Questions Section */}
          {examInfo?.answers && <QuestionsList answers={examInfo.answers} />}
        </div>
      </div>
    </PageWrapper>
  );
}
