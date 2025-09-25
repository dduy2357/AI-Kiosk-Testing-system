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
import { Textarea } from '@/components/ui/textarea';
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import { PermissionList } from '@/services/modules/authorize/interfaces/role.interface';
import { Form, Formik } from 'formik';
import React from 'react';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

export interface RoleFormValues {
  id?: string;
  name?: string;
  description?: string;
  isActive?: boolean;
}

interface DialogAddNewRoleProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: RoleFormValues) => Promise<void>;
  editRole?: PermissionList | null;
}

const DialogAddNewRole = (props: DialogAddNewRoleProps) => {
  //!State
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, editRole } = props;

  const validationSchema = Yup.object({
    name: Yup.string().required(t('UserManagement.NameRequired')),
    description: Yup.string().required(t('UserManagement.DescriptionRequired')),
  });

  const initialValues: RoleFormValues = {
    id: editRole ? String(editRole.roleId) : undefined,
    name: editRole ? editRole?.roleName : '',
    description: editRole ? editRole?.description : '',
    // isActive: editRole ? editRole?.isActive : false,
  };

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
              return (
                <Fragment>
                  <Form className="space-y-4">
                    <div>
                      <DialogTitle className="text-xl font-medium">
                        {editRole
                          ? t('UserManagement.UpdatePermission')
                          : t('UserManagement.AddPermission')}
                      </DialogTitle>
                      <DialogDescription className="mt-1 text-sm text-gray-500">
                        {editRole
                          ? t('UserManagement.EditPermissionDescription')
                          : t('UserManagement.AddNewPermissionDescription')}
                      </DialogDescription>
                    </div>

                    <div className="grid grid-cols-1 gap-4">
                      <div className="space-y-2">
                        <FormikField
                          id="name"
                          component={InputField}
                          name="name"
                          label={t('UserManagement.RoleName')}
                          placeholder={t('UserManagement.RoleNamePlaceholder')}
                          value={values.name}
                          required
                        />
                      </div>

                      <div className="space-y-2">
                        <FormikField
                          component={Textarea}
                          name="description"
                          label={t('UserManagement.RoleDescription')}
                          placeholder={t('UserManagement.RoleDescriptionPlaceholder')}
                          required
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
                        {editRole ? t('Edit') : t('Add')}
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

export default React.memo(DialogAddNewRole);
