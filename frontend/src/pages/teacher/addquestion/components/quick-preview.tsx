import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { FileText } from 'lucide-react';
import { questionTypes } from './question-type-selector';
import { useTranslation } from 'react-i18next';

interface QuickPreviewProps {
  selectedType: string;
  content: string;
  imageUrl?: string | null;
}

export function QuickPreview({ selectedType, content, imageUrl }: Readonly<QuickPreviewProps>) {
  const currentType = questionTypes.find((t) => t.id === selectedType);
  const { t } = useTranslation('shared');
  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center text-lg font-medium">
          <FileText className="mr-2 h-5 w-5" />
          {t('AddQuestion.QuickView')}
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500">{t('AddQuestion.Status')}</span>
            <Badge variant="outline" className="border-orange-200 text-orange-600">
              {t('AddQuestion.Drafting')}
            </Badge>
          </div>
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500">{t('AddQuestion.QuestionType')}</span>
            <span className="font-medium">{currentType?.title ?? t('AddQuestion.NotSelect')}</span>
          </div>
          <div className="flex items-center justify-between text-sm">
            <span className="text-gray-500">{t('AddQuestion.Length')}</span>
            <span className="font-medium">
              {content.length} {t('AddQuestion.Characters')}
            </span>
          </div>

          <Separator />

          <div className="text-sm">
            <span className="mb-2 block text-gray-500">{t('AddQuestion.PreviewContent')}</span>
            <div className="min-h-[60px] space-y-3 rounded border bg-gray-50 p-3">
              {content ? (
                <p className="text-gray-700">{content}</p>
              ) : (
                <p className="italic text-gray-400">{t('AddQuestion.EnterContentPreview')}</p>
              )}
              {imageUrl && (
                <img
                  src={imageUrl}
                  alt="Câu hỏi minh họa"
                  className="mx-auto max-h-40 object-contain"
                />
              )}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
