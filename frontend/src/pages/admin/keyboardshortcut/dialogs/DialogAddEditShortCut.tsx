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
import { OPTION_RISK_LEVEL } from '@/consts/common';
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import useGetDetailKeyboardShortcut from '@/services/modules/keyboardshortcut/hooks/useGetDetailKeyboardShortcut';
import { Form, Formik } from 'formik';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

export interface KeyboardShortcutFormValues {
  keyId?: string;
  keyCode?: string;
  keyCombination?: string;
  description?: string;
  riskLevel?: number;
  isActive?: boolean;
}

interface DialogAddNewKeyboardShortcutProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: KeyboardShortcutFormValues) => Promise<void>;
  editKeyboardShortcut?: KeyboardShortcutFormValues | null;
}

const DialogAddNewKeyboardShortcut = (props: DialogAddNewKeyboardShortcutProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, editKeyboardShortcut } = props;

  const { data: detailKeyboardShortcut } = useGetDetailKeyboardShortcut(
    editKeyboardShortcut?.keyId,
    {
      isTrigger: !!editKeyboardShortcut,
    },
  );

  const initialValues: KeyboardShortcutFormValues = {
    keyId: editKeyboardShortcut?.keyId ?? detailKeyboardShortcut?.keyId ?? '',
    keyCode: editKeyboardShortcut?.keyCode ?? detailKeyboardShortcut?.keyCode ?? '',
    keyCombination:
      editKeyboardShortcut?.keyCombination ?? detailKeyboardShortcut?.keyCombination ?? '',
    description: editKeyboardShortcut?.description ?? detailKeyboardShortcut?.description ?? '',
    riskLevel: editKeyboardShortcut?.riskLevel ?? detailKeyboardShortcut?.riskLevel ?? 0,
    isActive: editKeyboardShortcut?.isActive ?? detailKeyboardShortcut?.isActive ?? false,
  };

  const validationSchema = Yup.object({
    keyCode: Yup.string()
      .required(t('KeyboardShortcutManagement.KeyCodeRequired'))
      .max(100, t('KeyboardShortcutManagement.KeyCodeMaxLength'))
      .matches(/^[a-zA-Z0-9]+$/, t('KeyboardShortcutManagement.KeyCodeInvalid')),
    keyCombination: Yup.string()
      .required(t('KeyboardShortcutManagement.KeyCombinationRequired'))
      .max(100, t('KeyboardShortcutManagement.KeyCombinationMaxLength')),
    description: Yup.string().max(500, t('KeyboardShortcutManagement.DescriptionMaxLength')),
    riskLevel: Yup.number().required(t('KeyboardShortcutManagement.RiskLevelRequired')),
    isActive: Yup.boolean(),
  });

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
            {({ isSubmitting, values }) => (
              <Fragment>
                <Form className="space-y-4">
                  <div>
                    <DialogTitle className="text-xl font-medium">
                      {editKeyboardShortcut ? t('Edit') : t('Add')}{' '}
                      {t('KeyboardShortcutManagement.KeyboardShortcut')}
                    </DialogTitle>
                  </div>

                  <hr className="border-t border-gray-200" />

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <FormikField
                        component={InputField}
                        name="keyCode"
                        label={t('KeyboardShortcutManagement.KeyCode')}
                        placeholder={t('KeyboardShortcutManagement.KeyCodePlaceholder')}
                        value={values.keyCode}
                        required
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={InputField}
                        name="keyCombination"
                        label={t('KeyboardShortcutManagement.KeyCombination')}
                        placeholder={t('KeyboardShortcutManagement.KeyCombinationPlaceholder')}
                        value={values.keyCombination}
                        required
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={SelectField}
                      name="riskLevel"
                      label={t('KeyboardShortcutManagement.RiskLevel')}
                      placeholder={t('KeyboardShortcutManagement.RiskLevelPlaceholder')}
                      options={OPTION_RISK_LEVEL}
                      shouldHideSearch
                      required
                    />
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={Textarea}
                      name="description"
                      placeholder={t('KeyboardShortcutManagement.DescriptionPlaceholder')}
                      label={t('KeyboardShortcutManagement.Description')}
                    />
                  </div>

                  <div className="space-y-2">
                    <FormikField
                      component={CheckBoxField}
                      name="isActive"
                      label={t('KeyboardShortcutManagement.IsActive')}
                      checked={values.isActive}
                    />
                  </div>

                  <div className="flex justify-end gap-2 pt-4">
                    <DialogClose asChild>
                      <Button variant="outline" type="button">
                        Hủy
                      </Button>
                    </DialogClose>
                    <Button type="submit" isLoading={isSubmitting}>
                      {editKeyboardShortcut ? t('Edit') : t('Add')}
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

export default DialogAddNewKeyboardShortcut;
