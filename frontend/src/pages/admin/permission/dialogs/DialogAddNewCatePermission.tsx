import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import { DialogI } from '@/interfaces/common';
import useGetDetailCategoryPermission from '@/services/modules/permission/hooks/useGetDetailPermission';
import { Form, Formik } from 'formik';
import { Fragment } from 'react';
import { useTranslation } from 'react-i18next';
import * as Yup from 'yup';

interface DialogProps extends DialogI<any> {
  onSubmit?: (values: any) => void;
  permissionId?: string | null;
}

const DialogAddNewCategoryPermission = (props: DialogProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, permissionId } = props;
  const { data: permissionDetail } = useGetDetailCategoryPermission(permissionId ?? '', {
    isTrigger: !!permissionId,
  });

  const initialValues = {
    categoryID: permissionId ? permissionDetail?.categoryId : '',
    description: permissionId ? permissionDetail?.description : '',
  };

  const validationSchema = Yup.object().shape({
    description: Yup.string().required(t('PermissionManagement.DescriptionRequired')),
  });

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay className="fixed inset-0 bg-black/10 backdrop-blur-sm" />
        <DialogContent>
          <Formik
            initialValues={initialValues}
            onSubmit={onSubmit ?? (() => {})}
            validationSchema={validationSchema}
            enableReinitialize
          >
            {({ isSubmitting }) => {
              return (
                <Fragment>
                  <DialogTitle>
                    {permissionId
                      ? t('PermissionManagement.EditTitle')
                      : t('PermissionManagement.AddNewPermission')}
                  </DialogTitle>
                  <DialogDescription>
                    {permissionId
                      ? t('PermissionManagement.EditDescription')
                      : t('PermissionManagement.AddNewPermissionDescription')}
                  </DialogDescription>

                  <hr className="border-t border-gray-200" />

                  <div className="space-y-2">
                    <FormikField
                      component={InputField}
                      name="description"
                      placeholder={t('PermissionManagement.PermissionName')}
                      label={t('PermissionManagement.PermissionName')}
                      required
                    />
                  </div>

                  <Form className="mt-[25px] flex justify-end gap-2">
                    <Button type="submit" isLoading={isSubmitting}>
                      {permissionId
                        ? t('PermissionManagement.EditPermission')
                        : t('PermissionManagement.CreatePermission')}
                    </Button>
                    <Button variant="ghost" type="button" onClick={toggle}>
                      {t('Close')}
                    </Button>
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

export default DialogAddNewCategoryPermission;
