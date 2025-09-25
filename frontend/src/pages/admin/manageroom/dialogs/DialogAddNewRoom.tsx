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
import { showError } from '@/helpers/toast';
import { DialogI } from '@/interfaces/common';
import useGetAllClasses from '@/services/modules/class/hooks/useGetAllClasses';
import useGetDetailRoom from '@/services/modules/room/hooks/useGetRoomDetail';
import { RoomList } from '@/services/modules/room/interfaces/room.interface';
import useGetListSubject from '@/services/modules/subject/hooks/useGetAllSubject';
import { Form, Formik } from 'formik';
import { useMemo } from 'react';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

export interface RoomFormValues {
  roomId?: string;
  classId?: string;
  subjectId?: string;
  isActive?: boolean;
  roomDescription?: string;
  capacity?: number;
  roomCode?: string;
}

interface DialogAddNewRoomProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: RoomFormValues) => Promise<void>;
  editRoom?: RoomList | null;
}

const DialogAddNewRoom = (props: DialogAddNewRoomProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, editRoom } = props;
  const { data: roomDetail } = useGetDetailRoom(editRoom?.roomId, {
    isTrigger: !!editRoom,
  });

  const validationSchema = Yup.object({
    roomCode: Yup.string()
      .required(t('ExamRoomManagement.RoomCodeRequired'))
      .matches(/^[A-Z0-9]+$/, t('ExamRoomManagement.RoomCodeFormat')),
    capacity: Yup.number()
      .required(t('ExamRoomManagement.CapacityRequired'))
      .min(1, t('ExamRoomManagement.CapacityMin')),
    roomDescription: Yup.string().required(t('ExamRoomManagement.RoomDescriptionRequired')),
    isActive: Yup.boolean().required(t('ExamRoomManagement.StatusRequired')),
  });

  const { data: dataClasses, loading: loadingClasses } = useGetAllClasses(
    useMemo(() => ({ PageSize: 50, CurrentPage: 1, TextSearch: '' }), []),
  );

  const { data: dataSubject, loading: loadingSubject } = useGetListSubject(
    useMemo(() => ({ pageSize: 50, currentPage: 1, textSearch: '', status: true }), []),
  );

  const initialValues = useMemo(
    () => ({
      isActive: editRoom?.isRoomActive ?? true,
      roomDescription: editRoom?.roomDescription ?? roomDetail?.roomDescription ?? '',
      roomCode: editRoom?.roomCode ?? roomDetail?.roomCode ?? '',
      capacity: editRoom?.capacity ?? roomDetail?.capacity ?? 0,
      classId: editRoom?.classId ?? roomDetail?.classId ?? '',
      subjectId: editRoom?.subjectId ?? roomDetail?.subjectId ?? '',
    }),
    [editRoom, roomDetail],
  );

  const classOptions = useMemo(
    () =>
      dataClasses?.map((item: any) => ({
        label: item.classCode,
        value: item.classId,
      })) ?? [],
    [dataClasses],
  );

  const subjectOptions = useMemo(
    () =>
      dataSubject?.map((item: any) => ({
        label: item.subjectName,
        value: item.subjectId,
      })) ?? [],
    [dataSubject],
  );

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
                      {editRoom
                        ? t('ExamRoomManagement.EditRoom')
                        : t('ExamRoomManagement.CreateRoom')}
                    </DialogTitle>
                  </div>

                  <div className="grid grid-cols-1 gap-4">
                    <div className="space-y-2">
                      <FormikField
                        id="roomCode"
                        component={InputField}
                        name="roomCode"
                        label={t('ExamRoomManagement.RoomCode')}
                        placeholder={t('ExamRoomManagement.RoomCodePlaceholder')}
                        value={values.roomCode}
                        required
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={SelectField}
                        name="classId"
                        placeholder={t('ExamRoomManagement.ClassPlaceholder')}
                        options={classOptions}
                        label={t('ExamRoomManagement.ClassLabel')}
                        required
                        shouldHideSearch
                        loading={loadingClasses}
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={SelectField}
                        name="subjectId"
                        label={t('ExamRoomManagement.SubjectLabel')}
                        placeholder={t('ExamRoomManagement.SubjectPlaceholder')}
                        options={subjectOptions}
                        required
                        shouldHideSearch
                        loading={loadingSubject}
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={InputField}
                        id="capacity"
                        name="capacity"
                        label={t('ExamRoomManagement.CapacityLabel')}
                        placeholder={t('ExamRoomManagement.CapacityPlaceholder')}
                        value={values.capacity}
                        required
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={InputField}
                        id="roomDescription"
                        name="roomDescription"
                        label={t('ExamRoomManagement.RoomDescriptionLabel')}
                        placeholder={t('ExamRoomManagement.RoomDescriptionPlaceholder')}
                        value={values.roomDescription}
                        required
                      />
                    </div>

                    <div className="space-y-2">
                      <FormikField
                        component={SelectField}
                        name="isActive"
                        placeholder={t('ExamRoomManagement.StatusPlaceholder')}
                        label={t('ExamRoomManagement.StatusLabel')}
                        options={[
                          { label: t('ExamRoomManagement.Active'), value: true },
                          { label: t('ExamRoomManagement.Inactive'), value: false },
                        ]}
                        required
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
                      {editRoom
                        ? t('ExamRoomManagement.EditRoom')
                        : t('ExamRoomManagement.CreateRoom')}
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

export default DialogAddNewRoom;
