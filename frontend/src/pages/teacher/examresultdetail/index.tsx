import { useParams } from 'react-router-dom';
import { ExamHeader } from './component/exam-header';
import { ExamOverview } from './component/exam-overview';
import { ScoreDistribution } from './component/score-distribution';
import { StudentResults } from './component/student-results';

import { useEffect, useMemo, useState } from 'react';
import useGetResultReportForTeacher from '@/services/modules/examresultdetail/hooks/useGetResultReportForTeacher';
import useGetResultDetailForTeacher from '@/services/modules/examresultdetail/hooks/useGetResultDetailForTeacher';
import {
  StudentResult,
  StudentResultDetail,
} from '@/services/modules/examresultdetail/interfaces/examresultdetail.interface';

export default function ExamResultsDetail() {
  const { examId } = useParams();
  const [examData, setExamData] = useState({
    title: '',
    subtitle: '',
    subject: '',
    date: '',
    duration: 0,
    totalQuestions: 0,
    maxScore: 0,
    teacher: '',
    stats: {
      totalStudents: 0,
      averageScore: 0,
      passRate: 0,
    },
  });

  const { data: resultReportResponse } = useGetResultReportForTeacher(examId);
  const resultReport = resultReportResponse?.data;
  const { data: resultDetailResponse } = useGetResultDetailForTeacher(examId);
  const resultDetail = resultDetailResponse?.data?.data;

  useEffect(() => {
    if (examId && resultReport && resultDetail) {
      setExamData({
        title: resultDetail?.title ?? '',
        subtitle: 'Báo cáo chi tiết kết quả thi',
        subject: resultReport?.subjectName ?? '',
        date: resultReport?.examDate ?? '',
        duration: resultDetail?.duration ?? 0,
        totalQuestions: resultDetail?.questions?.length ?? 0,
        maxScore: 10,
        teacher: resultReport?.createdBy ?? '',
        stats: {
          totalStudents: resultReport?.studentResults.length,
          averageScore:
            resultReport?.studentResults.length === 0
              ? 0
              : resultReport?.studentResults.reduce(
                  (sum: number, value: StudentResult) => sum + value.score,
                  0,
                ) / resultReport.studentResults.length,
          passRate:
            (resultReport?.studentResults.filter((s) => s.score >= 5).length /
              resultReport?.studentResults.length) *
              100 || 0,
        },
      });
    } else if (!examId) {
      throw new Error('examId is undefined');
    }
  }, [examId, resultReport, resultDetail]);

  const studentResultsArr: { score: number }[] = resultReport?.studentResults ?? [];
  const total = studentResultsArr.length;
  const scoreDistribution = [
    {
      range: '9.0 - 10',
      count: studentResultsArr.filter((s) => s.score >= 9.0).length,
      color: 'bg-green-500',
    },
    {
      range: '8.0 - 8.9',
      count: studentResultsArr.filter((s) => s.score >= 8.0 && s.score < 9.0).length,
      color: 'bg-blue-500',
    },
    {
      range: '7.0 - 7.9',
      count: studentResultsArr.filter((s) => s.score >= 7.0 && s.score < 8.0).length,
      color: 'bg-yellow-500',
    },
    {
      range: '6.0 - 6.9',
      count: studentResultsArr.filter((s) => s.score >= 6.0 && s.score < 7.0).length,
      color: 'bg-orange-500',
    },
    {
      range: '5.0 - 5.9',
      count: studentResultsArr.filter((s) => s.score >= 5.0 && s.score < 6.0).length,
      color: 'bg-red-400',
    },
    {
      range: '< 5.0',
      count: studentResultsArr.filter((s) => s.score < 5.0).length,
      color: 'bg-red-600',
    },
  ].map((group) => ({
    ...group,
    percentage: total === 0 ? 0 : +((group.count / total) * 100).toFixed(1),
  }));

  const studentResults: StudentResultDetail[] = useMemo(() => {
    const results = resultReport?.studentResults ?? [];
    return results.map((result: StudentResult) => ({
      fullname: result.fullName ?? 'Unknown',
      className: result.className ?? 'N/A',
      gradingStatus: result.gradingStatus ?? 'Graded',
      score: result.score ?? 0,
      submitTime: result.submitTime ?? 'N/A',
      status: result.status ?? 'Submitted',
      workingTime: result.workingTime ?? '0 minutes',
      questionType: result.questionType ?? '',
      studentExamId: result.studentExamId ?? '',
    }));
  }, [resultReport?.studentResults]);
  return (
    <div className="min-h-screen bg-gray-50">
      <ExamHeader examData={examData} examId={examId ?? ''} />

      <div className="space-y-6 p-6">
        <ExamOverview stats={examData.stats} />

        <div className="grid grid-cols-1 gap-6">
          <ScoreDistribution data={scoreDistribution} />
        </div>

        <StudentResults
          data={studentResults}
          questionType={resultReport?.questionType}
          resultDetail={resultDetail}
        />
      </div>
    </div>
  );
}
