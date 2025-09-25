import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { t } from 'i18next';
import { CheckCircle, PenTool } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface QuestionType {
  id: string;
  title: string;
  description: string;
  icon: any;
  selectedColors: string;
  iconColor: string;
  titleColor: string;
  descColor: string;
}

interface QuestionTypeSelectorProps {
  selectedType: string;
  onTypeChange: (type: string) => void;
}

const questionTypes: QuestionType[] = [
  {
    id: 'multiple-choice',
    title: t('BankQuestion.MultipleChoice'),
    description: t('AddQuestion.MultipleDescription'),
    icon: CheckCircle,
    selectedColors: 'bg-green-50 border-green-200 text-green-700',
    iconColor: 'text-green-600',
    titleColor: 'text-green-900',
    descColor: 'text-green-700',
  },
  {
    id: 'essay',
    title: t('BankQuestion.Essay'),
    description: t('AddQuestion.EssayDescription'),
    icon: PenTool,
    selectedColors: 'bg-yellow-50 border-yellow-200 text-yellow-700',
    iconColor: 'text-yellow-600',
    titleColor: 'text-yellow-900',
    descColor: 'text-yellow-700',
  },
];

export function QuestionTypeSelector({
  selectedType,
  onTypeChange,
}: Readonly<QuestionTypeSelectorProps>) {
  const { t } = useTranslation('shared');
  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-medium">{t('AddQuestion.QuestionType')}</CardTitle>
        <p className="text-sm text-gray-500">{t('AddQuestion.SelectFormat')}</p>
      </CardHeader>
      <CardContent className="space-y-3">
        {questionTypes.map((type) => {
          const Icon = type.icon;
          const isSelected = selectedType === type.id;
          return (
            <button
              type="button"
              key={type.id}
              onClick={() => onTypeChange(type.id)}
              className={`w-full cursor-pointer rounded-lg border-2 p-4 text-left transition-all ${
                isSelected
                  ? `${type.selectedColors} shadow-sm`
                  : 'border-gray-200 bg-white hover:border-gray-300'
              }`}
            >
              <div className="flex items-start space-x-3">
                <Icon
                  className={`mt-0.5 h-5 w-5 ${isSelected ? type.iconColor : 'text-gray-400'}`}
                />
                <div className="flex-1">
                  <h3 className={`font-medium ${isSelected ? type.titleColor : 'text-gray-900'}`}>
                    {type.title}
                  </h3>
                  <p className={`mt-1 text-sm ${isSelected ? type.descColor : 'text-gray-500'}`}>
                    {type.description}
                  </p>
                </div>
              </div>
            </button>
          );
        })}
      </CardContent>
    </Card>
  );
}

export { questionTypes };
