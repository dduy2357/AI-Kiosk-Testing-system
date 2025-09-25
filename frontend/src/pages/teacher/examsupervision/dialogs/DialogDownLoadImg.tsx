import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import type { DialogI } from '@/interfaces/common';
import { Form, Formik } from 'formik';
import { Download } from 'lucide-react';
import { Fragment } from 'react';
import type { PhotoItem } from '../pages/DetailConnectionSupervisor';
import { useTranslation } from 'react-i18next';

interface DialogProps extends DialogI<any> {
  selectedPhoto?: PhotoItem | null;
}

const DialogDownLoadImg = (props: DialogProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, selectedPhoto } = props;

  const getStatusText = (status: string | undefined) => {
    switch (status) {
      case 'normal':
        return 'Bình thường';
      case 'warning':
        return 'Cảnh báo';
      case 'violation':
        return 'Vi phạm';
      default:
        return 'Không xác định';
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay className="bg-black/30 backdrop-blur-sm" />
        <DialogContent className="h-auto max-w-2xl">
          <Formik initialValues={{}} onSubmit={onSubmit ?? (() => {})}>
            {({ isSubmitting }) => {
              return (
                <Fragment>
                  <DialogTitle>{t('ExamSupervision.DownloadImageTitle')}</DialogTitle>
                  <DialogDescription>
                    {selectedPhoto ? (
                      <div className="space-y-2">
                        <p>{t('ExamSupervision.DownloadImageDescription')}</p>
                        <div className="grid grid-cols-2 gap-2 text-sm">
                          <div className="font-medium">
                            {t('ExamSupervision.DownloadImageTimestamp')}
                          </div>
                          <div>{selectedPhoto.timestamp ?? 'N/A'}</div>
                          <div className="font-medium">
                            {t('ExamSupervision.DownloadImageStatus')}
                          </div>
                          <div>{getStatusText(selectedPhoto.status)}</div>
                        </div>
                        {selectedPhoto.imageUrl && (
                          <img
                            src={selectedPhoto.imageUrl ?? '/placeholder.svg'}
                            alt="Preview"
                            className="h-full w-full rounded-md object-cover"
                          />
                        )}
                      </div>
                    ) : (
                      <p className="text-red-500">{t('ExamSupervision.NoImageSelected')}</p>
                    )}
                  </DialogDescription>
                  <Form className="mt-[25px] flex justify-end gap-2">
                    <Button type="submit" isLoading={isSubmitting} disabled={!selectedPhoto}>
                      <Download className="mr-2 h-4 w-4" />
                      {t('Download')}
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

export default DialogDownLoadImg;
