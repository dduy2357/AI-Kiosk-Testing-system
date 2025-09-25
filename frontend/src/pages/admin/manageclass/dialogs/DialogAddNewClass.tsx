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
import { Textarea } from '@/components/ui/textarea';
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import useGetDetailClass from '@/services/modules/class/hooks/useGetDetailClass';
import { ClassList } from '@/services/modules/class/interfaces/class.interface';
import { Form, Formik } from 'formik';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

interface DialogAddNewClassProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: ClassList) => Promise<void>;
  editClass?: ClassList | null;
}

const DialogAddNewClass = (props: DialogAddNewClassProps) => {
  //!State
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, editClass } = props;
  const { data: classDetail } = useGetDetailClass(editClass?.classId, {
    isTrigger: !!editClass,
  });

  const initialValues: ClassList = {
    classCode: editClass?.classCode ?? classDetail?.classCode ?? '',
    description: editClass?.description ?? classDetail?.description ?? '',
    isActive: editClass?.isActive !== undefined ? editClass.isActive : true,
  };

  const validationSchema = Yup.object({
    classCode: Yup.string()
      .required(t('ClassManagement.ClassCodeRequired'))
      .matches(/^[A-Z0-9]+$/, t('ClassManagement.ClassCodeInvalid')),
  });

  //!Functions

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
                        {editClass
                          ? t('ClassManagement.EditClass')
                          : t('ClassManagement.CreateNewClass')}
                      </DialogTitle>
                    </div>

                    <div className="grid grid-cols-1 gap-4">
                      <div className="space-y-2">
                        <FormikField
                          id="classCode"
                          component={InputField}
                          name="classCode"
                          label={t('ClassManagement.ClassCode')}
                          placeholder={t('ClassManagement.ClassCodePlaceholder')}
                          value={values.classCode}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={Textarea}
                          name="description"
                          placeholder={t('ClassManagement.DescriptionPlaceholder')}
                          label={t('ClassManagement.Description')}
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={SelectField}
                          name="isActive"
                          label={t('ClassManagement.Status')}
                          placeholder={t('ClassManagement.StatusPlaceholder')}
                          options={[
                            { label: t('ClassManagement.Active'), value: true },
                            { label: t('ClassManagement.Inactive'), value: false },
                          ]}
                          shouldHideSearch
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
                        {editClass
                          ? t('ClassManagement.UpdateClass')
                          : t('ClassManagement.CreateClass')}
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

export default DialogAddNewClass;
