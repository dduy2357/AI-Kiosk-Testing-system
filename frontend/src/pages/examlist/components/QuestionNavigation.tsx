import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { showError } from '@/helpers/toast';
import httpService from '@/services/httpService';
import faceCaptureService from '@/services/modules/facecapture/facecapture.srvice';
import { useCallback } from 'react';
import { AnalyzeFaceResponse } from '../hooks/useFaceDetection';
import { useTranslation } from 'react-i18next';

interface QuestionNavigationProps {
  currentQuestion: string;
  setCurrentQuestion: (questionId: string) => void;
  totalQuestions: number;
  getQuestionButtonClass: (isCurrent: boolean, isSelected: boolean, isMarked: boolean) => string;
  questions: { questionId: string }[];
  selectedAnswers?: { [key: string]: string };
  markedQuestions: Set<string>;
  cameraRef: React.MutableRefObject<any>;
  emotionData: AnalyzeFaceResponse | null;
  inferredState?: string | null;
}

const QuestionNavigation: React.FC<QuestionNavigationProps> = ({
  currentQuestion,
  setCurrentQuestion,
  getQuestionButtonClass,
  questions,
  selectedAnswers,
  markedQuestions,
  cameraRef,
  emotionData,
  inferredState,
}) => {
  const { t } = useTranslation('shared');
  const handleQuestionNavigation = useCallback(
    async (questionId: string) => {
      setCurrentQuestion(questionId);

      if (!cameraRef.current) {
        showError('Camera is not available');
        return;
      }

      try {
        await cameraRef.current.video?.play();

        const base64Image = cameraRef.current.takePhoto();
        if (!base64Image) {
          showError('Failed to capture image');
          return;
        }

        const response = await fetch(base64Image);
        const blob = await response.blob();
        const file = new File(
          [blob],
          `camera-screenshot-${Date.now()}-${Math.random().toString(36).substring(2, 8)}.jpg`,
          {
            type: 'image/jpeg',
          },
        );

        const formData = new FormData();
        formData.append('StudentExamId', httpService.getStudentIdStorage() ?? '');
        formData.append('ImageCapture', file);
        formData.append('Description', 'Auto-captured during question navigation');
        formData.append(
          'CaptureId',
          `capture-${Date.now()}-${Math.random().toString(36).substring(2, 8)}`,
        );
        formData.append('AvgArousal', emotionData?.avg_arousal?.toString() ?? '0');
        formData.append('AvgValence', emotionData?.avg_valence?.toString() ?? '0');
        formData.append('DominantEmotion', emotionData?.dominant_emotion ?? '');
        formData.append(
          'Emotions',
          emotionData?.emotions ? JSON.stringify(emotionData.emotions) : '{}',
        );
        formData.append('InferredState', emotionData?.inferred_state ?? inferredState ?? '');
        formData.append('Region', emotionData?.region ? JSON.stringify(emotionData.region) : '{}');
        formData.append('Result', emotionData?.result ?? '');
        formData.append('Status', emotionData?.status ?? '');
        formData.append('IsDetected', emotionData?.region ? 'true' : 'false');

        await faceCaptureService.addFaceCapture(formData);
      } catch (error) {
        showError(error);
      }
    },
    [setCurrentQuestion, cameraRef, emotionData, inferredState],
  );

  return (
    <Card>
      <CardHeader>
        <div className="text-sm font-medium">{t('ExamList.QuestionNavigation')}</div>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-5 gap-2">
          {questions.map((q, index) => {
            const isSelected = !!(selectedAnswers && selectedAnswers[q.questionId]);
            const isMarked = markedQuestions.has(q.questionId);
            return (
              <Button
                key={q.questionId}
                variant="outline"
                size="sm"
                className={getQuestionButtonClass(
                  q.questionId === currentQuestion,
                  isSelected,
                  isMarked,
                )}
                onClick={() => handleQuestionNavigation(q.questionId)}
              >
                {index + 1}
              </Button>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
};

export default QuestionNavigation;
