import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import { StudentExamList } from '@/services/modules/studentexam/interfaces/studentexam.interface';
import { Form, Formik } from 'formik';
import { Lock } from 'lucide-react';
import { Fragment } from 'react/jsx-runtime';
import { useTranslation } from 'react-i18next';
import * as Yup from 'yup';

export interface ConfirmOTPFormValues {
  otpCode: number | string;
  examId?: string;
}

interface DialogConfirmOTPProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: ConfirmOTPFormValues) => Promise<void>;
  selectedExam?: StudentExamList;
}

const DialogConfirmOTP = (props: DialogConfirmOTPProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, selectedExam } = props;

  const initialValues: ConfirmOTPFormValues = {
    otpCode: '',
    examId: selectedExam?.examId ?? '',
  };

  const validationSchema = Yup.object({
    otpCode: Yup.string()
      .required(t('OTP.Invalid') ?? 'Please enter OTP')
      .matches(/^\d{6}$/, t('OTP.Placeholder') ?? 'OTP must be 6 digits'),
    examId: Yup.string().required(t('OTP.ExamId') ?? 'Exam ID is required'),
  });

  const handleSubmit = async (values: ConfirmOTPFormValues, { setSubmitting }: any) => {
    try {
      setSubmitting(true);
      await onSubmit(values);
    } catch (error) {
      toggle();
      showError(t('OTP.Invalid') ?? 'Invalid OTP');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/40 backdrop-blur-sm transition-opacity duration-300" />
      <DialogPortal>
        <DialogContent className="max-w-md rounded-lg bg-white p-6 shadow-xl sm:max-w-lg lg:max-w-4xl">
          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={handleSubmit}
            enableReinitialize
          >
            {({ isSubmitting }) => (
              <Fragment>
                <Form className="space-y-6">
                  <div className="text-center">
                    <DialogTitle className="flex items-center justify-center gap-2 text-2xl font-semibold text-gray-900">
                      <Lock className="h-6 w-6 text-blue-600" />
                      {t('OTP.Title') ?? 'Enter OTP'}
                    </DialogTitle>
                    <p className="mt-2 text-sm text-gray-500">
                      {t('OTP.Description') ?? 'Please enter the OTP to access the exam'}
                    </p>
                  </div>

                  <hr className="border-t border-gray-200" />

                  <div className="space-y-4">
                    <div className="relative">
                      <FormikField
                        component={InputField}
                        name="examId"
                        label={t('OTP.ExamId') ?? 'Exam ID'}
                        disabled
                        className="pr-10"
                        iconText="#"
                        isIcon
                      />
                    </div>

                    <div className="relative">
                      <FormikField
                        component={InputField}
                        name="otpCode"
                        label={t('OTP.Title') ?? 'OTP Code'}
                        placeholder={t('OTP.Placeholder') ?? 'Enter 6-digit OTP'}
                        required
                        isNumberic
                        className="pr-10"
                      />
                    </div>
                  </div>

                  <div className="flex justify-end gap-3 pt-4">
                    <DialogClose asChild>
                      <Button
                        variant="outline"
                        type="button"
                        className="rounded-md border-gray-300 bg-gray-50 text-gray-700 transition-colors duration-200 hover:bg-gray-100"
                      >
                        {t('Close') ?? 'Close'}
                      </Button>
                    </DialogClose>
                    <Button
                      type="submit"
                      isLoading={isSubmitting}
                      disabled={isSubmitting}
                      className="rounded-md bg-blue-600 text-white transition-colors duration-200 hover:bg-blue-700 disabled:bg-gray-400"
                    >
                      {t('Confirm') ?? 'Confirm'}
                    </Button>
                  </div>
                </Form>
              </Fragment>
            )}
          </Formik>
        </DialogContent>
      </DialogPortal>
    </Dialog>
  );
};

export default DialogConfirmOTP;
