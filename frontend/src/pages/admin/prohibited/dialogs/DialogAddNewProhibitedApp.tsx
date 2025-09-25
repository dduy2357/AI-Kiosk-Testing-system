import CheckBoxField from '@/components/customFieldsFormik/CheckBoxField';
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
import useGetDetailProhibited from '@/services/modules/prohibited/hooks/useGetDetailProhibited';
import { ProhbitedList } from '@/services/modules/prohibited/interfaces/prohibited.interface';
import { Form, Formik } from 'formik';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

export interface ProhibitedFormValues {
  AppId?: string;
  AppName: string;
  ProcessName: string;
  Description?: string;
  AppIconUrl?: string;
  IsActive?: boolean;
  RiskLevel?: number;
  Category?: number;
  TypeApp?: string;
}

interface DialogAddNewProhibitedAppProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: ProhibitedFormValues) => Promise<void>;
  editProhibited?: ProhbitedList | null;
}

const DialogAddNewProhibitedApp = (props: DialogAddNewProhibitedAppProps) => {
  //!State
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, editProhibited } = props;
  const { data: detailProhibited } = useGetDetailProhibited(editProhibited?.appId, {
    isTrigger: !!editProhibited,
  });

  const validationSchema = Yup.object({
    AppName: Yup.string().required(t('ProhibitedManagement.AppNameRequired')),
    ProcessName: Yup.string().required(t('ProhibitedManagement.ProcessNameRequired')),
    Category: Yup.number().required(t('ProhibitedManagement.CategoryRequired')).nullable(),
    RiskLevel: Yup.number().required(t('ProhibitedManagement.RiskLevelRequired')).nullable(),
    TypeApp: Yup.string().required(t('ProhibitedManagement.TypeAppRequired')),
  });

  const initialValues: ProhibitedFormValues = {
    AppId: editProhibited?.appId ?? detailProhibited?.appId ?? '',
    AppName: editProhibited?.appName ?? detailProhibited?.appName ?? '',
    ProcessName: editProhibited?.processName ?? detailProhibited?.processName ?? '',
    Description: editProhibited?.description ?? detailProhibited?.description ?? '',
    AppIconUrl: editProhibited?.appIconUrl ?? detailProhibited?.appIconUrl ?? '',
    IsActive: editProhibited?.isActive ?? detailProhibited?.isActive ?? true,
    RiskLevel: editProhibited?.riskLevel ?? detailProhibited?.riskLevel ?? 1,
    Category: editProhibited?.category ?? detailProhibited?.category ?? 1,
    TypeApp: String(editProhibited?.typeApp ?? detailProhibited?.typeApp ?? ''),
  };

  //!Render
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/10 backdrop-blur-sm" />
      <DialogPortal>
        <DialogContent className="max-w-xl">
          <Formik
            enableReinitialize
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
            {({ isSubmitting, values }) => (
              <Fragment>
                <Form className="space-y-4">
                  <div>
                    <DialogTitle className="text-xl font-medium">
                      {editProhibited
                        ? t('ProhibitedManagement.UpdateProhibitedApp')
                        : t('ProhibitedManagement.CreateProhibitedApp')}
                    </DialogTitle>
                  </div>

                  <hr className="border-t border-gray-200" />

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <FormikField
                        component={InputField}
                        name="AppName"
                        label={t('ProhibitedManagement.AppName')}
                        placeholder={t('ProhibitedManagement.AppNamePlaceholder')}
                        value={values.AppName}
                        required
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={InputField}
                        name="ProcessName"
                        label={t('ProhibitedManagement.ProcessName')}
                        placeholder={t('ProhibitedManagement.ProcessNamePlaceholder')}
                        value={values.ProcessName}
                        required
                      />
                    </div>
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <FormikField
                        component={SelectField}
                        name="Category"
                        label={t('ProhibitedManagement.Category')}
                        placeholder={t('ProhibitedManagement.CategoryPlaceholder')}
                        options={[
                          { label: 'Website', value: 1 },
                          { label: 'Tool', value: 2 },
                        ]}
                        shouldHideSearch
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={SelectField}
                        name="RiskLevel"
                        label={t('ProhibitedManagement.RiskLevel')}
                        placeholder={t('ProhibitedManagement.RiskLevelPlaceholder')}
                        options={[
                          { label: t('ProhibitedManagement.LowRisk'), value: 0 },
                          { label: t('ProhibitedManagement.MediumRisk'), value: 1 },
                          { label: t('ProhibitedManagement.HighRisk'), value: 2 },
                        ]}
                        shouldHideSearch
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={InputField}
                      name="AppIconUrl"
                      label={t('ProhibitedManagement.AppIconUrl')}
                      placeholder={t('ProhibitedManagement.AppIconUrlPlaceholder')}
                      value={values.AppIconUrl}
                    />
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={SelectField}
                      name="TypeApp"
                      placeholder={t('ProhibitedManagement.TypeAppPlaceholder')}
                      label={t('ProhibitedManagement.TypeApp')}
                      options={[
                        { label: t('ProhibitedManagement.TypeAppDeny'), value: '0' },
                        { label: t('ProhibitedManagement.TypeAppAllow'), value: '1' },
                      ]}
                      shouldHideSearch
                    />
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={Textarea}
                      name="Description"
                      label={t('ProhibitedManagement.Description')}
                      placeholder={t('ProhibitedManagement.DescriptionPlaceholder')}
                    />
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={CheckBoxField}
                      name="IsActive"
                      label={t('ProhibitedManagement.IsActive')}
                      checked={values.IsActive}
                    />
                  </div>

                  <div className="flex justify-end gap-2 pt-4">
                    <DialogClose asChild>
                      <Button variant="outline" type="button">
                        {t('Close')}
                      </Button>
                    </DialogClose>
                    <Button type="submit" isLoading={isSubmitting}>
                      {editProhibited ? t('Edit') : t('Add')}
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

export default DialogAddNewProhibitedApp;
