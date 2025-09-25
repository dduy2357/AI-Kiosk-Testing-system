import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import { showError } from '@/helpers/toast';
import type { DialogI } from '@/interfaces/common';
import { Form, Formik } from 'formik';
import { AlertTriangle, CheckCircle, Info } from 'lucide-react';
import type React from 'react';
import { Fragment } from 'react';
import { useTranslation } from 'react-i18next';
import { Button } from '../ui/button';

type ButtonVariant = 'link' | 'default' | 'destructive' | 'outline' | 'secondary' | 'ghost';

interface DialogConfirmProps extends DialogI<any> {
  title: React.ReactNode;
  content: React.ReactNode;
  variantYes?: ButtonVariant;
  type?: 'warning' | 'success' | 'info';
}

const DialogConfirm = (props: DialogConfirmProps) => {
  const { isOpen, toggle, onSubmit, title, content, variantYes, type = 'warning' } = props;
  const { t } = useTranslation('shared');

  const getIcon = () => {
    switch (type) {
      case 'success':
        return <CheckCircle className="h-6 w-6 text-green-500" />;
      case 'info':
        return <Info className="h-6 w-6 text-blue-500" />;
      default:
        return <AlertTriangle className="h-6 w-6 text-amber-500" />;
    }
  };

  const getColorScheme = () => {
    switch (type) {
      case 'success':
        return 'bg-green-50 border-green-200';
      case 'info':
        return 'bg-blue-50 border-blue-200';
      default:
        return 'bg-amber-50 border-amber-200';
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/20 backdrop-blur-md transition-all duration-300 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0">
        <DialogPortal>
          <DialogContent className="fixed left-[50%] top-[50%] z-50 w-full max-w-md translate-x-[-50%] translate-y-[-50%] rounded-xl border bg-white p-0 shadow-2xl duration-300 data-[state=open]:animate-in data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:fade-in-0 data-[state=closed]:zoom-out-95 data-[state=open]:zoom-in-95 data-[state=closed]:slide-out-to-left-1/2 data-[state=closed]:slide-out-to-top-[48%] data-[state=open]:slide-in-from-left-1/2 data-[state=open]:slide-in-from-top-[48%]">
            <Formik
              initialValues={{}}
              onSubmit={async (values, formikHelpers) => {
                try {
                  if (onSubmit) {
                    await onSubmit(values, formikHelpers);
                  }
                } catch (error) {
                  showError(error);
                }
              }}
            >
              {({ isSubmitting }) => {
                return (
                  <Fragment>
                    <div
                      className={`flex items-center gap-4 rounded-t-xl border-b p-6 ${getColorScheme()}`}
                    >
                      <div className="flex-shrink-0">{getIcon()}</div>
                      <div className="flex-1">
                        {title && (
                          <DialogTitle className="text-lg font-semibold leading-tight text-gray-900">
                            {title}
                          </DialogTitle>
                        )}
                      </div>
                    </div>

                    <div className="p-6">
                      {content && (
                        <DialogDescription className="text-sm leading-relaxed text-gray-600">
                          {content}
                        </DialogDescription>
                      )}
                    </div>

                    <div className="flex items-center justify-end gap-3 rounded-b-xl bg-gray-50 px-6 py-4">
                      <Form className="flex gap-3">
                        <Button
                          variant="outline"
                          type="button"
                          onClick={toggle}
                          className="min-w-[80px] bg-transparent font-medium"
                          disabled={isSubmitting}
                        >
                          {t('Close')}
                        </Button>
                        <Button
                          type="submit"
                          isLoading={isSubmitting}
                          variant={variantYes ?? 'destructive'}
                          className="min-w-[80px] font-medium shadow-sm"
                        >
                          {t('Yes')}
                        </Button>
                      </Form>
                    </div>
                  </Fragment>
                );
              }}
            </Formik>
          </DialogContent>
        </DialogPortal>
      </DialogOverlay>
    </Dialog>
  );
};

export default DialogConfirm;
