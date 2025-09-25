import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import ClassicEditor from '@ckeditor/ckeditor5-build-classic';
import { CKEditor } from '@ckeditor/ckeditor5-react';
import { Star } from 'lucide-react';
import { useCallback } from 'react';
import { useTranslation } from 'react-i18next';

interface QuestionCardProps {
  currentQuestion: string;
  currentQuestionData?: {
    questionId: string;
    content: string;
    questionType: string;
    point: number;
    difficultLevel: number;
    options: string[];
  };
  markedQuestions: Set<string>;
  toggleMarkQuestion: () => void;
  selectedAnswer: string;
  setSelectedAnswer: (answer: string) => void;
  questionNumber: number;
}

const QuestionCard: React.FC<QuestionCardProps> = ({
  currentQuestion,
  currentQuestionData,
  markedQuestions,
  toggleMarkQuestion,
  selectedAnswer,
  setSelectedAnswer,
  questionNumber,
}) => {
  const { t } = useTranslation('shared');
  const handleOptionSelect = useCallback(
    (answer: string) => {
      setSelectedAnswer(answer);
    },
    [setSelectedAnswer],
  );

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <CardTitle className="text-lg">
            {t('ExamList.Question')} {questionNumber} (
            {currentQuestionData?.questionType ?? 'MultipleChoice'})
          </CardTitle>
          <Button
            variant="outline"
            size="sm"
            onClick={toggleMarkQuestion}
            className={markedQuestions.has(currentQuestion) ? 'bg-yellow-100' : ''}
          >
            <Star className="mr-1 h-4 w-4" />
            {t('ExamList.Mark')}
          </Button>
        </div>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="text-base leading-relaxed">
          {currentQuestionData?.content ?? 'Question not found'}
        </div>
        {currentQuestionData?.questionType === 'Essay' ? (
          <div className="space-y-3">
            <Label htmlFor="essay-answer" className="text-sm font-medium">
              {t('ExamList.YourAnswer')}
            </Label>
            <CKEditor
              editor={ClassicEditor as any}
              data={selectedAnswer ?? ''}
              onChange={(_event: any, editor: any) => {
                const data = editor.getData();
                setSelectedAnswer(data);
              }}
              config={{
                toolbar: [
                  'heading',
                  '|',
                  'bold',
                  'italic',
                  'link',
                  'bulletedList',
                  'numberedList',
                  '|',
                  'undo',
                  'redo',
                ],
                placeholder: 'Enter your essay answer here...',
              }}
              onReady={(editor) => {
                if (editor.editing.view.document.getRoot()) {
                  editor.editing.view.change((writer: any) => {
                    const root = editor.editing.view.document.getRoot();
                    if (root) {
                      writer.setStyle('min-height', '150px', root);
                    }
                  });
                }
              }}
            />
          </div>
        ) : currentQuestionData?.options?.length ? (
          <RadioGroup
            value={selectedAnswer}
            onValueChange={handleOptionSelect}
            className="space-y-3"
          >
            {currentQuestionData.options.map((option, index) => (
              <div
                key={index}
                className="flex cursor-pointer items-start space-x-3 rounded-lg border p-3 hover:bg-gray-50"
              >
                <RadioGroupItem value={option} id={`option-${index}`} className="mt-1" />
                <Label
                  htmlFor={`option-${index}`}
                  className="flex-1 cursor-pointer leading-relaxed"
                >
                  {option}
                </Label>
              </div>
            ))}
          </RadioGroup>
        ) : (
          <div className="text-sm text-gray-500">No options available</div>
        )}
      </CardContent>
    </Card>
  );
};

export default QuestionCard;
