import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useTranslation } from 'react-i18next';

interface EssayEditorProps {
  guidance: string;
  onGuidanceChange: (guidance: string) => void;
}

export function EssayEditor({ guidance, onGuidanceChange }: Readonly<EssayEditorProps>) {
  const { t } = useTranslation('shared');

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-medium">
          {t('AddQuestion.question_config_title')}
        </CardTitle>
        <p className="text-sm text-gray-500">{t('AddQuestion.essay_config_description')}</p>
      </CardHeader>
      <CardContent>
        <div>
          <label className="mb-2 block text-sm font-medium text-gray-700">
            {t('AddQuestion.grading_guidance_label')}
          </label>
          <textarea
            placeholder={t('AddQuestion.grading_guidance_placeholder')}
            value={guidance}
            onChange={(e) => onGuidanceChange(e.target.value)}
            className="min-h-[120px] w-full rounded-md border p-2"
          />
          <p className="mt-2 text-xs text-gray-500">
            {t('AddQuestion.grading_guidance_description')}
          </p>
        </div>
      </CardContent>
    </Card>
  );
}
