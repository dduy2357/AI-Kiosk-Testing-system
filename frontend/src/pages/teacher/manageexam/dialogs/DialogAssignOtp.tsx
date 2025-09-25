import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import { IAssignOtpToExam } from '@/services/modules/manageexam/interfaces/manageExam.interface';
import { Form, Formik } from 'formik';
import React, { useEffect, useState } from 'react';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';
import moment from 'moment';
import { useTranslation } from 'react-i18next';

interface DialogAssignOtpExamTeacherProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: IAssignOtpToExam) => Promise<void>;
  viewingOtp?: IAssignOtpToExam | null;
}

const DialogAssignOtpExamTeacher = (props: DialogAssignOtpExamTeacherProps) => {
  //!State
  const { isOpen, toggle, onSubmit, viewingOtp } = props;
  const [remainingTime, setRemainingTime] = useState<string>('');
  const { t } = useTranslation('shared');

  const initialValues: IAssignOtpToExam = {
    timeValid: viewingOtp?.timeValid ?? 0,
    examId: viewingOtp?.examId ?? '',
  };

  const validationSchema = Yup.object({
    timeValid: Yup.number()
      .required(t('ExamManagement.TimeValidRequired'))
      .min(1, t('ExamManagement.TimeValidMin'))
      .max(86400, t('ExamManagement.TimeValidMax')),
  });

  //!Functions
  useEffect(() => {
    if (viewingOtp && viewingOtp.expiredAt) {
      const updateTimer = () => {
        const now = moment();
        const expiry = moment(viewingOtp.expiredAt);
        if (expiry.isAfter(now)) {
          const duration = moment.duration(expiry.diff(now));
          const hours = Math.floor(duration.asHours());
          const minutes = duration.minutes();
          const seconds = duration.seconds();
          setRemainingTime(
            `${hours.toString().padStart(2, '0')}:${minutes
              .toString()
              .padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`,
          );
        } else {
          setRemainingTime('Hết hiệu lực');
        }
      };

      updateTimer(); // Initial call
      const interval = setInterval(updateTimer, 1000); // Update every second

      return () => clearInterval(interval); // Cleanup on unmount
    } else {
      setRemainingTime('');
    }
  }, [viewingOtp]);

  //!Render
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/10 backdrop-blur-sm" />
      <DialogPortal>
        <DialogContent className="max-w-2xl">
          <Formik
            initialValues={initialValues}
            validationSchema={viewingOtp ? Yup.object({}) : validationSchema} // Disable validation in view mode
            onSubmit={async (values, { setSubmitting }) => {
              if (viewingOtp) return; // No submission in view mode
              try {
                setSubmitting(true);
                await onSubmit(values);
              } catch (error) {
                showError(t('ExamManagement.ErrorOccurred'));
              } finally {
                setSubmitting(false);
              }
            }}
            enableReinitialize
          >
            {({ isSubmitting, values }) => {
              return (
                <Fragment>
                  <Form className="space-y-4">
                    <div>
                      <DialogTitle className="text-xl font-medium">
                        {viewingOtp ? t('ExamManagement.ViewOtp') : t('ExamManagement.AssignOtp')}
                      </DialogTitle>
                      <DialogDescription className="mt-1 text-sm text-gray-500">
                        {viewingOtp
                          ? t('ExamManagement.ViewOtpDescription')
                          : t('ExamManagement.AssignOtpDescription')}
                      </DialogDescription>
                    </div>

                    {viewingOtp ? (
                      <div className="space-y-4">
                        <div className="grid grid-cols-1 gap-4">
                          <div className="space-y-2">
                            <label className="text-sm font-medium text-gray-900">Mã OTP</label>
                            <div className="rounded-md border border-green-200 bg-green-50 p-2 font-mono text-lg text-green-700">
                              {viewingOtp.otpCode}
                            </div>
                          </div>
                          <div className="space-y-2">
                            <label className="text-sm font-medium text-gray-900">
                              Time Remaining
                            </label>
                            <div className="rounded-md border border-blue-200 bg-blue-50 p-2 font-mono text-lg text-blue-700">
                              {remainingTime}
                            </div>
                          </div>
                        </div>
                      </div>
                    ) : (
                      <div className="grid grid-cols-1 gap-4">
                        <div className="space-y-2">
                          <FormikField
                            component={InputField}
                            name="timeValid"
                            placeholder="Enter validity period (minutes)"
                            value={values.timeValid}
                            label="Validity Period"
                            required
                            isNumberic
                          />
                        </div>
                      </div>
                    )}

                    <div className="flex justify-end gap-2 pt-4">
                      <DialogClose asChild>
                        <Button variant="outline" type="button">
                          {viewingOtp ? 'Close' : 'Cancel'}
                        </Button>
                      </DialogClose>
                      {!viewingOtp && (
                        <Button type="submit" isLoading={isSubmitting}>
                          Save
                        </Button>
                      )}
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

export default React.memo(DialogAssignOtpExamTeacher);
