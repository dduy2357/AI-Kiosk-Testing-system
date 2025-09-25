import type React from 'react';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { SubjectList } from '@/services/modules/subject/interfaces/subject.interface';
import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { IQuestionBankForm } from '@/services/modules/bankquestion/interfaces/bankquestion.interface';
import useGetQuestionBankDetail from '@/services/modules/bankquestion/hooks/useGetQuestionBankDetail';

interface AddQuestionBankModalProps {
  id?: string | null;
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (data: IQuestionBankForm, questionBankId?: string) => void;
  dataSubjects: SubjectList[];
}

export default function AddQuestionBankModal({
  id,
  isOpen,
  onClose,
  onSubmit,
  dataSubjects,
}: Readonly<AddQuestionBankModalProps>) {
  const { t } = useTranslation('shared');
  const { data: dataQuestionBankDetail } = useGetQuestionBankDetail(id ?? null, {
    isTrigger: !!id,
  });

  const [formData, setFormData] = useState<IQuestionBankForm>({
    title: '',
    subjectId: '',
    description: '',
  });
  const [error, setError] = useState<string>('');

  useEffect(() => {
    if (dataQuestionBankDetail) {
      setFormData({
        title: dataQuestionBankDetail.questionBankName ?? '',
        subjectId: dataQuestionBankDetail.subjectId ?? '',
        description: dataQuestionBankDetail.description ?? '',
      });
    } else {
      setFormData({
        title: '',
        subjectId: '',
        description: '',
      });
    }
  }, [dataQuestionBankDetail]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Check if subjectId is empty
    if (!formData.subjectId) {
      setError(t('BankQuestion.NoSubjectsSelected'));
      return;
    }

    // Clear error if validation passes
    setError('');
    id ? onSubmit(formData, id) : onSubmit(formData);
    setFormData({
      title: '',
      subjectId: '',
      description: '',
    });
    onClose();
  };

  const handleCancel = () => {
    setFormData({
      title: '',
      subjectId: '',
      description: '',
    });
    setError('');
    onClose();
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader className="pb-4">
          <DialogTitle className="text-lg font-semibold">
            {id ? t('BankQuestion.EditBankQuestion') : t('BankQuestion.AddBankQuestion')}
          </DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">{t('BankQuestion.NameBankQuestion')}</Label>
            <Input
              id="name"
              placeholder={t('BankQuestion.PlaceholderAdd')}
              value={formData.title}
              onChange={(e) => setFormData({ ...formData, title: e.target.value })}
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="subject">{t('BankQuestion.NameSubject')}</Label>
            <Select
              value={formData.subjectId}
              onValueChange={(value) => {
                setFormData({ ...formData, subjectId: value });
                setError(''); // Clear error when a subject is selected
              }}
              required
            >
              <SelectTrigger>
                <SelectValue placeholder={t('BankQuestion.SelectSubject')} />
              </SelectTrigger>
              <SelectContent>
                {dataSubjects?.map((subject) => (
                  <SelectItem key={subject?.subjectId} value={subject?.subjectId}>
                    {subject?.subjectName}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {error && <p className="text-sm text-red-500">{error}</p>} {/* Display error message */}
          </div>

          <div className="space-y-2">
            <label htmlFor="note" className="block font-medium">
              {t('BankQuestion.Description')}
            </label>
            <textarea
              id="note"
              name="note"
              placeholder={t('BankQuestion.PlaceHolderDescription')}
              className="w-full rounded border p-2"
              value={formData.description}
              onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                setFormData({ ...formData, description: e.target.value })
              }
            />
          </div>

          <div className="flex gap-2 pt-4">
            <Button type="submit" className="flex-1">
              {id ? t('BankQuestion.Edit') : t('BankQuestion.Create')}
            </Button>
            <Button type="button" variant="outline" onClick={handleCancel}>
              {t('BankQuestion.Cancel')}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
