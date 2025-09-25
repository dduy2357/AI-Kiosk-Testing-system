import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogPortal,
  DialogOverlay,
  DialogContent,
  DialogTitle,
} from '@/components/ui/dialog';
import { DialogI } from '@/interfaces/common';
import useGetViolationDetail from '@/services/modules/violation/hooks/useGetViolationDetail';
import { Form, Formik } from 'formik';
import {
  AlertTriangle,
  Calendar,
  Camera,
  Code,
  FileText,
  Mail,
  RefreshCw,
  User,
} from 'lucide-react';
import { Fragment } from 'react';
import { useTranslation } from 'react-i18next';

interface DialogProps extends DialogI<any> {
  violationId: string;
}

const DialogViolationDetail = (props: DialogProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit } = props;
  const { data: selectedViolation, loading } = useGetViolationDetail(props.violationId, {
    isTrigger: isOpen,
  });

  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay />
        <DialogContent className="max-h-[80vh] max-w-7xl overflow-y-auto">
          <Formik initialValues={{}} onSubmit={onSubmit || (() => {})}>
            {() => {
              return (
                <Fragment>
                  <DialogTitle className="flex items-center gap-2">
                    <AlertTriangle className="h-5 w-5 text-orange-500" />
                    {t('ExamSupervision.ViolationDetail')}
                  </DialogTitle>
                  {loading ? (
                    <div className="flex items-center justify-center py-12">
                      <RefreshCw className="h-6 w-6 animate-spin text-blue-500" />
                      <span className="ml-2 text-gray-600">
                        {t('ExamSupervision.LoadingDetail')}
                      </span>
                    </div>
                  ) : (
                    selectedViolation && (
                      <div className="space-y-6">
                        {/* Student Information */}
                        <div className="rounded-lg bg-blue-50 p-4">
                          <h3 className="mb-3 flex items-center gap-2 text-lg font-semibold">
                            <User className="h-5 w-5 text-blue-600" />
                            {t('ExamSupervision.StudentInformation')}
                          </h3>
                          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.StudentName')}:
                              </label>
                              <p className="font-medium">{selectedViolation.studentName}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.StudentCode')}:
                              </label>
                              <p className="flex items-center gap-1 font-medium">
                                <Code className="h-4 w-4" />
                                {selectedViolation.studentCode}
                              </p>
                            </div>
                            <div className="md:col-span-2">
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.Email')}:
                              </label>
                              <p className="flex items-center gap-1 font-medium">
                                <Mail className="h-4 w-4" />
                                {selectedViolation.studentEmail}
                              </p>
                            </div>
                          </div>
                        </div>

                        <div className="rounded-lg bg-red-50 p-4">
                          <h3 className="mb-3 flex items-center gap-2 text-lg font-semibold">
                            <AlertTriangle className="h-5 w-5 text-red-600" />
                            {t('ExamSupervision.ViolationDetail')}
                          </h3>
                          <div className="space-y-3">
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.ViolationType')}:
                              </label>
                              <p className="font-medium text-red-700">
                                {selectedViolation.violationName}
                              </p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.ViolationDescription')}:
                              </label>
                              <p className="rounded border bg-white p-3">
                                {selectedViolation.message}
                              </p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.ViolationTime')}:
                              </label>
                              <p className="flex items-center gap-1 font-medium">
                                <Calendar className="h-4 w-4" />
                                {new Date(selectedViolation.createdAt).toLocaleString('vi-VN')}
                              </p>
                            </div>
                          </div>
                        </div>

                        <div className="rounded-lg bg-green-50 p-4">
                          <h3 className="mb-3 flex items-center gap-2 text-lg font-semibold">
                            <User className="h-5 w-5 text-green-600" />
                            Người báo cáo
                          </h3>
                          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.ReporterName')}:
                              </label>
                              <p className="font-medium">{selectedViolation.creatorName}</p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.ReporterCode')}:
                              </label>
                              <p className="flex items-center gap-1 font-medium">
                                <Code className="h-4 w-4" />
                                {selectedViolation.creatorCode}
                              </p>
                            </div>
                            <div className="md:col-span-2">
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.Email')}:
                              </label>
                              <p className="flex items-center gap-1 font-medium">
                                <Mail className="h-4 w-4" />
                                {selectedViolation.creatorEmail}
                              </p>
                            </div>
                          </div>
                        </div>

                        {selectedViolation.screenshotPath && (
                          <div className="rounded-lg bg-gray-50 p-4">
                            <h3 className="mb-3 flex items-center gap-2 text-lg font-semibold">
                              <Camera className="h-5 w-5 text-gray-600" />
                              {t('ExamSupervision.ViolationScreenshot')}
                            </h3>
                            <div className="overflow-hidden rounded-lg border">
                              <img
                                src={selectedViolation.screenshotPath}
                                alt="Screenshot bằng chứng vi phạm"
                                className="h-auto max-h-96 w-full bg-white object-contain"
                                onError={(e) => {
                                  const target = e.target as HTMLImageElement;
                                  target.src =
                                    '/placeholder.svg?height=200&width=400&text=Không thể tải hình ảnh';
                                }}
                              />
                            </div>
                            <p className="mt-2 text-sm text-gray-600">
                              {t('ExamSupervision.ScreenshotAttimeViolation')}
                            </p>
                          </div>
                        )}

                        <div className="rounded-lg bg-yellow-50 p-4">
                          <h3 className="mb-3 flex items-center gap-2 text-lg font-semibold">
                            <FileText className="h-5 w-5 text-yellow-600" />
                            {t('ExamSupervision.ViolationMetadata')}
                          </h3>
                          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.ViolationId')}:
                              </label>
                              <p className="rounded border bg-white p-2 font-mono text-sm">
                                {selectedViolation.violationId}
                              </p>
                            </div>
                            <div>
                              <label className="text-sm font-medium text-gray-600">
                                {t('ExamSupervision.StudentExamId')}:
                              </label>
                              <p className="rounded border bg-white p-2 font-mono text-sm">
                                {selectedViolation.studentExamId}
                              </p>
                            </div>
                          </div>
                        </div>
                      </div>
                    )
                  )}
                  <Form className="mt-[25px] flex justify-end gap-2">
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

export default DialogViolationDetail;
