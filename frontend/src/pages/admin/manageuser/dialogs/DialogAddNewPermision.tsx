import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import SelectField from '@/components/customFieldsFormik/SelectField';
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
import useGetDetailPermission from '@/services/modules/authorize/hooks/useGetDetailPermission';
import { PermissionsList } from '@/services/modules/authorize/interfaces/permission.interface';
import { Form, Formik } from 'formik';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';
export interface PermissionFormValues {
  id?: string;
  name?: string;
  action?: string;
  resource?: string;
  categoryID?: string;
  isActive?: boolean;
}

interface DialogAddNewPermisionProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: PermissionFormValues) => Promise<void>;
  dataMain?: PermissionsList[];
  editId?: string;
}

const DialogAddNewPermision = (props: DialogAddNewPermisionProps) => {
  //!State
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, dataMain, editId } = props;
  const { data: detailPermission } = useGetDetailPermission(editId ?? '');

  const permission = Array.isArray(detailPermission) ? detailPermission[0] : detailPermission;

  const initialValues: PermissionFormValues = {
    name: permission?.name ?? '',
    action: permission?.action ?? '',
    resource: permission?.resource ?? '',
    categoryID: permission?.categoryID ?? '',
    isActive: permission?.isActive ?? true,
  };

  const validationSchema = Yup.object({
    name: Yup.string().required(t('UserManagement.PermissionNameRequired')),
    action: Yup.string().required(t('UserManagement.ActionRequired')),
    resource: Yup.string()
      .required(t('UserManagement.ResourceRequired'))
      .matches(/^[a-zA-Z0-9_\-/{}/?]+$/, t('UserManagement.ResourceFormatInvalid')),
    categoryID: Yup.string().required(t('UserManagement.CategoryRequired')),
    isActive: Yup.boolean().required(t('UserManagement.StatusRequired')),
  });

  //!Functions

  //!Render
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/10 backdrop-blur-sm" />
      <DialogPortal>
        <DialogContent className="max-w-2xl">
          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={async (values, { setSubmitting }) => {
              try {
                setSubmitting(true);
                await onSubmit(values);
              } catch (error) {
                showError(error);
              } finally {
                setSubmitting(false);
              }
            }}
            enableReinitialize
          >
            {({ isSubmitting, values }) => {
              console;
              return (
                <Fragment>
                  <Form className="space-y-4">
                    <div>
                      <DialogTitle className="text-xl font-medium">
                        {editId
                          ? t('UserManagement.EditPermissionTitle')
                          : t('UserManagement.AddPermissionTitle')}
                      </DialogTitle>
                      <DialogDescription className="mt-1 text-sm text-gray-500">
                        {editId
                          ? t('UserManagement.EditPermissionDescription')
                          : t('UserManagement.AddPermissionDescription')}
                      </DialogDescription>
                    </div>

                    <div className="grid grid-cols-1 gap-4">
                      <div className="space-y-2">
                        <FormikField
                          id="name"
                          component={InputField}
                          name="name"
                          label={t('UserManagement.PermissionName')}
                          placeholder={t('UserManagement.PermissionNamePlaceholder')}
                          value={values.name}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={SelectField}
                          name="action"
                          placeholder={t('UserManagement.ActionPlaceholder')}
                          label={t('UserManagement.Action')}
                          options={[
                            { value: 'create', label: t('UserManagement.Create') },
                            { value: 'view', label: t('UserManagement.View') },
                            { value: 'update', label: t('UserManagement.Update') },
                            { value: 'delete', label: t('UserManagement.Delete') },
                          ]}
                          required
                          shouldHideSearch
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={SelectField}
                          name="categoryID"
                          placeholder={t('UserManagement.CategoryPlaceholder')}
                          options={
                            dataMain?.map((item) => ({
                              value: item.categoryId,
                              label: item.description,
                            })) ?? []
                          }
                          label={t('UserManagement.CategoryLabel')}
                          required
                          shouldHideSearch
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={InputField}
                          id="resource"
                          name="resource"
                          placeholder={t('UserManagement.ResourcePlaceholder')}
                          value={values.resource}
                          label={t('UserManagement.ResourceLabel')}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={SelectField}
                          name="isActive"
                          placeholder={t('UserManagement.StatusPlaceholder')}
                          options={[
                            { value: true, label: t('UserManagement.Active') },
                            { value: false, label: t('UserManagement.Inactive') },
                          ]}
                          label={t('UserManagement.StatusLabel')}
                          required
                          shouldHideSearch
                        />
                      </div>
                    </div>

                    <div className="flex justify-end gap-2 pt-4">
                      <DialogClose asChild>
                        <Button variant="outline" type="button">
                          {t('Close')}
                        </Button>
                      </DialogClose>
                      <Button type="submit" isLoading={isSubmitting}>
                        {editId
                          ? t('UserManagement.EditPermissionTitle')
                          : t('UserManagement.AddPermissionTitle')}
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

export default React.memo(DialogAddNewPermision);
