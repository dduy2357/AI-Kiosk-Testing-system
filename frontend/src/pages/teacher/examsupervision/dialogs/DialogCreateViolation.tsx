import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Dialog,
  DialogContent,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { showError, showSuccess } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import { Student } from '@/services/modules/monitor/interfaces/monitorDetail.interface';
import { IViolationForm } from '@/services/modules/violation/interfaces/violation.interface';
import violationService from '@/services/modules/violation/violation.service';
import { Label } from '@radix-ui/react-label';
import { Form, Formik } from 'formik';
import { Mail } from 'lucide-react';
import { Fragment, useState } from 'react';
import { useTranslation } from 'react-i18next';
import ImageUpload from '../components/image-upload';

interface DialogProps extends DialogI<any> {
  row?: Student | null;
  studentExamId?: string;
  refetch?: () => void;
}
const DialogCreateViolation = (props: DialogProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle } = props;
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);

  const initialValues: IViolationForm = {
    studentExamId: props.row?.studentExamId ?? props.studentExamId ?? '',
    violateName: '',
    message: '',
    screenshotPath: undefined,
    isSendMail: false,
  };

  const handleSubmit = async (values: IViolationForm) => {
    try {
      const formData = new FormData();

      if (uploadedFile) {
        formData.append('screenshotPath', uploadedFile);
      }
      formData.append('studentExamId', values.studentExamId);
      formData.append('violateName', values.violateName);
      formData.append('message', values.message ?? '');
      formData.append('isSendMail', values.isSendMail.toString());
      await violationService.createViolation(formData);
      if (props.refetch) {
        props.refetch();
      }
      showSuccess(t('ExamSupervision.CreateViolationSuccess'));
      toggle();
    } catch (error) {
      showError(error);
    }
  };

  const handleFileUpload = (file: File | null) => {
    setUploadedFile(file);
  };

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay />
        <DialogContent className="max-h-[80vh] overflow-y-auto">
          <Formik initialValues={initialValues} onSubmit={handleSubmit}>
            {({ isSubmitting, setFieldValue, values }) => {
              return (
                <Fragment>
                  <DialogTitle>{t('ExamSupervision.CreateViolationTitle')}</DialogTitle>
                  <Form className="mt-[25px] justify-end gap-2">
                    <div className="space-y-2">
                      <FormikField
                        id="violateName"
                        component={InputField}
                        name="violateName"
                        label={t('ExamSupervision.CreateViolationTitle')}
                        placeholder={t('ExamSupervision.CreateViolationPlaceholder')}
                        required
                        className="transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                      />

                      <FormikField
                        component={Textarea}
                        name="message"
                        placeholder={t('ExamSupervision.CreateViolationPlaceholder')}
                        label={t('ExamSupervision.CreateViolationDescription')}
                      />
                    </div>

                    <ImageUpload
                      uploadedFile={uploadedFile}
                      onFileUpload={(file: File | null) => {
                        handleFileUpload(file);
                        setFieldValue('screenshotPath', file); // Update Formik state for consistency
                      }}
                    />
                    <br />
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="sendEmail"
                        checked={values.isSendMail}
                        onCheckedChange={(checked) => setFieldValue('isSendMail', checked)}
                      />
                      <Label
                        htmlFor="sendEmail"
                        className="flex cursor-pointer items-center gap-2 text-sm font-medium"
                      >
                        <Mail className="h-4 w-4 text-gray-500" />
                        {t('ExamSupervision.CreateViolationSendEmail')}
                      </Label>
                    </div>
                    <div className="flex justify-end gap-2">
                      <Button type="submit" disabled={isSubmitting}>
                        {isSubmitting
                          ? t('ExamSupervision.CreateViolationSubmitting')
                          : t('ExamSupervision.CreateViolationSubmit')}
                      </Button>
                      <Button variant="ghost" type="button" onClick={toggle}>
                        {t('Close')}
                      </Button>
                    </div>
                  </Form>
                </Fragment>
              );
            }}
          </Formik>
        </DialogContent>
      </DialogPortal>
    </Dialog>
  );
};

export default DialogCreateViolation;
