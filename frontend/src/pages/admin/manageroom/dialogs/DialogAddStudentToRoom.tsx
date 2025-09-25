import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Dialog, DialogContent, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import Timer from '@/helpers/timer';
import { showError } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useGetListUserInRoom from '@/services/modules/user/hooks/useGetListUserInRoom';
import { AllRoomUser } from '@/services/modules/userinroom/interfaces/userinroom.interface';
import { Form, Formik } from 'formik';
import { Loader2, Search, X } from 'lucide-react';
import { useCallback, useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import * as Yup from 'yup';

export interface StudentInRoomFormValues {
  studentList: string[];
}

interface DialogAddStudentInRoomProps {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: StudentInRoomFormValues) => Promise<void>;
  roomId: string;
}

const timer = new Timer();

const DialogAddStudentInRoom = (props: DialogAddStudentInRoomProps) => {
  const { t } = useTranslation('shared');
  const { isOpen, toggle, onSubmit, roomId } = props;
  const [searchValue, setSearchValue] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [initialFetchDone, setInitialFetchDone] = useState(false);
  const [allStudents, setAllStudents] = useState<AllRoomUser[]>([]);

  const { filters, setFilters } = useFiltersHandler({
    RoomId: roomId ?? '',
    PageSize: 10,
    CurrentPage: 1,
    TextSearch: '',
  });

  const { data, loading, hasMore, refetch, totalPage } = useGetListUserInRoom(roomId, filters, {
    isTrigger: isOpen,
    isLoadmore: true,
  });

  const initialValues: StudentInRoomFormValues = {
    studentList: [],
  };

  useEffect(() => {
    if (isOpen && !initialFetchDone) {
      setAllStudents([]);
      setFilters({ PageSize: 10, CurrentPage: 1, TextSearch: '', RoomId: roomId ?? '' });
      refetch();
      setInitialFetchDone(true);
    } else if (!isOpen) {
      setInitialFetchDone(false);
      setSearchValue('');
      setAllStudents([]);
      setFilters({
        RoomId: roomId ?? '',
        PageSize: 10,
        CurrentPage: 1,
        TextSearch: '',
      });
    }
  }, [isOpen, refetch, initialFetchDone, setFilters, roomId]);

  useEffect(() => {
    if (data?.length) {
      if (filters.CurrentPage === 1) {
        setAllStudents(data);
      } else {
        setAllStudents((prev) => [
          ...prev,
          ...data.filter(
            (newStudent) => !prev.some((student) => student.userId === newStudent.userId),
          ),
        ]);
      }
    }
  }, [data, filters.CurrentPage]);

  const prevFiltersRef = useRef(filters);
  useEffect(() => {
    if (
      isOpen &&
      !loading &&
      (filters.CurrentPage !== prevFiltersRef.current.CurrentPage ||
        filters.TextSearch !== prevFiltersRef.current.TextSearch ||
        filters.PageSize !== prevFiltersRef.current.PageSize)
    ) {
      if (
        filters.TextSearch !== prevFiltersRef.current.TextSearch ||
        filters.PageSize !== prevFiltersRef.current.PageSize
      ) {
        setAllStudents([]); // Reset allStudents on search or pageSize change
      }
      refetch();
    }
    prevFiltersRef.current = filters;
  }, [
    filters.CurrentPage,
    filters.TextSearch,
    filters.PageSize,
    isOpen,
    refetch,
    loading,
    filters,
  ]);

  const handleSearch = useCallback(
    (value: string) => {
      setSearchValue(value);
      timer.debounce(() => {
        setFilters((prev) => ({
          ...prev,
          CurrentPage: 1,
          TextSearch: value,
        }));
      }, 500);
    },
    [setFilters],
  );

  const handleClose = () => {
    setSearchValue('');
    setAllStudents([]);
    setFilters({
      RoomId: roomId ?? '',
      PageSize: 10,
      CurrentPage: 1,
      TextSearch: '',
    });
    toggle();
  };

  const handleLoadMore = () => {
    if (!loading && hasMore && filters.CurrentPage < totalPage) {
      setFilters((prev) => ({
        ...prev,
        CurrentPage: prev.CurrentPage + 1,
      }));
    }
  };

  const validationSchema = Yup.object({
    studentList: Yup.array().min(1, t('ExamRoomManagement.RequiredStudentList')),
  });

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogContent className="max-w-2xl gap-0 p-0">
        <Formik
          initialValues={initialValues}
          validationSchema={validationSchema}
          onSubmit={async (values, { setSubmitting }) => {
            setIsSubmitting(true);
            try {
              await onSubmit(values);
            } catch (error) {
              showError(error);
            } finally {
              setIsSubmitting(false);
              setSubmitting(false);
            }
          }}
        >
          {({ values, setFieldValue, errors, touched }) => (
            <Form>
              {/* Header */}
              <div className="flex items-center justify-between p-6 pb-4">
                <DialogTitle className="text-xl font-semibold text-gray-900">
                  {t('ExamRoomManagement.AddStudentToRoom')}
                </DialogTitle>
                <Button
                  variant="ghost"
                  size="icon"
                  onClick={handleClose}
                  className="h-6 w-6 rounded-full hover:bg-gray-100"
                  type="button"
                >
                  <X className="h-4 w-4" />
                </Button>
              </div>

              <div className="px-6">
                <hr className="border-gray-200" />
              </div>

              {/* Content */}
              <div className="px-6 py-4">
                <div className="mb-4">
                  <label className="text-sm font-medium text-gray-700">
                    {t('ExamRoomManagement.SelectStudent')} <span className="text-red-500">*</span>
                  </label>
                </div>

                {/* Search Input */}
                <div className="relative mb-4">
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
                  <Input
                    placeholder={t('ExamRoomManagement.SearchStudentPlaceholder')}
                    value={searchValue}
                    onChange={(e) => handleSearch(e.target.value)}
                    className="pl-10"
                  />
                </div>

                {/* Student List */}
                <div className="max-h-80 overflow-y-auto rounded-lg border border-gray-200 bg-gray-50">
                  <div className="space-y-3 p-4">
                    {loading && !allStudents?.length ? (
                      <div className="flex items-center justify-center py-8">
                        <Loader2 className="h-6 w-6 animate-spin text-gray-400" />
                        <span className="ml-2 text-gray-500">
                          {t('ExamRoomManagement.Loading')}
                        </span>
                      </div>
                    ) : allStudents?.length ? (
                      <>
                        {allStudents.map((student) => {
                          const isSelected = values.studentList.includes(student.userId);
                          return (
                            <div
                              key={student.userId}
                              className="flex items-center space-x-3 rounded-lg border border-gray-100 bg-white p-3 transition-colors hover:border-gray-200"
                            >
                              <Checkbox
                                id={student.userId}
                                checked={isSelected}
                                onCheckedChange={(checked) => {
                                  if (checked) {
                                    setFieldValue('studentList', [
                                      ...values.studentList,
                                      student.userId,
                                    ]);
                                  } else {
                                    setFieldValue(
                                      'studentList',
                                      values.studentList.filter((id) => id !== student.userId),
                                    );
                                  }
                                }}
                                className="h-4 w-4"
                              />
                              <div className="flex flex-1 items-center space-x-3">
                                <Badge
                                  variant="secondary"
                                  className="bg-orange-100 text-orange-700 hover:bg-orange-100"
                                >
                                  {student.userCode ?? 'SV001'}
                                </Badge>
                                <span className="flex-1 font-medium text-gray-900">
                                  {student.fullName}
                                </span>
                                <Badge
                                  variant="secondary"
                                  className="bg-green-100 text-green-700 hover:bg-green-100"
                                >
                                  {student.major ?? 'CNTT01'}
                                </Badge>
                              </div>
                            </div>
                          );
                        })}
                        {hasMore && (
                          <div className="flex justify-center pt-4">
                            <Button
                              onClick={handleLoadMore}
                              disabled={loading ?? !hasMore}
                              className="bg-blue-600 px-6 hover:bg-blue-700"
                            >
                              {loading ? (
                                <>
                                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                                  {t('ExamRoomManagement.Loading')}
                                </>
                              ) : (
                                t('ExamRoomManagement.LoadMore')
                              )}
                            </Button>
                          </div>
                        )}
                      </>
                    ) : (
                      <div className="flex items-center justify-center py-8">
                        <span className="text-gray-500">
                          {t('ExamRoomManagement.NoStudentsFound')}
                        </span>
                      </div>
                    )}
                  </div>
                </div>

                {/* Error Message */}
                {errors.studentList && touched.studentList && (
                  <p className="mt-2 text-sm text-red-500">{errors.studentList}</p>
                )}

                {/* Footer Info */}
                <div className="mt-4 flex items-center justify-between text-sm text-gray-500">
                  <span>
                    {t('ExamRoomManagement.SelectedStudents')}: {values.studentList.length}{' '}
                    {t('ExamRoomManagement.Students')}
                  </span>
                  <span>
                    {t('ExamRoomManagement.AvailableStudents')}: {allStudents?.length}{' '}
                    {t('ExamRoomManagement.Students')}
                  </span>
                </div>
              </div>

              <div className="px-6">
                <hr className="border-gray-200" />
              </div>

              {/* Action Buttons */}
              <div className="flex justify-end gap-3 p-6 pt-4">
                <Button
                  variant="outline"
                  onClick={handleClose}
                  disabled={isSubmitting}
                  type="button"
                  className="px-6"
                >
                  Hủy
                </Button>
                <Button
                  type="submit"
                  disabled={values.studentList.length === 0 || isSubmitting}
                  className="bg-blue-600 px-6 hover:bg-blue-700"
                >
                  {isSubmitting ? (
                    <>
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                      {t('ExamRoomManagement.Submitting')}
                    </>
                  ) : (
                    <>
                      {t('ExamRoomManagement.Adding')} {values.studentList.length}{' '}
                      {t('ExamRoomManagement.Students')}
                    </>
                  )}
                </Button>
              </div>
            </Form>
          )}
        </Formik>
      </DialogContent>
    </Dialog>
  );
};

export default DialogAddStudentInRoom;
