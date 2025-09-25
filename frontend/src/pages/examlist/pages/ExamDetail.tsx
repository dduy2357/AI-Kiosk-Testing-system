import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { SIGNALR_URL } from '@/consts/apiUrl';
import BaseUrl from '@/consts/baseUrl';
import { formatTime, getQuestionButtonClass } from '@/helpers/common';
import { showError, showSuccess } from '@/helpers/toast';
import httpService, {
  EXAM_START_TIME_KEY,
  EXTRA_START_TIME_KEY,
  TIME_REMAINING_KEY,
} from '@/services/httpService';
import useGetDetailExam from '@/services/modules/studentexam/hooks/useGetExamDetail';
import studentexamService from '@/services/modules/studentexam/studentexam.service';
import createSignalRService from '@/services/signalRService';
import { AlertTriangle, Eye } from 'lucide-react';
import React, { startTransition, useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import CameraMonitor from '../components/CameraMonitor';
import QuestionCard from '../components/QuestionCard';
import QuestionNavigation from '../components/QuestionNavigation';
import TimerCard from '../components/TimerCard';
import useCameraMonitor from '../hooks/useCameraMonitor';
import useFaceDetection from '../hooks/useFaceDetection';
import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { useTranslation } from 'react-i18next';

const emotionColors = {
  angry: 'bg-red-500',
  disgust: 'bg-green-600',
  fear: 'bg-purple-600',
  happy: 'bg-green-500',
  neutral: 'bg-gray-400',
  sad: 'bg-blue-500',
  surprise: 'bg-pink-500',
};

const inferredStateColors: { [key: string]: string } = {
  Confident: 'bg-green-500',
  Relaxed: 'bg-green-500',
  Focused: 'bg-blue-500',
  Distracted: 'bg-yellow-500',
  Stressed: 'bg-red-500',
  Anxious: 'bg-red-500',
  Mixed: 'bg-gray-500',
};

const ExamDetail: React.FC = () => {
  const { t } = useTranslation('shared');
  const { examId } = useParams();
  const { data: examData, isLoading } = useGetDetailExam(examId, {
    isTrigger: !!examId,
  });
  const navigate = useNavigate();

  const [currentQuestion, setCurrentQuestion] = useState<string>('');
  const [selectedAnswers, setSelectedAnswers] = useState<{ [key: string]: string }>({});
  const [markedQuestions, setMarkedQuestions] = useState<Set<string>>(new Set());
  const [timeRemaining, setTimeRemaining] = useState<number>(0);
  const timerRef = useRef<NodeJS.Timeout | null>(null);
  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const signalRServiceRef = useRef<ReturnType<typeof createSignalRService> | null>(null);

  const { cameraStatus, showCameraWarning, cameraRef, cameraKey } = useCameraMonitor();
  const { emotionData, multipleFaceDetected, inferredState, errorMsg } = useFaceDetection(
    cameraStatus,
    cameraRef,
  );

  // Initialize SignalR connection
  useEffect(() => {
    const signalRService = createSignalRService(SIGNALR_URL);
    signalRServiceRef.current = signalRService;

    const initSignalR = async () => {
      try {
        connectionRef.current = await signalRService.start();
        signalRService.on('FinishStudentExam', (data: { examId: string; success: boolean }) => {
          if (data.examId === examId) {
            if (data.success) {
              showSuccess(t('ExamList.FinishExamSuccess'));
              navigate(BaseUrl.ExamList);
              localStorage.removeItem(`${TIME_REMAINING_KEY}_${examId}`);
              localStorage.removeItem(`${EXTRA_START_TIME_KEY}_${examId}`);
              localStorage.removeItem(`${EXAM_START_TIME_KEY}_${examId}`);
            } else {
              showError(t('ExamList.FinishExamError'));
            }
          } else {
            console.warn(
              `Received FinishStudentExam for wrong examId: ${data.examId}, expected: ${examId}`,
            );
          }
        });
        signalRService.on(
          'ReceiveExtraTime',
          (data: { studentExamId: string; newSubmitTime: string; extraMinutes: number }) => {
            if (data.studentExamId === httpService.getStudentIdStorage()) {
              const extraSeconds = data.extraMinutes * 60;
              setTimeRemaining((prev) => {
                const newTime = prev + extraSeconds;
                httpService.saveTimeRemainingStorage(examId ?? '', newTime);
                showSuccess(t('ExamList.ReceiveExtraTime', { minutes: data.extraMinutes }));
                return newTime;
              });
            } else {
              console.warn(
                `Received ReceiveExtraTime for wrong studentExamId: ${data.studentExamId}, expected: ${httpService.getStudentIdStorage()}`,
              );
            }
          },
        );
        signalRService.connection.onreconnected(() => {
          const newStudentExamId = httpService.getStudentIdStorage();
          if (examId && newStudentExamId) {
            signalRService
              .invoke('JoinExamGroup', examId, newStudentExamId)
              .then(() => console.log(`Rejoined exam group: ${examId}, ${newStudentExamId}`))
              .catch((error) => console.error('Failed to rejoin exam group:', error));
          }
        });
        const newStudentExamId = httpService.getStudentIdStorage();
        if (examId && newStudentExamId) {
          await signalRService.invoke('JoinExamGroup', examId, newStudentExamId);
        } else {
          console.warn('Missing examId or newStudentExamId:', { examId, newStudentExamId });
        }
      } catch (error) {
        console.error('Failed to initialize SignalR:', error);
      }
    };

    initSignalR();

    return () => {
      if (signalRServiceRef.current) {
        signalRServiceRef.current.stop();
      }
    };
  }, [examId, navigate, t]);

  const handleSubmit = useCallback(async () => {
    if (!examId || !examData) {
      showError(t('ExamList.NoDataFound'));
      return;
    }
    if (!httpService.getStudentIdStorage()) {
      showError(t('ExamList.NoStudentIdFound'));
      return;
    }
    try {
      const answers = Object.entries(selectedAnswers).map(([questionId, userAnswer]) => ({
        questionId,
        userAnswer,
      }));
      await studentexamService.submitExam(examId, answers, httpService.getStudentIdStorage());
      showSuccess(t('ExamList.SubmitExamSuccess'));

      localStorage.removeItem(`${TIME_REMAINING_KEY}_${examId}`);
      localStorage.removeItem(`${EXTRA_START_TIME_KEY}_${examId}`);
      localStorage.removeItem(`${EXAM_START_TIME_KEY}_${examId}`);

      setTimeout(() => {
        navigate(-1);
      }, 1000);
    } catch (error) {
      showError(error);
    }
  }, [examId, examData, selectedAnswers, navigate, t]);

  useEffect(() => {
    if (examData?.questions?.length && !currentQuestion) {
      startTransition(() => {
        setCurrentQuestion(examData.questions[0].questionId);
      });
      if (window.chrome && window.chrome.webview) {
        const message = {
          event: 'startExam',
          examId: examId,
          studentExamId: httpService.getStudentIdStorage() ?? '',
          timestamp: new Date().toISOString(),
          userId: httpService.getUserStorage()?.userID ?? '',
          token: httpService.getTokenStorage() ?? '',
        };
        window.chrome.webview.postMessage(message);
      }
    }
  }, [examData, currentQuestion, examId]);

  useEffect(() => {
    const fetchSavedAnswers = async () => {
      if (!examId) return;
      try {
        const response = await studentexamService.getSavedAnswers(examId);
        const savedAnswers = response?.data?.data ?? [];
        if (savedAnswers.length > 0) {
          startTransition(() => {
            const answersMap = savedAnswers.reduce(
              (
                acc: { [key: string]: string },
                answer: { questionId: string; userAnswer: string },
              ) => {
                acc[answer.questionId] = answer.userAnswer;
                return acc;
              },
              {},
            );
            setSelectedAnswers((prev) => ({ ...prev, ...answersMap }));
          });
        }
      } catch (error) {
        showError(error);
      }
    };
    fetchSavedAnswers();
  }, [examId]);

  useEffect(() => {
    if (!examId || !examData?.duration) return;

    const initialTime = examData.duration * 60;
    const startTimeKey = `${EXAM_START_TIME_KEY}_${examId}`;
    let startTime = localStorage.getItem(startTimeKey);

    if (!startTime) {
      startTime = new Date().toISOString();
      localStorage.setItem(startTimeKey, startTime);
    }

    const calculateTimeRemaining = () => {
      if (!startTime) return 0;
      const start = new Date(startTime).getTime();
      const now = new Date().getTime();
      const elapsedSeconds = Math.floor((now - start) / 1000);
      const remaining = initialTime - elapsedSeconds;
      return remaining > 0 ? remaining : 0;
    };

    startTransition(() => {
      setTimeRemaining(calculateTimeRemaining());
    });

    if (!timerRef.current) {
      timerRef.current = setInterval(() => {
        setTimeRemaining((prev) => {
          const remaining = prev - 1;
          if (remaining <= 0) {
            if (timerRef.current) clearInterval(timerRef.current);
            handleSubmit();
            return 0;
          }
          httpService.saveTimeRemainingStorage(examId, remaining);
          return remaining;
        });
      }, 1000);
    }

    return () => {
      if (timerRef.current) {
        clearInterval(timerRef.current);
        timerRef.current = null;
      }
    };
  }, [examId, examData?.duration, handleSubmit]);

  const toggleMarkQuestion = useCallback(() => {
    setMarkedQuestions((prev) => {
      const newSet = new Set(prev);
      if (newSet.has(currentQuestion)) {
        newSet.delete(currentQuestion);
      } else {
        newSet.add(currentQuestion);
      }
      return newSet;
    });
  }, [currentQuestion]);

  const handleSaveExam = useCallback(async () => {
    if (!examId || !examData) {
      showError(t('ExamList.NoDataFound'));
      return;
    }
    try {
      const answers = Object.entries(selectedAnswers).map(([questionId, userAnswer]) => ({
        questionId,
        userAnswer,
      }));
      await studentexamService.saveAnswerTemporary(
        examId,
        answers,
        httpService.getStudentIdStorage(),
      );
      showSuccess(t('ExamList.SaveTemporarySuccess'));
    } catch (error) {
      showError(error);
    }
  }, [examId, examData, selectedAnswers, t]);

  const handleNextQuestion = useCallback(() => {
    if (!examData?.questions) return;
    const currentIndex = examData.questions.findIndex((q) => q.questionId === currentQuestion);
    const nextIndex = (currentIndex + 1) % examData.questions.length;
    startTransition(() => {
      setCurrentQuestion(examData.questions[nextIndex].questionId);
    });
  }, [examData, currentQuestion]);

  const handlePrevQuestion = useCallback(() => {
    if (!examData?.questions) return;
    const currentIndex = examData.questions.findIndex((q) => q.questionId === currentQuestion);
    const prevIndex = (currentIndex - 1 + examData.questions.length) % examData.questions.length;
    startTransition(() => {
      setCurrentQuestion(examData.questions[prevIndex].questionId);
    });
  }, [examData, currentQuestion]);

  const currentQuestionData = examData?.questions?.find((q) => q.questionId === currentQuestion);
  const currentQuestionIndex =
    examData?.questions?.findIndex((q) => q.questionId === currentQuestion) ?? -1;

  if (!examData || !examData.questions?.length) {
    return <div className="p-4 text-center">Không tìm thấy dữ liệu bài thi.</div>;
  }

  return (
    <PageWrapper
      name="Chi tiết bài thi"
      className="bg-white dark:bg-gray-900"
      isLoading={isLoading}
    >
      <div className="relative min-h-screen bg-gray-50 p-4">
        {showCameraWarning && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm">
            <div className="mx-4 max-w-md rounded-xl bg-white p-6 shadow-2xl">
              <div className="text-center">
                <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
                  <AlertTriangle className="h-8 w-8 text-red-600" />
                </div>
                <h3 className="mb-2 text-xl font-bold text-gray-900">
                  {t('ExamList.CameraDisconnected')}
                </h3>
                <p className="mb-6 text-gray-600">{t('ExamList.CameraDisconnectedDescription')}</p>
                <div className="space-y-3">
                  <Button
                    onClick={() => window.location.reload()}
                    className="w-full bg-red-600 hover:bg-red-700"
                  >
                    {t('ExamList.ReloadPage')}
                  </Button>
                </div>
              </div>
            </div>
          </div>
        )}
        {showCameraWarning && (
          <div className="pointer-events-none fixed inset-0 z-40 bg-black/20"></div>
        )}
        <div
          className="mx-auto max-w-[100%]"
          style={{ pointerEvents: showCameraWarning ? 'none' : 'auto' }}
        >
          <div className="grid grid-cols-1 gap-4 lg:grid-cols-4">
            <div className="space-y-4 lg:col-span-1">
              <TimerCard
                timeRemaining={timeRemaining}
                formatTime={formatTime}
                onSubmit={handleSubmit}
                onSave={handleSaveExam}
                onNext={handleNextQuestion}
                onPrev={handlePrevQuestion}
                totalQuestions={examData.totalQuestions}
                currentQuestionIndex={
                  examData.questions.findIndex((q) => q.questionId === currentQuestion) + 1
                }
                totalDuration={examData.duration * 60}
              />
              <QuestionNavigation
                currentQuestion={currentQuestion}
                setCurrentQuestion={setCurrentQuestion}
                totalQuestions={examData.totalQuestions}
                getQuestionButtonClass={getQuestionButtonClass}
                questions={examData.questions}
                selectedAnswers={selectedAnswers}
                markedQuestions={markedQuestions}
                cameraRef={cameraRef}
                emotionData={emotionData}
                inferredState={inferredState}
              />
              <CameraMonitor
                cameraStatus={cameraStatus}
                cameraRef={cameraRef}
                cameraKey={cameraKey}
                emotionData={emotionData}
              />
              <Card className="border-0 shadow-lg">
                <CardHeader className="flex items-center justify-between">
                  <CardTitle className="flex items-center gap-2">
                    <Eye className="h-5 w-5" />
                    Emotion Analysis
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  {errorMsg && (
                    <div className="mb-6">
                      <div className="flex items-start gap-3 rounded-lg border border-red-200 bg-red-50 p-4 shadow-sm">
                        <div className="flex-shrink-0">
                          <AlertTriangle className="h-5 w-5 text-red-500" />
                        </div>
                        <div className="flex-1">
                          <h4 className="mb-1 text-sm font-medium text-red-800">Có lỗi xảy ra</h4>
                          <p className="text-sm text-red-700">
                            {errorMsg.replace(/DeepFace/g, 'Camera')}
                          </p>
                        </div>
                      </div>
                    </div>
                  )}
                  {multipleFaceDetected ? (
                    <div className="space-y-4">
                      <div className="text-center">
                        <Badge
                          variant="destructive"
                          className="px-4 py-2 text-lg"
                        >{`Phát hiện nhiều hơn 1 khuôn mặt`}</Badge>
                      </div>
                      <div className="border-t pt-2">
                        <div className="flex items-center gap-2">
                          <div className="h-2 w-2 rounded-full bg-red-500"></div>
                          <span className="text-sm font-medium text-red-700">
                            {multipleFaceDetected.status.toUpperCase()}: Chỉ được phép có một khuôn
                            mặt trong khung hình
                          </span>
                        </div>
                      </div>
                    </div>
                  ) : emotionData ? (
                    <>
                      <div className="mb-4 space-y-4 border-b pb-4">
                        <div className="text-center">
                          <Badge variant="secondary" className="px-4 py-2 text-lg">
                            {emotionData?.dominant_emotion &&
                              emotionData?.dominant_emotion.charAt(0).toUpperCase() +
                                emotionData?.dominant_emotion.slice(1)}
                          </Badge>
                        </div>
                        <div className="space-y-3">
                          {Object.entries(emotionData.emotions).map(([emotion, value]) => {
                            const emotionKey = emotion as keyof typeof emotionColors;
                            return (
                              <div key={emotion} className="space-y-1">
                                <div className="flex justify-between text-sm">
                                  <span className="font-medium capitalize">{emotion}</span>
                                  <span className="text-gray-600">{value.toFixed(1)}%</span>
                                </div>
                                <div className="h-2 w-full rounded-full bg-gray-200">
                                  <div
                                    className={`h-2 rounded-full transition-all duration-300 ${
                                      emotionColors[emotionKey] ?? 'bg-gray-400'
                                    }`}
                                    style={{ width: `${Math.min(value, 100)}%` }}
                                  ></div>
                                </div>
                              </div>
                            );
                          })}
                        </div>
                        <div className="border-t pt-2">
                          <div className="flex items-center gap-2">
                            <div className="h-2 w-2 rounded-full bg-green-500"></div>
                            <span className="text-sm font-medium text-green-700">
                              {emotionData.status}
                            </span>
                          </div>
                        </div>
                      </div>
                      {inferredState && (
                        <div className="space-y-4 pt-4">
                          <div className="text-center">
                            <h4 className="mb-2 text-base font-semibold">
                              Inferred State Analysis
                            </h4>
                            <Badge
                              className={`px-4 py-2 text-lg ${inferredStateColors[inferredState] ?? 'bg-gray-500'}`}
                            >
                              {inferredState}
                            </Badge>
                          </div>
                        </div>
                      )}
                    </>
                  ) : (
                    <div className="py-8 text-center">
                      <Eye className="mx-auto mb-3 h-12 w-12 text-gray-400" />
                      <p className="text-sm text-gray-500">
                        {emotionData ? 'Analyzing emotions...' : 'No face detected'}
                      </p>
                    </div>
                  )}
                </CardContent>
              </Card>
            </div>
            <div className="lg:col-span-3">
              <QuestionCard
                key={currentQuestion}
                currentQuestion={currentQuestion}
                currentQuestionData={currentQuestionData}
                markedQuestions={markedQuestions}
                toggleMarkQuestion={toggleMarkQuestion}
                selectedAnswer={selectedAnswers[currentQuestion] ?? ''}
                setSelectedAnswer={(answer: string) =>
                  setSelectedAnswers((prev) => ({ ...prev, [currentQuestion]: answer }))
                }
                questionNumber={currentQuestionIndex + 1}
              />
            </div>
          </div>
        </div>
      </div>
    </PageWrapper>
  );
};

export default React.memo(ExamDetail);
