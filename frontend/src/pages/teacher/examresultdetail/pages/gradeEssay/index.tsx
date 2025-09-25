import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Slider } from '@/components/ui/slider';
import { showError, showSuccess } from '@/helpers/toast';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetListEssay from '@/services/modules/studentexam/hooks/useGetListEssay';
import type { FormEssayValue } from '@/services/modules/studentexam/interfaces/studentexam.interface';
import studentexamService from '@/services/modules/studentexam/studentexam.service';
import { Form, Formik } from 'formik';
import { CheckCircle, Clock, FileText, Pen, Star } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

export default function GradeEssay() {
  // State
  const { examId, studentExamId } = useParams();
  const { data, loading } = useGetListEssay(studentExamId ?? '', examId ?? '');
  const [quickScore, setQuickScore] = useState<{ [questionId: string]: number }>({});
  const navigate = useNavigate();

  const isUserAnswer = data?.answers.some(
    (answer) => answer.userAnswer && answer.userAnswer !== null,
  );

  // Effects
  useEffect(() => {
    if (data && data.answers) {
      // Initialize quickScore with pointsEarned for all answers
      const initialScores = data.answers.reduce(
        (acc, answer) => ({
          ...acc,
          [answer.questionId]: answer.pointsEarned ?? 0,
        }),
        {},
      );
      setQuickScore(initialScores);
    }
  }, [data]);

  // Handlers
  const handleQuickScoreChange = (questionId: string, value: number[]) => {
    setQuickScore((prev) => ({ ...prev, [questionId]: value[0] }));
  };

  // Render
  if (!data && !loading) {
    return (
      <PageWrapper name="Chấm bài luận" className="bg-white dark:bg-gray-900" isLoading={loading}>
        <ExamHeader
          title="Chấm bài luận"
          subtitle="Chấm điểm bài luận của học sinh"
          icon={<Pen className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />
        <div className="flex h-full min-h-[calc(100vh-14rem)] items-center justify-center text-lg text-muted-foreground">
          Không tìm thấy bài luận.
        </div>
      </PageWrapper>
    );
  }

  return (
    <PageWrapper name="Chấm bài luận" className="bg-gray-50 dark:bg-gray-900" isLoading={loading}>
      <ExamHeader
        title={data?.examTitle ?? 'Chấm bài luận'}
        subtitle={`Chấm điểm bài luận của học sinh - ${data?.subjectName ?? ''}`}
        icon={<Pen className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
      />
      <div className="mx-auto grid grid-cols-1 gap-8 px-4 py-8 md:grid-cols-[1fr_360px] lg:px-6">
        {/* Left Column */}
        <div className="space-y-6">
          {/* Student and Exam Info Header */}
          <Card className="rounded-xl shadow-md">
            <CardContent className="flex flex-col items-center justify-between gap-4 p-6 sm:flex-row">
              <div className="flex items-center gap-4">
                <Avatar className="h-14 w-14 border-2 border-blue-500">
                  <AvatarImage
                    src={data?.studentAvatar ?? '/placeholder.svg?height=56&width=56'}
                    alt="Student Avatar"
                  />
                  <AvatarFallback className="bg-blue-100 text-xl font-semibold text-blue-700">
                    {data?.studentName?.charAt(0) ?? 'N/A'}
                  </AvatarFallback>
                </Avatar>
                <div>
                  <h2 className="text-xl font-bold text-gray-900 dark:text-gray-50">
                    {data?.studentName ?? 'N/A'}
                  </h2>
                  <p className="text-sm text-muted-foreground">
                    Mã HS: {data?.studentCode ?? 'N/A'} | Lớp: {data?.roomCode ?? 'N/A'}{' '}
                    <Badge
                      variant="outline"
                      className={`ml-2 px-2 py-1 text-xs font-medium ${
                        data?.isMarked
                          ? 'border-green-400 bg-green-50 text-green-800 dark:bg-green-900/20 dark:text-green-300'
                          : 'border-yellow-400 bg-yellow-50 text-yellow-800 dark:bg-yellow-900/20 dark:text-yellow-300'
                      }`}
                    >
                      {data?.isMarked ? 'Đã chấm' : 'Chưa chấm'}
                    </Badge>
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Exam Info Card */}
          <Card className="rounded-xl shadow-md">
            <CardHeader className="pb-4">
              <CardTitle className="flex items-center gap-2 text-xl font-semibold text-gray-900 dark:text-gray-50">
                <FileText className="h-6 w-6 text-blue-500" /> Thông tin kỳ thi
              </CardTitle>
            </CardHeader>
            <CardContent className="text-gray-700 dark:text-gray-300">
              <div className="grid grid-cols-1 gap-x-8 gap-y-2 sm:grid-cols-2">
                <p>
                  <strong>Mã kỳ thi:</strong> {data?.examId ?? 'N/A'}
                </p>
                <p>
                  <strong>Mã bài làm:</strong> {data?.studentExamId ?? 'N/A'}
                </p>
                <p>
                  <strong>Tên kỳ thi:</strong> {data?.examTitle ?? 'N/A'}
                </p>
                <p>
                  <strong>Môn học:</strong> {data?.subjectName ?? 'N/A'}
                </p>
                <p>
                  <strong>Phòng thi:</strong> {data?.roomCode ?? 'N/A'}
                </p>
                <p>
                  <strong>Ngày thi:</strong>{' '}
                  {data?.examDate ? new Date(data.examDate).toLocaleDateString() : 'N/A'}
                </p>
                <p>
                  <strong>Thời gian nộp bài:</strong>{' '}
                  {data?.submitTime ? new Date(data.submitTime).toLocaleString() : 'N/A'}
                </p>
                <p>
                  <strong>Thời gian làm bài:</strong>{' '}
                  {data?.durationSpent ? `${data.durationSpent} phút` : 'N/A'}
                </p>
                <p>
                  <strong>Tổng số câu hỏi:</strong> {data?.totalQuestions ?? 0}
                </p>
              </div>
            </CardContent>
          </Card>

          {/* Answers */}
          {data?.answers.map((answer, index) => (
            <div key={answer.questionId} className="space-y-6">
              {/* Question Card */}
              <Card className="rounded-xl shadow-md">
                <CardHeader className="flex flex-row items-center justify-between pb-4">
                  <CardTitle className="flex items-center gap-2 text-xl font-semibold text-gray-900 dark:text-gray-50">
                    <FileText className="h-6 w-6 text-blue-500" /> Câu {index + 1}
                  </CardTitle>
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <div className="flex items-center gap-1 text-gray-600 dark:text-gray-400">
                      <Clock className="h-4 w-4" /> {data?.durationSpent || 45} phút
                    </div>
                    <div className="font-medium text-gray-700 dark:text-gray-300">
                      Tối đa {answer.maxPoints} điểm
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="text-gray-800 dark:text-gray-200">
                  <p className="prose dark:prose-invert max-w-none">{answer.questionContent}</p>
                </CardContent>
              </Card>

              {/* Student Answer Card */}
              <Card className="rounded-xl shadow-md">
                <CardHeader className="flex flex-row items-center justify-between pb-4">
                  <CardTitle className="flex items-center gap-2 text-xl font-semibold text-gray-900 dark:text-gray-50">
                    <Pen className="h-6 w-6 text-purple-500" /> Bài làm của học sinh
                  </CardTitle>
                  <div className="flex items-center gap-4 text-sm text-muted-foreground">
                    <div className="text-gray-600 dark:text-gray-400">
                      Thời gian: {data?.durationSpent || 45} phút
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div
                    className="prose dark:prose-invert max-w-none rounded-lg border border-gray-200 bg-gray-50 p-5 text-gray-800 dark:border-gray-700 dark:bg-gray-800/50 dark:text-gray-200"
                    dangerouslySetInnerHTML={{ __html: answer.userAnswer || '' }}
                  />
                </CardContent>
              </Card>

              {/* Sample Answer Card */}
              <Card className="rounded-xl shadow-md">
                <CardHeader className="pb-4">
                  <CardTitle className="flex items-center gap-2 text-xl font-semibold text-gray-900 dark:text-gray-50">
                    <CheckCircle className="h-6 w-6 text-green-500" /> Đáp án mẫu
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div
                    className="prose dark:prose-invert max-w-none rounded-lg border border-green-300 bg-green-50 p-5 text-green-800 dark:border-green-700 dark:bg-green-900/30 dark:text-green-200"
                    dangerouslySetInnerHTML={{ __html: answer.correctAnswer || '' }}
                  />
                </CardContent>
              </Card>
            </div>
          ))}
        </div>

        {/* Right Column (Sidebar) */}
        <div className="space-y-6 lg:sticky lg:top-8 lg:self-start">
          <Formik
            validationSchema={null}
            initialValues={{ quickScore }}
            enableReinitialize
            onSubmit={async (values, { setSubmitting }) => {
              try {
                if (!studentExamId || !examId) {
                  throw new Error('Mã bài thi hoặc mã kỳ thi không hợp lệ.');
                }
                const payload: FormEssayValue = {
                  studentExamId,
                  examId,
                  scores: Object.entries(values.quickScore).map(([questionId, pointsEarned]) => ({
                    questionId,
                    pointsEarned,
                  })),
                };
                await studentexamService.markEssay(payload);
                showSuccess('Điểm đã được lưu thành công!');
                navigate(-1);
              } catch (error: any) {
                const errorMessage =
                  error.response?.data?.message ||
                  error.message ||
                  'Đã xảy ra lỗi khi lưu điểm. Vui lòng thử lại.';
                showError(errorMessage);
                setSubmitting(false);
              }
            }}
          >
            {({ isSubmitting, errors, touched }) => (
              <Form className="space-y-6">
                {/* Quick Grading Cards */}
                {data?.answers.map((answer) => (
                  <Card key={answer.questionId} className="rounded-xl shadow-md">
                    <CardHeader className="pb-4">
                      <CardTitle className="flex items-center gap-2 text-xl font-semibold text-gray-900 dark:text-gray-50">
                        <Star className="h-6 w-6 text-yellow-500" /> Chấm điểm nhanh
                      </CardTitle>
                      {errors.quickScore && touched.quickScore && (
                        <p className="mt-2 text-sm text-red-500">
                          {typeof errors.quickScore === 'string'
                            ? errors.quickScore
                            : 'Điểm không hợp lệ'}
                        </p>
                      )}
                    </CardHeader>
                    <CardContent className="space-y-5">
                      <div className="flex items-center gap-4">
                        <Slider
                          min={0}
                          max={answer.maxPoints}
                          step={0.5}
                          value={[quickScore[answer.questionId] ?? 0]}
                          onValueChange={(value) =>
                            handleQuickScoreChange(answer.questionId, value)
                          }
                          className="w-full [&>span:first-child>span]:h-2 [&>span:first-child>span]:rounded-full [&>span:first-child>span]:bg-blue-500 dark:[&>span:first-child>span]:bg-blue-600 [&>span:first-child]:h-2 [&>span:first-child]:rounded-full [&>span:first-child]:bg-blue-200 dark:[&>span:first-child]:bg-blue-800 [&>span:last-child]:h-5 [&>span:last-child]:w-5 [&>span:last-child]:rounded-full [&>span:last-child]:border-2 [&>span:last-child]:border-blue-500 [&>span:last-child]:bg-white dark:[&>span:last-child]:border-blue-600 dark:[&>span:last-child]:bg-gray-900"
                          disabled={isSubmitting}
                        />
                        <Input
                          type="number"
                          min={0}
                          max={answer.maxPoints}
                          step={0.5}
                          value={quickScore[answer.questionId] ?? 0}
                          onChange={(e) =>
                            handleQuickScoreChange(answer.questionId, [
                              Number.parseFloat(e.target.value) ?? 0,
                            ])
                          }
                          className="w-24 border-gray-300 text-center font-semibold text-gray-900 focus-visible:ring-blue-500 dark:border-gray-700 dark:text-gray-50"
                          disabled={isSubmitting}
                        />
                        <span className="text-lg font-medium text-muted-foreground">
                          /{answer.maxPoints}
                        </span>
                      </div>
                    </CardContent>
                  </Card>
                ))}
                <Button
                  type="submit"
                  className="w-full rounded-xl bg-blue-600 py-3 text-lg font-semibold text-white shadow-lg transition-colors duration-200 hover:bg-blue-700"
                  disabled={isSubmitting || !data?.answers.length || !isUserAnswer}
                >
                  <Pen className="mr-2 h-5 w-5" />
                  {isSubmitting ? 'Đang lưu...' : 'Lưu điểm'}
                </Button>
              </Form>
            )}
          </Formik>
        </div>
      </div>
    </PageWrapper>
  );
}
