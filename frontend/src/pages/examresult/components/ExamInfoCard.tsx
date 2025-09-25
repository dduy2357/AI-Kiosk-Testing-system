import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Calendar, Clock, FileText } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { format } from 'date-fns';
import { vi, enUS } from 'date-fns/locale';
import { formatDuration } from '@/utils/exam.utils';

interface ExamInfoCardProps {
  examTitle?: string;
  durationSpent?: number;
  totalQuestions?: number;
  startTime?: string;
  submitTime?: string;
}

export default function ExamInfoCard({
  examTitle,
  durationSpent,
  totalQuestions,
  startTime,
  submitTime,
}: Readonly<ExamInfoCardProps>) {
  const { t, i18n } = useTranslation('shared');
  const dateLocale = i18n.language === 'vi' ? vi : enUS;

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileText className="h-5 w-5" />
          {t('StudentExamResultDetail.examInfo')}
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div className="grid grid-cols-1 gap-4 text-sm md:grid-cols-2">
          <div>
            <p className="text-gray-500">{t('StudentExamResultDetail.examTitle')}</p>
            <p className="font-semibold">{examTitle ?? t('StudentExamResultDetail.noTitle')}</p>
          </div>
          <div>
            <p className="text-gray-500">
              {t('StudentExamResultDetail.durationSpent')}: {formatDuration(durationSpent ?? 0)}
            </p>
            <p className="font-semibold">
              {t('StudentExamResultDetail.totalQuestions')}: {totalQuestions ?? 0}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2 text-sm text-gray-500">
          <Calendar className="h-4 w-4" />
          <span>
            {t('StudentExamResultDetail.examDate')}:{' '}
            {startTime
              ? format(new Date(startTime), 'dd/MM/yyyy HH:mm:ss', { locale: dateLocale })
              : t('StudentExamResultDetail.noInfo')}
          </span>
        </div>

        {submitTime && (
          <div className="flex items-center gap-2 text-sm text-gray-500">
            <Clock className="h-4 w-4" />
            <span>
              {t('StudentExamResultDetail.submitTime')}:{' '}
              {format(new Date(submitTime), 'dd/MM/yyyy HH:mm:ss', { locale: dateLocale })}
            </span>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
