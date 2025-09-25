import { useTranslation } from 'react-i18next';
import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Textarea } from '@/components/ui/textarea';
import { showError, showSuccess } from '@/helpers/toast';
import feedbackService from '@/services/modules/feedback/feedback.service';
import type { IFeedbackForm } from '@/services/modules/feedback/interfaces/feedback.interface';
import { Form, Formik } from 'formik';
import { Heart, Lightbulb, MessageSquare, Send, Star } from 'lucide-react';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

export default function StudentFeedback() {
  const { t } = useTranslation('shared');

  const validationSchema = Yup.object({
    title: Yup.string()
      .required(t('SendFeedback.validation.titleRequired'))
      .max(1000, t('SendFeedback.validation.titleMaxLength')),
    content: Yup.string()
      .required(t('SendFeedback.validation.contentRequired'))
      .min(10, t('SendFeedback.validation.contentMinLength')),
  });

  const handleSubmit = async (values: IFeedbackForm) => {
    try {
      await feedbackService.addFeedback(values);
      showSuccess(t('SendFeedback.successMessage'));
    } catch (error) {
      showError(error);
    }
  };

  const initialValues: IFeedbackForm = {
    title: '',
    content: '',
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-100 px-4 py-8">
      <div className="mx-auto max-w-4xl">
        {/* Header Section */}
        <div className="mb-12 text-center">
          <div className="mb-6 flex justify-center">
            <div className="relative">
              <div className="absolute -inset-4 rounded-full bg-gradient-to-r from-blue-400 to-purple-400 opacity-20 blur-lg"></div>
              <div className="relative rounded-2xl bg-gradient-to-r from-blue-500 to-purple-600 p-4 shadow-xl">
                <MessageSquare className="h-8 w-8 text-white" />
              </div>
            </div>
          </div>
          <h1 className="mb-4 bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-4xl font-bold text-transparent">
            {t('SendFeedback.header.title')}
          </h1>
          <p className="mx-auto max-w-2xl text-lg text-slate-600">
            {t('SendFeedback.header.description')}
          </p>
        </div>

        {/* Main Form Card */}
        <Card className="mb-8 border-0 bg-white/80 shadow-2xl shadow-blue-500/10 backdrop-blur-sm">
          <CardHeader className="pb-6">
            <div className="mb-2 flex items-center gap-3">
              <div className="rounded-lg bg-gradient-to-r from-blue-100 to-purple-100 p-2">
                <Send className="h-5 w-5 text-blue-600" />
              </div>
              <CardTitle className="bg-gradient-to-r from-slate-700 to-slate-900 bg-clip-text text-xl font-semibold text-transparent">
                {t('SendFeedback.form.title')}
              </CardTitle>
            </div>
            <p className="ml-12 text-slate-600">
              {t('SendFeedback.form.description')}
            </p>
          </CardHeader>
          <CardContent className="px-8 pb-8">
            <Formik
              initialValues={initialValues}
              validationSchema={validationSchema}
              onSubmit={async (values, { setSubmitting }) => {
                try {
                  await handleSubmit(values);
                } catch (error) {
                  showError(error);
                }
                setSubmitting(false);
              }}
            >
              {({ isSubmitting, values }) => {
                return (
                  <Fragment>
                    <Form className="space-y-6">
                      <div className="space-y-6">
                        <div className="group">
                          <FormikField
                            id="title"
                            component={InputField}
                            name="title"
                            placeholder={t('SendFeedback.form.titlePlaceholder')}
                            value={values.title}
                            label={t('SendFeedback.form.titleLabel')}
                            required
                            className="transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                          />
                        </div>
                        <div className="group">
                          <FormikField
                            component={Textarea}
                            name="content"
                            placeholder={t('SendFeedback.form.contentPlaceholder')}
                            label={t('SendFeedback.form.contentLabel')}
                            required
                            className="min-h-[120px] resize-none transition-all duration-200 focus:border-blue-500 focus:ring-2 focus:ring-blue-500/20"
                          />
                        </div>
                      </div>
                      <div className="flex justify-end pt-6">
                        <Button
                          type="submit"
                          isLoading={isSubmitting}
                          className="transform rounded-xl bg-gradient-to-r from-blue-600 to-purple-600 px-8 py-3 text-white shadow-lg transition-all duration-200 hover:scale-105 hover:from-blue-700 hover:to-purple-700 hover:shadow-xl"
                        >
                          <Send className="mr-2 h-4 w-4" />
                          {t('SendFeedback.form.submitButton')}
                        </Button>
                      </div>
                    </Form>
                  </Fragment>
                );
              }}
            </Formik>
          </CardContent>
        </Card>

        {/* Tips Card */}
        <Card className="border-0 bg-gradient-to-r from-blue-50 to-purple-50 shadow-xl shadow-blue-500/5">
          <CardContent className="p-8">
            <div className="flex items-start gap-4">
              <div className="rounded-full bg-gradient-to-r from-blue-500 to-purple-500 p-3 shadow-lg">
                <Lightbulb className="h-5 w-5 text-white" />
              </div>
              <div className="flex-1">
                <h3 className="mb-4 bg-gradient-to-r from-blue-700 to-purple-700 bg-clip-text text-lg font-semibold text-transparent">
                  {t('SendFeedback.tips.title')}
                </h3>
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="flex items-start gap-3 rounded-lg bg-white/50 p-3 backdrop-blur-sm">
                    <Star className="mt-0.5 h-4 w-4 flex-shrink-0 text-blue-600" />
                    <span className="text-sm text-slate-700">
                      {t('SendFeedback.tips.tip1')}
                    </span>
                  </div>
                  <div className="flex items-start gap-3 rounded-lg bg-white/50 p-3 backdrop-blur-sm">
                    <Star className="mt-0.5 h-4 w-4 flex-shrink-0 text-purple-600" />
                    <span className="text-sm text-slate-700">
                      {t('SendFeedback.tips.tip2')}
                    </span>
                  </div>
                  <div className="flex items-start gap-3 rounded-lg bg-white/50 p-3 backdrop-blur-sm">
                    <Star className="mt-0.5 h-4 w-4 flex-shrink-0 text-blue-600" />
                    <span className="text-sm text-slate-700">
                      {t('SendFeedback.tips.tip3')}
                    </span>
                  </div>
                  <div className="flex items-start gap-3 rounded-lg bg-white/50 p-3 backdrop-blur-sm">
                    <Heart className="mt-0.5 h-4 w-4 flex-shrink-0 text-purple-600" />
                    <span className="text-sm text-slate-700">
                      {t('SendFeedback.tips.tip4')}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Footer */}
        <div className="mt-12 text-center">
          <p className="text-sm text-slate-500">
            {t('SendFeedback.footer.thankYou')}
          </p>
        </div>
      </div>
    </div>
  );
}