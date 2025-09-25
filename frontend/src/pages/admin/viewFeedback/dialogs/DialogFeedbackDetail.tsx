import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogPortal,
  DialogOverlay,
  DialogContent,
  DialogTitle,
} from '@/components/ui/dialog';
import { AvatarFallback } from '@/components/ui/avatar';
import { Avatar } from '@radix-ui/react-avatar';
import { Calendar, Mail, RefreshCw, User } from 'lucide-react';
import { Badge } from '@/components/ui/badge';

import { DialogI } from '@/interfaces/common';
import useGetFeedbackDetail from '@/services/modules/feedback/hooks/useGetFeedbackDetail';
import { Form, Formik } from 'formik';
import { Fragment } from 'react';
import { convertUTCToVietnamTime } from '@/helpers/common';
import { DateTimeFormat } from '@/consts/dates';
import { useTranslation } from 'react-i18next';

interface DialogProps extends DialogI<any> {
  feedbackId: string;
}

const DialogFeedbackDetail = (props: DialogProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit } = props;
  const getInitials = (name: string) => {
    if (!name || typeof name !== 'string') return '??';
    return name
      .split(' ')
      .filter((n) => n.length > 0)
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };
  const truncateContent = (content: string, maxLength = 100) => {
    if (!content || typeof content !== 'string') return '';
    if (content.length <= maxLength) return content;
    return content.substring(0, maxLength) + '...';
  };
  // const { t } = useTranslation("shared");

  const { data: dataFeedbackDetail, loading } = useGetFeedbackDetail(props.feedbackId, {
    isTrigger: isOpen,
  });
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogPortal>
        <DialogOverlay />
        <DialogContent>
          <Formik initialValues={{}} onSubmit={onSubmit ?? (() => {})}>
            {() => {
              return (
                <Fragment>
                  <DialogTitle>{t('AdminFeedback.FeedbackDetails')}</DialogTitle>

                  {loading ? (
                    <div className="flex items-center justify-center py-12">
                      <RefreshCw className="h-6 w-6 animate-spin text-blue-500" />
                      <span className="ml-2 text-gray-600">{t('AdminFeedback.LoadingDetail')}</span>
                    </div>
                  ) : dataFeedbackDetail ? (
                    <div className="space-y-6">
                      {/* Feedback ID */}
                      <div className="rounded-lg bg-gray-50 p-3">
                        <p className="font-mono text-xs text-gray-500">
                          ID: {dataFeedbackDetail.feedbackId}
                        </p>
                      </div>

                      {/* Student Info */}
                      <div className="flex items-start gap-4">
                        <Avatar className="h-12 w-12">
                          <AvatarFallback className="bg-blue-100 text-blue-600">
                            {getInitials(dataFeedbackDetail.studentName)}
                          </AvatarFallback>
                        </Avatar>
                        <div className="flex-1">
                          <h3 className="mb-2 text-lg font-semibold text-gray-900">
                            {dataFeedbackDetail.title}
                          </h3>
                          <div className="grid grid-cols-1 gap-3 text-sm md:grid-cols-2">
                            <div className="flex items-center gap-2 text-gray-600">
                              <User className="h-4 w-4" />
                              <span>{dataFeedbackDetail.studentName}</span>
                            </div>
                            <div className="flex items-center gap-2 text-gray-600">
                              <Badge variant="secondary" className="text-xs">
                                {dataFeedbackDetail.studentCode}
                              </Badge>
                            </div>
                            <div className="flex items-center gap-2 text-gray-600">
                              <Mail className="h-4 w-4" />
                              <span>{dataFeedbackDetail.studentEmail}</span>
                            </div>
                            <div className="flex items-center gap-2 text-gray-600">
                              <Calendar className="h-4 w-4" />
                              {convertUTCToVietnamTime(
                                dataFeedbackDetail.createdAt,
                                DateTimeFormat.DateTimeWithTimezone,
                              )?.toString() ?? 'N/A'}
                            </div>
                          </div>
                        </div>
                      </div>

                      <div>
                        <h4 className="mb-3 font-medium text-gray-900">
                          {t('AdminFeedback.Content')}
                        </h4>
                        <div className="rounded-lg border bg-gray-50 p-4">
                          <p className="whitespace-pre-wrap leading-relaxed text-gray-700">
                            {truncateContent(dataFeedbackDetail.content)}
                          </p>
                        </div>
                      </div>
                    </div>
                  ) : null}
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

export default DialogFeedbackDetail;
