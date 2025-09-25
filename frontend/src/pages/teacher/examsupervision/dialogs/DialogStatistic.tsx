import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import type { DialogI } from '@/interfaces/common';
import useGetStatistic from '@/services/modules/monitor/hooks/useGetStatistic';
import { Form, Formik } from 'formik';
import { Fragment } from 'react';
import { useTranslation } from 'react-i18next';

interface DialogProps extends DialogI<any> {
  studentExamId: string | null;
}

const DialogStatistic = (props: DialogProps) => {
  const { isOpen, toggle, onSubmit, studentExamId } = props;
  const { t } = useTranslation('shared');

  const { data: emotionData, isLoading } = useGetStatistic(studentExamId ?? '', {
    isTrigger: !!studentExamId,
  });

  const emotionConfig = {
    angry: { emoji: '😠', color: 'text-red-600', bg: 'bg-red-50', border: 'border-red-200' },
    disgust: {
      emoji: '🤢',
      color: 'text-green-600',
      bg: 'bg-green-50',
      border: 'border-green-200',
    },
    fear: {
      emoji: '😨',
      color: 'text-purple-600',
      bg: 'bg-purple-50',
      border: 'border-purple-200',
    },
    happy: {
      emoji: '😊',
      color: 'text-yellow-600',
      bg: 'bg-yellow-50',
      border: 'border-yellow-200',
    },
    neutral: { emoji: '😐', color: 'text-gray-600', bg: 'bg-gray-50', border: 'border-gray-200' },
    sad: { emoji: '😢', color: 'text-blue-600', bg: 'bg-blue-50', border: 'border-blue-200' },
    surprise: {
      emoji: '😲',
      color: 'text-orange-600',
      bg: 'bg-orange-50',
      border: 'border-orange-200',
    },
  };

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay className="bg-black/60 backdrop-blur-sm" />
        <DialogContent className="max-h-[80vh] max-w-2xl overflow-hidden">
          <Formik initialValues={{}} onSubmit={onSubmit ?? (() => {})}>
            {() => {
              return (
                <Fragment>
                  <div className="flex items-center gap-3 border-b border-gray-100 pb-4">
                    <div className="flex h-10 w-10 items-center justify-center rounded-full bg-gradient-to-br from-blue-500 to-purple-600">
                      <span className="text-lg text-white">📊</span>
                    </div>
                    <div>
                      <DialogTitle className="text-xl font-semibold text-gray-900">
                        Emotion Statistics
                      </DialogTitle>
                      <p className="mt-1 text-sm text-gray-500">
                        Detailed analysis of emotional responses
                      </p>
                    </div>
                  </div>

                  <DialogDescription className="max-h-96 overflow-y-auto">
                    {isLoading && (
                      <div className="flex flex-col items-center justify-center py-12">
                        <div className="mb-4 h-8 w-8 animate-spin rounded-full border-4 border-blue-200 border-t-blue-600"></div>
                        <p className="font-medium text-gray-600">Loading statistics...</p>
                        <p className="mt-1 text-sm text-gray-400">
                          Please wait while we analyze the data
                        </p>
                      </div>
                    )}

                    {!isLoading && emotionData?.length > 0 && (
                      <div className="space-y-6 py-4">
                        {emotionData.map((stat, index) => {
                          // Ánh xạ dữ liệu để đảm bảo các khóa khớp với emotionConfig
                          const normalizedStat = {
                            angry: Number(stat.angry) || 0,
                            disgust: Number(stat.disgust) || 0,
                            fear: Number(stat.fear) || 0,
                            happy: Number(stat.happy) || 0,
                            neutral: Number(stat.neutral) || 0,
                            sad: Number(stat.sad) || 0,
                            surprise: Number(stat.surprise) || 0,
                            message: stat.message || '',
                          };

                          return (
                            <div
                              key={index}
                              className="rounded-xl border border-gray-200 bg-white shadow-sm transition-shadow duration-200 hover:shadow-md"
                            >
                              {/* Message header */}
                              <div className="rounded-t-xl border-b border-gray-100 bg-gradient-to-r from-gray-50 to-gray-100 px-6 py-4">
                                <div className="flex items-center gap-2">
                                  <span className="text-blue-600">💬</span>
                                  <p className="font-semibold text-gray-800">
                                    Message #{index + 1}
                                  </p>
                                </div>
                                <p className="mt-2 leading-relaxed text-gray-700">
                                  {normalizedStat.message}
                                </p>
                              </div>

                              {/* Emotions grid */}
                              <div className="p-6">
                                <h4 className="mb-4 text-sm font-medium uppercase tracking-wide text-gray-600">
                                  Emotion Analysis
                                </h4>
                                <div className="grid grid-cols-2 gap-3 lg:grid-cols-3">
                                  {Object.entries(emotionConfig).map(([emotion, config]) => {
                                    const rawValue =
                                      normalizedStat[emotion as keyof typeof normalizedStat] ?? 0;
                                    const total = Object.values(normalizedStat).reduce(
                                      (sum, value) =>
                                        Number(sum) +
                                        (typeof value === 'number' ? value : Number(value) || 0),
                                      0,
                                    );
                                    const totalNumber = Number(total) || 0;
                                    const rawValueNumber = Number(rawValue) || 0;
                                    const percentage =
                                      totalNumber > 0
                                        ? Math.ceil((rawValueNumber / totalNumber) * 100)
                                        : 0;

                                    console.log(
                                      `Emotion: ${emotion}, rawValue: ${rawValue}, total: ${total}, percentage: ${percentage}`,
                                    ); // Debug tính toán

                                    return (
                                      <div
                                        key={emotion}
                                        className={`${config.bg} ${config.border} rounded-lg border p-3 transition-all duration-200 hover:scale-105`}
                                      >
                                        <div className="mb-2 flex items-center justify-between">
                                          <div className="flex items-center gap-2">
                                            <span className="text-lg">{config.emoji}</span>
                                            <span
                                              className={`font-medium capitalize ${config.color}`}
                                            >
                                              {emotion}
                                            </span>
                                          </div>
                                          <span className={`text-sm font-bold ${config.color}`}>
                                            {percentage}%
                                          </span>
                                        </div>
                                        <div className="h-2 w-full overflow-hidden rounded-full bg-white">
                                          <div
                                            className={`h-full transition-all duration-500 ease-out ${config.color.replace('text-', 'bg-')}`}
                                            style={{ width: `${percentage}%` }}
                                          />
                                        </div>
                                      </div>
                                    );
                                  })}
                                </div>
                              </div>
                            </div>
                          );
                        })}
                      </div>
                    )}

                    {!isLoading && emotionData?.length === 0 && (
                      <div className="flex flex-col items-center justify-center py-12">
                        <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
                          <span className="text-2xl text-gray-400">📈</span>
                        </div>
                        <p className="mb-2 font-medium text-gray-600">No data available</p>
                        <p className="max-w-sm text-center text-sm text-gray-400">
                          There are no emotion statistics to display at this time. Please try again
                          later.
                        </p>
                      </div>
                    )}
                  </DialogDescription>

                  <Form className="flex justify-end gap-3 border-t border-gray-100 pt-6">
                    <Button
                      variant="ghost"
                      type="button"
                      onClick={toggle}
                      className="px-6 py-2 transition-colors duration-200 hover:bg-gray-100"
                    >
                      {t('Close')}
                    </Button>
                  </Form>
                </Fragment>
              );
            }}
          </Formik>
        </DialogContent>
      </DialogPortal>
    </Dialog>
  );
};

export default DialogStatistic;
