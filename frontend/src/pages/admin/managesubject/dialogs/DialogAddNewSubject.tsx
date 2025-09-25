import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import Loading from '@/components/ui/loading';
import { Textarea } from '@/components/ui/textarea';
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import useGetDetailSubject from '@/services/modules/subject/hooks/useGetDetailSubject';
import { ISubjectForm } from '@/services/modules/subject/interfaces/subject.interface';
import { Form, Formik } from 'formik';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

interface DialogAddNewSubjectProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: ISubjectForm) => Promise<void>;
  editSubject?: ISubjectForm | null;
}

const DialogAddNewSubject = (props: DialogAddNewSubjectProps) => {
  //!State
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, editSubject } = props;
  const { isLoading: isLoadingDetail, data: detailSubject } = useGetDetailSubject(
    editSubject?.subjectId,
    {
      isTrigger: !!editSubject,
    },
  );

  const initialValues: ISubjectForm = {
    subjectName: editSubject ? editSubject.subjectName : '',
    subjectCode: editSubject ? editSubject.subjectCode : '',
    subjectDescription: editSubject ? editSubject.subjectDescription : '',
    status: editSubject ? editSubject.status : true,
    subjectContent: editSubject ? editSubject.subjectContent : '',
  };

  const validationSchema = Yup.object({
    subjectName: Yup.string()
      .required(t('SubjectManagement.SubjectNameRequired'))
      .min(2, t('SubjectManagement.SubjectNameMinLength'))
      .max(100, t('SubjectManagement.SubjectNameMaxLength')),
    subjectCode: Yup.string()
      .required(t('SubjectManagement.SubjectCodeRequired'))
      .matches(/^[A-Z]{2,5}\d{3}$/, t('SubjectManagement.SubjectCodeFormat')),
    subjectDescription: Yup.string().max(500, t('SubjectManagement.SubjectDescriptionMaxLength')),
    subjectContent: Yup.string().max(1000, t('SubjectManagement.SubjectContentMaxLength')),
    status: Yup.boolean().required(t('SubjectManagement.StatusRequired')),
  });
  //!Functions

  if (isLoadingDetail) {
    return (
      <Dialog open={isOpen} onOpenChange={toggle}>
        <DialogOverlay className="fixed inset-0 bg-black/10 backdrop-blur-sm" />
        <DialogPortal>
          <DialogContent className="max-w-xl">
            <div className="flex h-full items-center justify-center">
              <Loading />
            </div>
          </DialogContent>
        </DialogPortal>
      </Dialog>
    );
  }

  //!Render
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/10 backdrop-blur-sm" />
      <DialogPortal>
        <DialogContent className="max-w-xl">
          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={async (values, { setSubmitting }) => {
              try {
                await onSubmit(values);
              } catch (error) {
                showError(error);
              }
              setSubmitting(false);
            }}
          >
            {({ isSubmitting, values }) => {
              return (
                <Fragment>
                  <Form className="space-y-4">
                    <div>
                      <DialogTitle className="text-xl font-medium">
                        {editSubject
                          ? `${t('SubjectManagement.EditSubject')} ${detailSubject?.subjectName}`
                          : t('SubjectManagement.AddNewSubject')}
                      </DialogTitle>
                    </div>

                    <div className="grid grid-cols-1 gap-4">
                      <div className="space-y-2">
                        <FormikField
                          id="subjectName"
                          component={InputField}
                          name="subjectName"
                          label={t('SubjectManagement.SubjectName')}
                          placeholder={t('SubjectManagement.SubjectNamePlaceholder')}
                          value={values.subjectName}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={InputField}
                          id="subjectCode"
                          name="subjectCode"
                          label={t('SubjectManagement.SubjectCode')}
                          placeholder={t('SubjectManagement.SubjectCodePlaceholder')}
                          value={values.subjectCode}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={SelectField}
                          name="status"
                          label={t('SubjectManagement.Status')}
                          placeholder={t('SubjectManagement.StatusPlaceholder')}
                          options={[
                            { label: t('SubjectManagement.Active'), value: true },
                            { label: t('SubjectManagement.Inactive'), value: false },
                          ]}
                          shouldHideSearch
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={Textarea}
                          name="subjectDescription"
                          label={t('SubjectManagement.SubjectDescription')}
                          placeholder={t('SubjectManagement.SubjectDescriptionPlaceholder')}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={Textarea}
                          name="subjectContent"
                          placeholder={t('SubjectManagement.SubjectContentPlaceholder')}
                          label={t('SubjectManagement.SubjectContent')}
                          required
                        />
                      </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                      <DialogClose asChild>
                        <Button variant="outline" type="button">
                          Hủy
                        </Button>
                      </DialogClose>
                      <Button type="submit" isLoading={isSubmitting}>
                        {editSubject
                          ? t('SubjectManagement.EditSubject')
                          : t('SubjectManagement.AddNewSubject')}
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

export default DialogAddNewSubject;
