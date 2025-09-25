import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Plus, Trash2 } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface Option {
  id: string;
  text: string;
  isCorrect: boolean;
}

interface MultipleChoiceEditorProps {
  options: Option[];
  onOptionsChange: (options: Option[]) => void;
}

export function MultipleChoiceEditor({
  options,
  onOptionsChange,
}: Readonly<MultipleChoiceEditorProps>) {
  const { t } = useTranslation('shared');

  const addOption = () => {
    const newOption: Option = {
      id: `option-${Date.now()}`,
      text: '',
      isCorrect: false,
    };
    onOptionsChange([...options, newOption]);
  };

  const removeOption = (id: string) => {
    if (options.length <= 2) return;
    onOptionsChange(options.filter((option) => option.id !== id));
  };

  const updateOptionText = (id: string, text: string) => {
    onOptionsChange(options.map((option) => (option.id === id ? { ...option, text } : option)));
  };

  const setCorrectOption = (id: string) => {
    onOptionsChange(
      options.map((option) => ({
        ...option,
        isCorrect: option.id === id,
      })),
    );
  };

  const getOptionLabel = (index: number) => {
    return String.fromCharCode(65 + index); // A, B, C, D...
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-medium">
          {t('AddQuestion.question_config_title')}
        </CardTitle>
        <p className="text-sm text-gray-500">{t('AddQuestion.question_config_description')}</p>
      </CardHeader>
      <CardContent>
        <div>
          <label className="mb-3 block text-sm font-medium text-gray-700">
            {t('AddQuestion.options_label')}
          </label>
          <p className="mb-4 text-xs text-gray-500">{t('AddQuestion.options_description')}</p>

          <RadioGroup
            value={options.find((o) => o.isCorrect)?.id}
            onValueChange={setCorrectOption}
            className="space-y-3"
          >
            {options.map((option, index) => (
              <div
                key={option.id}
                className="flex items-center space-x-3 rounded-lg border bg-white p-3"
              >
                <RadioGroupItem value={option.id} id={option.id} />
                <div className="flex flex-1 items-center space-x-2">
                  <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100">
                    <span className="text-sm font-medium text-gray-600">
                      {getOptionLabel(index)}
                    </span>
                  </div>
                  <Input
                    placeholder={`${t('AddQuestion.option_placeholder')} ${getOptionLabel(index)}`}
                    value={option.text}
                    onChange={(e) => updateOptionText(option.id, e.target.value)}
                    className="flex-1"
                  />
                </div>
                <div className="flex items-center space-x-1">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => removeOption(option.id)}
                    disabled={options.length <= 2}
                    className="text-red-500 hover:text-red-700"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
                {option.text.trim().length === 0 && (
                  <p className="absolute -bottom-5 left-12 text-xs text-red-500">
                    {t('AddQuestion.empty_option_error')}
                  </p>
                )}
              </div>
            ))}
          </RadioGroup>

          <Button
            variant="outline"
            onClick={addOption}
            className="mt-4 flex w-full items-center justify-center"
          >
            <Plus className="mr-2 h-4 w-4" />
            {t('AddQuestion.add_option_button')}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
