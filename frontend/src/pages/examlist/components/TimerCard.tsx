import DialogConfirm from '@/components/dialogs/DialogConfirm';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Progress } from '@/components/ui/progress'; // Import Progress component
import useToggleDialog from '@/hooks/useToggleDialog';
import { ChevronLeft, ChevronRight, RotateCcw, Save, Send } from 'lucide-react';
import type React from 'react';
import { useTranslation } from 'react-i18next';

interface TimerCardProps {
  timeRemaining: number;
  formatTime: (seconds: number) => string;
  onSubmit?: () => void;
  onSave?: () => void;
  onNext?: () => void;
  onPrev?: () => void;
  totalQuestions: number;
  currentQuestionIndex: number;
  totalDuration: number;
}

const TimerCard: React.FC<TimerCardProps> = ({
  timeRemaining,
  formatTime,
  onSubmit,
  onSave,
  onNext,
  onPrev,
  totalQuestions,
  currentQuestionIndex,
  totalDuration, // Destructure totalDuration
}) => {
  const { t } = useTranslation('shared');
  const [openAskConfirmSubmitExam, toggleAskConfirmSubmitExam, shouldRenderAskConfirmSubmitExam] =
    useToggleDialog();

  const progressPercentage = (timeRemaining / totalDuration) * 100;

  const getProgressBarColorClass = (progress: number) => {
    if (progress > 50) {
      return 'bg-green-500';
    } else if (progress > 20) {
      return 'bg-yellow-500';
    } else {
      return 'bg-red-500';
    }
  };

  const progressBarColorClass = getProgressBarColorClass(progressPercentage);

  return (
    <Card>
      {shouldRenderAskConfirmSubmitExam && (
        <DialogConfirm
          isOpen={openAskConfirmSubmitExam}
          toggle={toggleAskConfirmSubmitExam}
          title={t('ExamList.ConfirmSubmitExam')}
          content={t('ExamList.ConfirmSubmitExamDescription')}
          onSubmit={onSubmit}
          variantYes="destructive"
        />
      )}
      <CardHeader className="pb-4">
        <div className="flex items-center justify-between">
          <div className="text-sm text-muted-foreground">{t('ExamList.TimeRemaining')}</div>
        </div>
        <div className="text-2xl font-bold text-blue-600">{formatTime(timeRemaining)}</div>
        <Progress value={progressPercentage} className={`h-2 ${progressBarColorClass}`} />{' '}
        {/* Add Progress bar */}
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="space-y-2">
          <div className="grid grid-cols-3 gap-2">
            <Button
              variant="outline"
              size="sm"
              className="bg-transparent text-xs"
              onClick={onPrev}
              disabled={currentQuestionIndex === 1}
            >
              <ChevronLeft className="mr-1 h-3 w-3" />
              {t('ExamList.PreviousPage')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              onClick={() => window.location.reload()}
              className="text-xs"
            >
              <RotateCcw className="mr-1 h-3 w-3" />
              {t('ExamList.ReloadPage')}
            </Button>
            <Button
              variant="outline"
              size="sm"
              className="bg-transparent text-xs"
              onClick={onNext}
              disabled={currentQuestionIndex === totalQuestions}
            >
              {t('ExamList.NextPage')}
              <ChevronRight className="ml-1 h-3 w-3" />
            </Button>
          </div>
          <div className="grid grid-cols-2 gap-2">
            <Button
              variant="outline"
              size="sm"
              className="bg-transparent text-xs"
              onClick={toggleAskConfirmSubmitExam}
            >
              <Save className="mr-1 h-3 w-3" />
              {t('ExamList.SubmitExam')}
            </Button>
            <Button variant="outline" size="sm" className="bg-transparent text-xs" onClick={onSave}>
              <Send className="mr-1 h-3 w-3" />
              {t('ExamList.SaveExam')}
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  );
};

export default TimerCard;
