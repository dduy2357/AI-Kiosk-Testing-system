import { Badge } from '@/components/ui/badge';
import { CheckCircle, XCircle } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface AnswerBadgeProps {
  type: 'correct' | 'incorrect' | 'user-correct' | 'user-incorrect';
  size?: 'sm' | 'md';
}

export default function AnswerBadge({ type, size = 'md' }: Readonly<AnswerBadgeProps>) {
  const { t } = useTranslation('shared');
  const sizeClass = size === 'sm' ? 'text-xs' : '';

  switch (type) {
    case 'correct':
      return (
        <Badge variant="secondary" className={`bg-green-100 text-green-800 ${sizeClass}`}>
          <CheckCircle className="mr-1 h-3 w-3" />
          {t('StudentExamResultDetail.correct')}
        </Badge>
      );
    case 'user-correct':
      return (
        <Badge variant="secondary" className={`bg-green-100 text-green-800 ${sizeClass}`}>
          <CheckCircle className="mr-1 h-3 w-3" />
          {t('StudentExamResultDetail.user-correct')}
        </Badge>
      );
    case 'user-incorrect':
      return (
        <Badge variant="destructive" className={sizeClass}>
          <XCircle className="mr-1 h-3 w-3" />
          {t('StudentExamResultDetail.user-incorrect')}
        </Badge>
      );
    default:
      return null;
  }
}
