import type React from 'react';
import { useTranslation } from 'react-i18next';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Progress } from '@/components/ui/progress';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { cn } from '@/lib/utils';
import useGetListBankQuestion from '@/services/modules/bankquestion/hooks/useGetAllBankQuestion';
import { IBankQuestionRequest } from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import { IQuestionForm } from '@/services/modules/question/interfaces/question.interface';
import { SubjectList } from '@/services/modules/subject/interfaces/subject.interface';
import { AlertCircle, CheckCircle, FileText, Upload } from 'lucide-react';
import { useRef, useState } from 'react';
import questionService from '@/services/modules/question/question.service';
import httpService from '@/services/httpService';

interface ImportWordModalProps {
  isOpen: boolean;
  onClose: () => void;
  onImportComplete: (questions: IQuestionForm[]) => void;
  dataSubject: SubjectList[];
  refetch: () => void;
}

interface QuestionWithValidation extends IQuestionForm {
  isValid: boolean;
  validationErrors: string[];
}

export default function ImportWordModal({
  isOpen,
  onClose,
  onImportComplete,
  dataSubject,
  refetch,
}: ImportWordModalProps) {
  const { t } = useTranslation('shared'); // Initialize useTranslation with 'shared' namespace
  const [file, setFile] = useState<File | null>(null);
  const [bankQuestion, setBankQuestion] = useState('');
  const [subject, setSubject] = useState('');
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState('');
  const [previewQuestions, setPreviewQuestions] = useState<QuestionWithValidation[]>([]);
  const userId = httpService.getUserStorage()?.roleId[0];

  // Fetch bank questions
  const [filtersBankquestion] = useState<IBankQuestionRequest>({
    pageSize: 10,
    currentPage: 1,
    status: 1,
    IsMyQuestion: userId === 2 ? true : undefined,
  });
  const { data } = useGetListBankQuestion(filtersBankquestion);

  const [step, setStep] = useState<'upload' | 'preview' | 'complete'>('upload');
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (event: React.ChangeEvent<HTMLInputElement>) => {
    const selectedFile = event.target.files?.[0];
    if (selectedFile) {
      if (
        selectedFile.type ===
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document' ||
        selectedFile.name.endsWith('.docx')
      ) {
        setFile(selectedFile);
        setError('');
      } else {
        setError(t('ImportModal.errorFileType'));
        setFile(null);
      }
    }
  };

  const handleDragOver = (e: React.DragEvent) => e.preventDefault();
  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    const droppedFile = e.dataTransfer.files[0];
    if (droppedFile) {
      if (
        droppedFile.type ===
        'application/vnd.openxmlformats-officedocument.wordprocessingml.document' ||
        droppedFile.name.endsWith('.docx')
      ) {
        setFile(droppedFile);
        setError('');
      } else {
        setError(t('ImportModal.errorFileType'));
      }
    }
  };

  const parseWordFile = async (file: File): Promise<QuestionWithValidation[]> => {
    try {
      const format = await questionService.formatFileImport(file, bankQuestion);
      const questions: QuestionWithValidation[] = format.data.data;

      return questions.map((q) => ({
        ...q,
        questionBankId: bankQuestion,
        subjectId: subject,
        type: 1,
        objectFile: '',
      }));
    } catch (error) {
      console.error('Lỗi khi phân tích file Word:', error);
      return [];
    }
  };

  const handleUpload = async () => {
    if (!file || !subject || !bankQuestion) {
      setError(t('ImportModal.errorMissingFields'));
      return;
    }

    setIsUploading(true);
    setUploadProgress(0);
    setError('');

    try {
      const progressInterval = setInterval(() => {
        setUploadProgress((prev) =>
          prev >= 90 ? (clearInterval(progressInterval), 90) : prev + 10,
        );
      }, 200);

      const questions = await parseWordFile(file);
      clearInterval(progressInterval);
      setUploadProgress(100);

      setTimeout(() => {
        setPreviewQuestions(questions);
        setStep('preview');
        setIsUploading(false);
      }, 500);
    } catch (err) {
      setError(t('ImportModal.errorProcessingFile'));
      setIsUploading(false);
      setUploadProgress(0);
    }
  };

  const handleConfirmImport = () => {
    const validQuestions = previewQuestions.map(({ ...question }) => question as IQuestionForm);
    onImportComplete(validQuestions);
    setStep('complete');
    refetch();
    setTimeout(handleClose, 2000);
  };

  const handleClose = () => {
    setFile(null);
    setSubject('');
    setBankQuestion('');
    setError('');
    setPreviewQuestions([]);
    setStep('upload');
    setUploadProgress(0);
    setIsUploading(false);
    onClose();
  };

  // Automatically set subject based on selected bankQuestion
  const handleBankQuestionChange = (value: string) => {
    setBankQuestion(value);
    const selectedBank = data?.find((e) => e.questionBankId === value);
    const matchedSubject = dataSubject?.find((s) => s.subjectName === selectedBank?.subjectName);
    setSubject(matchedSubject?.subjectId || '');
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-h-[80vh] overflow-y-auto sm:max-w-2xl">
        <DialogHeader className="pb-4">
          <DialogTitle className="text-lg font-semibold">
            {step === 'upload' && t('ImportModal.uploadTitle')}
            {step === 'preview' && t('ImportModal.previewTitle')}
            {step === 'complete' && t('ImportModal.completeTitle')}
          </DialogTitle>
        </DialogHeader>

        {step === 'upload' && (
          <div className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="bankQuestion">{t('ImportModal.bankQuestionLabel')}</Label>
              <Select value={bankQuestion} onValueChange={handleBankQuestionChange} required>
                <SelectTrigger>
                  <SelectValue placeholder={t('ImportModal.bankQuestionPlaceholder')} />
                </SelectTrigger>
                <SelectContent>
                  {data?.map((e) => (
                    <SelectItem key={e?.questionBankId} value={e?.questionBankId}>
                      {e?.title}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="subject">{t('ImportModal.subjectLabel')}</Label>
              <div className="rounded-md border bg-gray-50 p-2 text-sm">
                {data?.find((e) => e.questionBankId === bankQuestion)?.subjectName ||
                  t('ImportModal.subjectPlaceholder')}
              </div>
            </div>

            <div className="space-y-2">
              <Label>{t('ImportModal.fileLabel')}</Label>
              <div
                className={cn(
                  'cursor-pointer rounded-lg border-2 border-dashed p-6 text-center transition-colors',
                  file ? 'border-green-300 bg-green-50' : 'border-gray-300 hover:border-gray-400',
                )}
                onDragOver={handleDragOver}
                onDrop={handleDrop}
                onClick={() => fileInputRef.current?.click()}
              >
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".docx"
                  onChange={handleFileSelect}
                  className="hidden"
                />
                {file ? (
                  <div className="flex items-center justify-center gap-2">
                    <FileText className="h-8 w-8 text-green-600" />
                    <div>
                      <p className="font-medium text-green-700">{file.name}</p>
                      <p className="text-sm text-green-600">
                        {(file.size / 1024 / 1024).toFixed(2)} MB
                      </p>
                    </div>
                  </div>
                ) : (
                  <div>
                    <Upload className="mx-auto mb-2 h-12 w-12 text-gray-400" />
                    <p className="text-gray-600">{t('ImportModal.dragDropText')}</p>
                    <p className="mt-1 text-sm text-gray-500">{t('ImportModal.fileTypeText')}</p>
                  </div>
                )}
              </div>
            </div>

            {error && (
              <Alert variant="destructive">
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>{error}</AlertDescription>
              </Alert>
            )}

            {isUploading && (
              <div className="space-y-2">
                <div className="flex justify-between text-sm">
                  <span>{t('ImportModal.uploadingText')}</span>
                  <span>{uploadProgress}%</span>
                </div>
                <Progress value={uploadProgress} className="w-full" />
              </div>
            )}

            <div className="flex gap-2 pt-4">
              <Button
                onClick={handleUpload}
                className="flex-1"
                disabled={!file || !subject || !bankQuestion || isUploading}
              >
                {isUploading ? t('ImportModal.uploadingText') : t('ImportModal.uploadButton')}
              </Button>
              <Button variant="outline" onClick={handleClose}>
                {t('ImportModal.cancelButton')}
              </Button>
            </div>
          </div>
        )}

        {step === 'preview' && (
          <div className="space-y-4">
            <div className="rounded-lg bg-blue-50 p-4">
              <p className="text-sm text-blue-700">
                {t('ImportModal.previewSummaryPrefix')}
                <strong>{previewQuestions.length}</strong>
                {t('ImportModal.previewSummaryValid')}
              </p>
            </div>

            <div className="max-h-96 space-y-3 overflow-y-auto">
              {previewQuestions.map((question, index) => (
                <div
                  key={`${question.questionBankId}-${index}`}
                  className={cn('rounded-lg border p-4', 'bg-white')}
                >
                  <div className="mb-2 flex items-start justify-between">
                    <span className="text-sm font-medium text-gray-500">
                      {t('ImportModal.questionLabel')} {index + 1}
                    </span>
                    <div className="flex gap-2">
                      <span className="rounded bg-gray-100 px-2 py-1 text-xs">
                        {t('ImportModal.questionTypeMultipleChoice')}
                      </span>
                      <span className="rounded bg-gray-100 px-2 py-1 text-xs">
                        {question.difficultLevel === 1
                          ? t('ImportModal.difficultyEasy')
                          : question.difficultLevel === 2
                            ? t('ImportModal.difficultyMedium')
                            : question.difficultLevel === 3
                              ? t('ImportModal.difficultyHard')
                              : t('ImportModal.difficultyVeryHard')}
                      </span>
                    </div>
                  </div>

                  <p className="mb-2 font-medium">
                    {question.content || t('ImportModal.noContent')}
                  </p>

                  {question.options && (
                    <div className="space-y-1">
                      {question.options.map((option, optIndex) => (
                        <div
                          key={optIndex}
                          className={cn(
                            'rounded p-2 text-sm',
                            option === question.correctAnswer
                              ? 'bg-green-100 font-medium text-green-700'
                              : 'bg-gray-50',
                          )}
                        >
                          {String.fromCharCode(65 + optIndex)}. {option}
                        </div>
                      ))}
                    </div>
                  )}

                  {question.explanation && (
                    <p className="mt-2 text-sm italic text-gray-600">
                      {t('ImportModal.explanationLabel')} {question.explanation}
                    </p>
                  )}
                  {question.tags && (
                    <p className="mt-2 text-sm text-gray-600">
                      {t('ImportModal.tagsLabel')} {question.tags}
                    </p>
                  )}
                  {question.description && (
                    <p className="mt-2 text-sm text-gray-600">
                      {t('ImportModal.descriptionLabel')} {question.description}
                    </p>
                  )}
                  <p className="mt-2 text-sm text-gray-600">
                    {t('ImportModal.pointsLabel')}{' '}
                    {question.point || t('ImportModal.pointsUndefined')}
                  </p>
                </div>
              ))}
            </div>

            <div className="flex gap-2 pt-4">
              <Button
                onClick={handleConfirmImport}
                className="flex-1"
                disabled={previewQuestions.length === 0}
              >
                {t('ImportModal.confirmImportButtonPrefix')} {previewQuestions.length}{' '}
                {t('ImportModal.confirmImportButtonSuffix')}
              </Button>
              <Button variant="outline" onClick={() => setStep('upload')}>
                {t('ImportModal.backButton')}
              </Button>
            </div>
          </div>
        )}

        {step === 'complete' && (
          <div className="py-8 text-center">
            <CheckCircle className="mx-auto mb-4 h-16 w-16 text-green-600" />
            <h3 className="mb-2 text-lg font-semibold text-green-700">
              {t('ImportModal.completeTitle')}
            </h3>
            <p className="text-gray-600">
              {t('ImportModal.completeMessagePrefix')}{' '}
              {previewQuestions.filter((q) => q.isValid).length}{' '}
              {t('ImportModal.completeMessageSuffix')}
            </p>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
}
