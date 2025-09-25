import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import type { DialogI } from '@/interfaces/common';
import useGetViewGuideLine from '@/services/modules/manageexam/hooks/useGetViewGuideLine';
import type { StudentExamList } from '@/services/modules/studentexam/interfaces/studentexam.interface';
import { BookOpen, Clock, FileText, AlertCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface DialogShowGuidelineProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: () => void;
  selectedExam?: StudentExamList;
}

const DialogShowGuideline = (props: DialogShowGuidelineProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, selectedExam } = props;

  const { guideLine } = useGetViewGuideLine(selectedExam?.examId ?? '', {
    isTrigger: isOpen,
  });

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/50 backdrop-blur-sm transition-all duration-300" />
      <DialogPortal>
        <DialogContent className="max-w-4xl overflow-hidden rounded-xl border-0 bg-white shadow-2xl">
          <div className="flex h-full flex-col">
            {/* Header */}
            <div className="relative bg-gradient-to-r from-blue-600 to-blue-700 px-8 py-6 text-white">
              <div className="absolute inset-0 bg-black/10"></div>
              <div className="relative z-10">
                <DialogTitle className="flex items-center justify-center gap-3 text-2xl font-bold">
                  <div className="rounded-full bg-white/20 p-2">
                    <BookOpen className="h-6 w-6" />
                  </div>
                  {t('Guidelines.Title')}
                </DialogTitle>
                {selectedExam && (
                  <div className="mt-3 text-center">
                    <p className="text-sm font-medium text-blue-100">
                      {t('Guidelines.ExamCode')}: {selectedExam.examId}
                    </p>
                  </div>
                )}
              </div>
            </div>

            {/* Content */}
            <div className="max-h-[calc(100vh-400px)] flex-1 overflow-y-auto p-4">
              {guideLine ? (
                <div className="p-8">
                  {/* Info Cards */}
                  <div className="mb-8 grid grid-cols-1 gap-4 md:grid-cols-3">
                    <div className="flex items-center gap-3 rounded-lg border border-blue-200 bg-blue-50 p-4">
                      <div className="rounded-full bg-blue-100 p-2">
                        <Clock className="h-5 w-5 text-blue-600" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-blue-900">{t('Guidelines.Time')}</p>
                        <p className="text-xs text-blue-700">{t('Guidelines.ReadCarefully')}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3 rounded-lg border border-green-200 bg-green-50 p-4">
                      <div className="rounded-full bg-green-100 p-2">
                        <FileText className="h-5 w-5 text-green-600" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-green-900">
                          {t('Guidelines.Regulations')}
                        </p>
                        <p className="text-xs text-green-700">{t('Guidelines.StrictCompliance')}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-3 rounded-lg border border-amber-200 bg-amber-50 p-4">
                      <div className="rounded-full bg-amber-100 p-2">
                        <AlertCircle className="h-5 w-5 text-amber-600" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-amber-900">{t('Guidelines.Note')}</p>
                        <p className="text-xs text-amber-700">{t('Guidelines.ReadCarefully')}</p>
                      </div>
                    </div>
                  </div>

                  {/* Guidelines Content */}
                  <div className="rounded-xl border border-gray-200 bg-gray-50 p-6">
                    <div className="prose prose-sm max-w-none">
                      <div
                        className="guideline-content leading-relaxed text-gray-800"
                        dangerouslySetInnerHTML={{ __html: guideLine }}
                      />
                    </div>
                  </div>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center px-8 py-16">
                  <div className="mb-4 rounded-full bg-gray-100 p-4">
                    <FileText className="h-8 w-8 text-gray-400" />
                  </div>
                  <h3 className="mb-2 text-lg font-semibold text-gray-900">
                    {t('Guidelines.Loading')}
                  </h3>
                  <p className="max-w-md text-center text-gray-600">
                    {t('Guidelines.LoadingDescription')}
                  </p>
                  <div className="mt-6 flex space-x-1">
                    <div className="h-2 w-2 animate-bounce rounded-full bg-blue-600"></div>
                    <div
                      className="h-2 w-2 animate-bounce rounded-full bg-blue-600"
                      style={{ animationDelay: '0.1s' }}
                    ></div>
                    <div
                      className="h-2 w-2 animate-bounce rounded-full bg-blue-600"
                      style={{ animationDelay: '0.2s' }}
                    ></div>
                  </div>
                </div>
              )}
            </div>

            {/* Footer */}
            <div className="border-t border-gray-200 bg-gray-50 px-8 py-6">
              <div className="flex items-center justify-between">
                <div className="flex items-center gap-2 text-sm text-gray-600">
                  <AlertCircle className="h-4 w-4" />
                  <span>{t('Guidelines.ReadCarefully')}</span>
                </div>

                <div className="flex gap-3">
                  <DialogClose asChild>
                    <Button
                      variant="outline"
                      type="button"
                      className="rounded-lg border-gray-300 bg-white px-6 py-2 font-medium text-gray-700 transition-all duration-200 hover:border-gray-400 hover:bg-gray-50"
                    >
                      {t('Close')}
                    </Button>
                  </DialogClose>
                  <Button
                    onClick={onSubmit}
                    className="rounded-lg bg-gradient-to-r from-blue-600 to-blue-700 px-6 py-2 font-medium text-white shadow-lg transition-all duration-200 hover:from-blue-700 hover:to-blue-800 hover:shadow-xl"
                  >
                    {t('Confirm')}
                  </Button>
                </div>
              </div>
            </div>
          </div>
        </DialogContent>
      </DialogPortal>
    </Dialog>
  );
};

export default DialogShowGuideline;
