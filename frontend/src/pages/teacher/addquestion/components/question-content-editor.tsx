import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { Form, Formik } from 'formik';
import * as yup from 'yup';
import { Upload } from 'lucide-react';
import { useState } from 'react';
import { useTranslation } from 'react-i18next';

interface QuestionContentEditorProps {
  content: string;
  onContentChange: (content: string) => void;
  onNext: () => void;
  canProceed: boolean | string;
  onImageUpload?: (file: File) => void;
  existingImageUrl?: string | null; // Optional prop for existing image URL
}

const validationSchema = yup.object().shape({
  content: yup.string().required('Nội dung câu hỏi là bắt buộc'),
});

export default function QuestionContentEditor({
  content,
  onContentChange,
  onNext,
  canProceed,
  onImageUpload,
  existingImageUrl,
}: Readonly<QuestionContentEditorProps>) {
  const [imagePreview, setImagePreview] = useState<string | null>(existingImageUrl ?? null);
  const [error, setError] = useState<string | null>(null);
  const { t } = useTranslation('shared');

  const handleImageUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      // Validate file type
      const validTypes = ['image/png', 'image/jpeg', 'image/gif'];
      if (!validTypes.includes(file.type)) {
        setError(t('AddQuestion.RequireImgUpload'));
        return;
      }

      // Validate file size (10MB)
      const maxSize = 10 * 1024 * 1024; // 10MB in bytes
      if (file.size > maxSize) {
        setError(t('AddQuestion.MaxSizeOfImg'));
        return;
      }

      // Create preview
      const reader = new FileReader();
      reader.onload = () => {
        setImagePreview(reader.result as string);
        setError(null);
        if (onImageUpload) {
          onImageUpload(file);
        }
      };
      reader.readAsDataURL(file);
    }
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-medium">{t('AddQuestion.Content')}</CardTitle>
        <p className="text-sm text-gray-500">{t('AddQuestion.EnterContent')}</p>
      </CardHeader>
      <Formik
        initialValues={{ content }}
        enableReinitialize
        onSubmit={() => onNext()}
        validationSchema={validationSchema}
      >
        {() => {
          return (
            <Form>
              <CardContent className="space-y-4">
                <div>
                  <textarea
                    id="content"
                    name="content"
                    placeholder={t('AddQuestion.PlaceholderContent')}
                    className="h-40 w-full rounded-md border border-input bg-background px-3 py-2 text-sm text-foreground shadow-sm placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                    value={content}
                    onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
                      onContentChange(e.target.value)
                    }
                  />
                </div>

                <div>
                  <label className="mb-2 block text-sm font-medium text-gray-700">
                    {t('AddQuestion.Illustration')}
                  </label>
                  <div className="rounded-lg border-2 border-dashed border-gray-300 p-8 text-center transition-colors hover:border-gray-400">
                    {imagePreview ? (
                      <img
                        src={imagePreview}
                        alt="Preview"
                        className="mx-auto mb-2 max-h-40 object-contain"
                      />
                    ) : (
                      <Upload className="mx-auto mb-2 h-8 w-8 text-gray-400" />
                    )}
                    <p className="mb-2 text-sm text-gray-500">{t('AddQuestion.DragAndDrop')}</p>
                    <p className="text-xs text-gray-400">{t('AddQuestion.MaxSizeOfImg')}</p>
                    {error && <p className="mt-2 text-xs text-red-500">{error}</p>}
                    <input
                      type="file"
                      accept="image/png,image/jpeg,image/gif"
                      className="hidden"
                      id="image-upload"
                      onChange={handleImageUpload}
                    />
                    <label htmlFor="image-upload">
                      <Button variant="outline" size="sm" className="mt-3" asChild>
                        <span>{t('AddQuestion.SelectFile')}</span>
                      </Button>
                    </label>
                  </div>
                </div>

                <Separator />

                <div className="flex justify-between pt-4">
                  <Button variant="outline" disabled>
                    {t('AddQuestion.Back')}
                  </Button>
                  <Button type="submit" disabled={!canProceed}>
                    {t('AddQuestion.NextPreview')}
                  </Button>
                </div>
              </CardContent>
            </Form>
          );
        }}
      </Formik>
    </Card>
  );
}
