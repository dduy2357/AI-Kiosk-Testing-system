import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Dialog, DialogContent, DialogOverlay, DialogPortal } from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { DialogI } from '@/interfaces/common';
import { Form, Formik } from 'formik';
import { Send, ShieldAlert } from 'lucide-react';
import { Fragment } from 'react';
import { useTranslation } from 'react-i18next';
import * as Yup from 'yup';

interface DialogProps extends DialogI<any> {
  createUserId: string;
  onSubmit?: (values: SendAlertValues) => void;
}

export interface SendAlertValues {
  message: string;
  sendToId: string;
  type: string;
}

const DialogSendAlert = (props: DialogProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, createUserId } = props;
  const validationSchema = Yup.object({
    message: Yup.string().required(t('ExamSupervision.AlertMessageRequired')),
    type: Yup.string().required(t('ExamSupervision.AlertTypeRequired')),
  });
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay className="bg-black/30 backdrop-blur-sm" />
        <DialogContent className="overflow-hidden p-0 sm:max-w-[500px] lg:max-w-3xl">
          <Alert className="flex items-start gap-4 rounded-none border-none p-6">
            <ShieldAlert className="mt-1 h-6 w-6 text-orange-500" />
            <div>
              <AlertTitle className="text-xl font-bold text-gray-900">
                {t('ExamSupervision.SendAlertTitle')}
              </AlertTitle>
              <AlertDescription className="text-gray-600">
                {t('ExamSupervision.SendAlertDescription')}
              </AlertDescription>
            </div>
          </Alert>

          <div className="border-b border-gray-200" />

          <Formik
            validationSchema={validationSchema}
            initialValues={{
              message: '',
              type: '',
              sendToId: createUserId || '',
            }}
            onSubmit={onSubmit || (() => {})}
          >
            {({ isSubmitting }) => {
              return (
                <Fragment>
                  <Form>
                    <Card className="rounded-none border-none shadow-none">
                      <CardHeader className="pb-4">
                        <h3 className="flex items-center gap-2 text-lg font-semibold">
                          <Send className="h-5 w-5" />
                          {t('ExamSupervision.SendAlertFormTitle')}
                        </h3>
                      </CardHeader>
                      <CardContent className="grid gap-4">
                        <div className="grid gap-2">
                          <FormikField
                            component={InputField}
                            name="type"
                            label={t('ExamSupervision.AlertType')}
                            placeholder={t('ExamSupervision.AlertTypePlaceholder')}
                            required
                          />
                        </div>
                        <div className="grid gap-2">
                          <FormikField
                            component={Textarea}
                            name="message"
                            label={t('ExamSupervision.AlertMessage')}
                            placeholder={t('ExamSupervision.AlertMessagePlaceholder')}
                            required
                          />
                        </div>
                      </CardContent>
                    </Card>
                    <div className="flex justify-end p-6 pt-0">
                      <Button
                        type="submit"
                        isLoading={isSubmitting}
                        className="w-full bg-black text-white hover:bg-gray-800"
                      >
                        <Send className="mr-2 h-4 w-4" />
                        {t('ExamSupervision.SendAlertButton')}
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

export default DialogSendAlert;
