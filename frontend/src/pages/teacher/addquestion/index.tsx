import { Button } from '@/components/ui/button';
import { essay, MAX_QUESTION_LENGTH, multipleChoice } from '@/consts/common';
import { showError, showSuccess } from '@/helpers/toast';
import questionService from '@/services/modules/question/question.service';
import { useState } from 'react';
import { useLocation } from 'react-router-dom';
import { AdditionalInfoSection } from './components/additional-info-section';
import { EssayEditor } from './components/essay-editor';
import { Header } from './components/header';
import { MultipleChoiceEditor } from './components/multiple-choice-editor';
import { ProgressStepper } from './components/progress-stepper';
import { QuestionBankSelector } from './components/question-bank-selector';
import QuestionContentEditor from './components/question-content-editor';
import { QuestionTypeSelector } from './components/question-type-selector';
import { QuickPreview } from './components/quick-preview';
import { SavedQuestionsList } from './components/saved-questions-list';
import { TipsPanel } from './components/tips-panel';
import { useTranslation } from 'react-i18next';
import amazonServices from '@/services/modules/amazon/amazon.Services';

const convertApiToState = (options: string[], correctAnswer: string) => {
  if (!options || options.length === 0) {
    return [
      { id: 'option-1', text: '', isCorrect: true },
      { id: 'option-2', text: '', isCorrect: false },
      { id: 'option-3', text: '', isCorrect: false },
      { id: 'option-4', text: '', isCorrect: false },
    ];
  }
  return options.map((text: string, index: number) => ({
    id: `option-${index + 1}`,
    text: text,
    isCorrect: text === correctAnswer,
  }));
};

export default function QuestionManagement() {
  const { t } = useTranslation('shared');
  const location = useLocation();
  const { question, refetch } = location.state ?? {};

  const [currentStep, setCurrentStep] = useState(1);
  const [selectedBank, setSelectedBank] = useState(question?.questionBankId ?? '');
  const [selectedQuestionType, setSelectedQuestionType] = useState(
    question ? (question?.options.length !== 0 ? 'multiple-choice' : 'essay') : 'multiple-choice',
  );
  const [questionContent, setQuestionContent] = useState(question?.content ?? '');
  const [imageupload, setImageUpload] = useState(question?.objectFile ?? null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState<string | null>(
    question?.objectFile ?? null,
  );
  const [multipleChoiceOptions, setMultipleChoiceOptions] = useState(
    convertApiToState(question?.options, question?.correctAnswer),
  );
  const [essayGuidance, setEssayGuidance] = useState('');
  const [difficulty, setDifficulty] = useState(question?.difficultLevel ?? 1);
  const [subject, setSubject] = useState(question?.subjectId ?? '');
  const [points, setPoints] = useState(1);
  const [explanation, setExplanation] = useState(question?.explanation ?? '');
  const [tag, setTag] = useState('');
  const [selectedSubjectName, setSelectedSubjectName] = useState('');

  const steps = [
    {
      id: 1,
      title: t('AddQuestion.Step1'),
      completed: false,
      current: currentStep === 1,
    },
    {
      id: 2,
      title: t('AddQuestion.Step2'),
      completed: false,
      current: currentStep === 2,
    },
    {
      id: 3,
      title: t('AddQuestion.Step3'),
      completed: false,
      current: currentStep === 3,
    },
  ];

  const hasDuplicateOptions = () => {
    const nonEmptyOptions = multipleChoiceOptions
      .map((opt) => opt.text.trim())
      .filter((text) => text.length > 0);
    return new Set(nonEmptyOptions).size !== nonEmptyOptions.length;
  };

  const canProceedFromStep1 =
    selectedBank &&
    questionContent.trim().length > 0 &&
    questionContent.length <= MAX_QUESTION_LENGTH;

  const canProceedFromStep2 = () => {
    if (!subject) {
      return false;
    }
    switch (selectedQuestionType) {
      case multipleChoice:
        return (
          multipleChoiceOptions.every((option) => option.text.trim().length > 0) &&
          !hasDuplicateOptions()
        );
      case essay:
        return true;
      default:
        return true;
    }
  };

  const handleNext = () => {
    if (currentStep === 1 && canProceedFromStep1) {
      setCurrentStep(2);
    } else if (currentStep === 2 && canProceedFromStep2()) {
      if (selectedQuestionType === multipleChoice && hasDuplicateOptions()) {
        showError(t('AddQuestion.AnswerNotDuplicate'));
        return;
      }
      addNewQuestion();
      setCurrentStep(3);
    } else if (currentStep === 1 && questionContent.length > MAX_QUESTION_LENGTH) {
      showError(`Nội dung câu hỏi không được vượt quá ${MAX_QUESTION_LENGTH} ký tự`);
    }
  };

  const handleBack = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const renderStep2Content = () => {
    switch (selectedQuestionType) {
      case multipleChoice:
        return (
          <>
            <MultipleChoiceEditor
              options={multipleChoiceOptions}
              onOptionsChange={setMultipleChoiceOptions}
            />
            {hasDuplicateOptions() && (
              <p className="mt-2 text-sm text-red-500">Các đáp án không được trùng lặp</p>
            )}
          </>
        );
      case essay:
        return <EssayEditor guidance={essayGuidance} onGuidanceChange={setEssayGuidance} />;
      default:
        return null;
    }
  };

  const addNewQuestion = async () => {
    try {
      let imgUrl = imageupload;
      if (imageupload instanceof File) {
        const formUpload = new FormData();
        formUpload.append('file', imageupload);
        const response = await amazonServices.uploadImg(formUpload);
        imgUrl = response.data.fileUrl;
      }
      const newQuestion = {
        questionBankId: selectedBank,
        subjectId: subject,
        content: questionContent,
        type: selectedQuestionType === essay ? 0 : selectedQuestionType === multipleChoice ? 1 : 2,
        difficultLevel: difficulty,
        point: points,
        options: multipleChoiceOptions.map((op) => op.text) ?? [],
        correctAnswer:
          selectedQuestionType === essay
            ? 'không có'
            : selectedQuestionType === multipleChoice
              ? (multipleChoiceOptions.find((op) => op.isCorrect === true)?.text ?? '')
              : '',
        explanation: explanation,
        objectFile: imgUrl ?? null,
        tags: tag,
        description: essayGuidance ?? '',
        questionId: question?.questionId ?? '',
        createUserId: question?.creatorId ?? '',
      };
      if (question && question.questionId !== '') {
        await questionService.editQuestion(newQuestion);
        showSuccess('Chỉnh sửa câu hỏi thành công');
      } else {
        const formData = new FormData();
        formData.append('questionBankId', newQuestion.questionBankId);
        formData.append('subjectId', newQuestion.subjectId);
        formData.append('content', newQuestion.content);
        formData.append('type', newQuestion.type.toString());
        formData.append('difficultLevel', newQuestion.difficultLevel.toString());
        formData.append('point', newQuestion.point.toString());
        newQuestion.options.forEach((option, index) => {
          formData.append(`options[${index}]`, option);
        });
        formData.append('correctAnswer', newQuestion.correctAnswer);
        formData.append('explanation', newQuestion.explanation);
        formData.append('objectFile', newQuestion.objectFile ?? '');
        formData.append('tags', newQuestion.tags);
        formData.append('description', newQuestion.description);
        await questionService.addQuestion(formData);
        showSuccess('Thêm mới câu hỏi thành công');
      }
      refetch();
    } catch (error) {
      showError(error);
    }
  };

  const handleCreateNew = () => {
    setCurrentStep(1);
    setSelectedBank('');
    setSelectedQuestionType('multiple-choice');
    setQuestionContent('');
    setImageUpload(null);
    setImagePreviewUrl(null);
    setMultipleChoiceOptions([
      { id: 'option-1', text: '', isCorrect: true },
      { id: 'option-2', text: '', isCorrect: false },
      { id: 'option-3', text: '', isCorrect: false },
      { id: 'option-4', text: '', isCorrect: false },
    ]);
    setEssayGuidance('');
    setDifficulty(1);
    setSubject('');
    setPoints(1);
    setExplanation('');
  };

  const handleImageUpload = (file: File) => {
    setImageUpload(file);
    const reader = new FileReader();
    reader.onload = () => {
      setImagePreviewUrl(reader.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleQuestionContentChange = (content: string) => {
    if (content.length <= MAX_QUESTION_LENGTH) {
      setQuestionContent(content);
    } else {
      showError(
        `${t('AddQuestion.MaxQSLength')} ${MAX_QUESTION_LENGTH} ${t('AddQuestion.Character')}`,
      );
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <ProgressStepper steps={steps} />

      <div className="flex gap-6 p-6">
        <div className="flex-1 space-y-6">
          {currentStep === 1 ? (
            <>
              <QuestionBankSelector
                selectedBank={selectedBank}
                onBankChange={setSelectedBank}
                isBankDisabled={!!question}
                setSelectedSubjectName={setSelectedSubjectName}
              />
              <QuestionTypeSelector
                selectedType={selectedQuestionType}
                onTypeChange={setSelectedQuestionType}
              />
              <QuestionContentEditor
                content={questionContent}
                onContentChange={handleQuestionContentChange}
                onNext={handleNext}
                canProceed={canProceedFromStep1}
                onImageUpload={handleImageUpload}
                existingImageUrl={imagePreviewUrl}
              />
            </>
          ) : currentStep === 2 ? (
            <>
              {renderStep2Content()}
              <AdditionalInfoSection
                difficulty={difficulty}
                onDifficultyChange={setDifficulty}
                selectedSubjectName={selectedSubjectName}
                subject={subject}
                onSubjectChange={setSubject}
                explanation={explanation}
                onExplanationChange={setExplanation}
                tag={tag}
                onTagChange={setTag}
                isSubjectDisabled={!!question}
              />
              <div className="flex justify-between pt-4">
                <Button variant="outline" onClick={handleBack}>
                  {t('AddQuestion.Back')}
                </Button>
                <div className="flex space-x-3">
                  <Button
                    onClick={handleNext}
                    disabled={!canProceedFromStep2()}
                    className="bg-black text-white"
                  >
                    {t('AddQuestion.Save')}
                  </Button>
                </div>
              </div>
            </>
          ) : (
            <SavedQuestionsList onCreateNew={handleCreateNew} />
          )}
        </div>

        {currentStep < 3 && (
          <div className="w-80 space-y-6">
            <QuickPreview
              selectedType={selectedQuestionType}
              content={questionContent}
              imageUrl={imagePreviewUrl}
            />
            <TipsPanel />
          </div>
        )}
      </div>
    </div>
  );
}
