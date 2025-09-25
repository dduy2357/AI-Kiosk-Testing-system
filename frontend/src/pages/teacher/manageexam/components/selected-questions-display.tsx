import { Button } from '@/components/ui/button';
import { ChevronDown, ChevronUp, X } from 'lucide-react';
import type React from 'react';

interface SelectedQuestion {
  questionId: string;
  content: string;
  difficulty?: string;
  questionBankName?: string;
  subjectName?: string;
}

interface SelectedQuestionsDisplayProps {
  questions: SelectedQuestion[];
  onRemoveQuestion: (questionId: string) => void;
  onMoveQuestion: (fromIndex: number, toIndex: number) => void;
}

const SelectedQuestionsDisplay: React.FC<SelectedQuestionsDisplayProps> = ({
  questions,
  onRemoveQuestion,
  onMoveQuestion,
}) => {
  if (questions.length === 0) {
    return null;
  }

  return (
    <div className="space-y-2">
      <div className="text-sm font-medium text-gray-700">Câu hỏi đã chọn ({questions.length})</div>
      <div className="max-h-60 space-y-2 overflow-y-auto">
        {questions.map((question, index) => (
          <div
            key={question.questionId}
            className="flex items-center gap-3 rounded-lg border bg-gray-50 p-3"
          >
            <div className="min-w-0 flex-1">
              <div className="mb-1 text-sm font-medium text-gray-900">{question.content}</div>
              <div className="text-xs text-gray-500">
                {question.subjectName ?? question.questionBankName ?? 'Toán học cơ bản'}
              </div>
            </div>

            <div className="flex flex-col gap-1">
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => onMoveQuestion(index, index - 1)}
                disabled={index === 0}
                className="h-6 w-6 p-0"
              >
                <ChevronUp className="h-3 w-3" />
              </Button>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => onMoveQuestion(index, index + 1)}
                disabled={index === questions.length - 1}
                className="h-6 w-6 p-0"
              >
                <ChevronDown className="h-3 w-3" />
              </Button>
            </div>

            <Button
              type="button"
              variant="ghost"
              size="sm"
              onClick={() => onRemoveQuestion(question.questionId)}
              className="h-6 w-6 p-0 text-red-500 hover:text-red-700"
            >
              <X className="h-3 w-3" />
            </Button>
          </div>
        ))}
      </div>
    </div>
  );
};

export default SelectedQuestionsDisplay;
