import PageWrapper from '@/components/PageWrapper/PageWrapper';
import CKEditorField from '@/components/customFieldsFormik/CKEditorField';
import DateTimePickerField from '@/components/customFieldsFormik/DateTimePickerField';
import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import cachedKeys from '@/consts/cachedKeys';
import { ExamStatus, ExamType } from '@/consts/common';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import useGetExamDetail from '@/services/modules/manageexam/hooks/useGetExamDetail';
import type { IManageExamFormValue } from '@/services/modules/manageexam/interfaces/manageExam.interface';
import manageExamService from '@/services/modules/manageexam/manageExam.service';
import useGetListAllRooms from '@/services/modules/room/hooks/useGetAllRooms';
import { useSave } from '@/stores/useStores';
import { Form, Formik } from 'formik';
import {
  BookOpen,
  CheckCircle,
  Clock,
  FileText,
  GraduationCap,
  Plus,
  Settings,
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';
import SelectedQuestionsDisplay from '../components/selected-questions-display';
import DialogAddQuestionBankTeacher from '../dialogs/DialogBankQuestion';
import { useTranslation } from 'react-i18next';

const AddNewExamLecture = () => {
  const { t } = useTranslation('shared');
  const { examId } = useParams();
  const navigate = useNavigate();
  const save = useSave();

  const { data: dataExamDetail } = useGetExamDetail(examId, {
    isTrigger: !!examId,
  });

  const [selectedQuestions, setSelectedQuestions] = useState<any[]>([]);
  const [selectedQuestionBankId, setSelectedQuestionBankId] = useState<string>('');

  // Sync selectedQuestions and selectedQuestionBankId with dataExamDetail when it changes
  useEffect(() => {
    if (examId && dataExamDetail?.questions) {
      const newSelectedQuestions = dataExamDetail.questions.map((q) => ({
        questionId: q.questionId,
        content: q.content,
        difficulty: q.difficulty?.toString(),
        questionBankName: q.questionBankName,
        subjectName: dataExamDetail?.roomName,
      }));
      setSelectedQuestions(newSelectedQuestions);
      setSelectedQuestionBankId(dataExamDetail.questions[0]?.questionBankId || '');
    } else {
      setSelectedQuestions([]);
      setSelectedQuestionBankId('');
    }
  }, [examId, dataExamDetail]);

  const [
    isOpenDialogAddQuestionBankTeacher,
    toggleDialogAddQuestionBankTeacher,
    shouldRenderDialogAddQuestionBankTeacher,
  ] = useToggleDialog();

  const { filters: filterRoom } = useFiltersHandler({
    PageSize: 50,
    CurrentPage: 1,
    TextSearch: '',
  });

  const { data: dataRoomList } = useGetListAllRooms(filterRoom, {
    isTrigger: true,
  });

  const initialValues: IManageExamFormValue = useMemo(
    () => ({
      examId: examId || '',
      roomId: examId ? dataExamDetail?.roomId || '' : '',
      title: examId ? dataExamDetail?.title || '' : '',
      description: examId ? dataExamDetail?.description || '' : '',
      duration: examId ? dataExamDetail?.duration || 0 : 60,
      startTime: examId ? (dataExamDetail?.startTime ?? null) : new Date(),
      endTime: examId ? (dataExamDetail?.endTime ?? null) : new Date(),
      isShowResult: examId ? dataExamDetail?.isShowResult || false : true,
      isShowCorrectAnswer: examId ? dataExamDetail?.isShowCorrectAnswer || false : true,
      status: examId ? dataExamDetail?.status || 0 : 0,
      examType: examId ? dataExamDetail?.examType || 0 : 0,
      questionBankId: selectedQuestionBankId,
      questionIds: selectedQuestions.map((q) => q.questionId),
      guideLines: examId
        ? typeof dataExamDetail?.guideLines === 'string'
          ? dataExamDetail.guideLines
          : ''
        : '',
      verifyCamera: examId ? dataExamDetail?.verifyCamera || false : true,
    }),
    [examId, dataExamDetail, selectedQuestionBankId, selectedQuestions],
  );

  const validationSchema = Yup.object({
    title: Yup.string().required(t('ExamManagement.titleRequired')),
    examType: Yup.number().required(t('ExamManagement.examTypeRequired')),
    duration: Yup.number()
      .required(t('ExamManagement.durationRequired'))
      .min(1, t('ExamManagement.durationMin'))
      .max(1440, t('ExamManagement.durationMax')),
    startTime: Yup.date().required(t('ExamManagement.startTimeRequired')),
    endTime: Yup.date()
      .required(t('ExamManagement.endTimeRequired'))
      .min(Yup.ref('startTime'), t('ExamManagement.endTimeMin'))
      .test('is-future', t('ExamManagement.endTimeFuture'), (value, ctx) => {
        if (!value) return false;
        const now = new Date();
        const start = ctx.parent.startTime;
        return value >= start && (value >= now || start >= now);
      }),

    isShowCorrectAnswer: Yup.boolean().required(t('ExamManagement.isShowCorrectAnswerRequired')),
    status: Yup.number().required(t('ExamManagement.statusRequired')),
    isShowResult: Yup.boolean().required(t('ExamManagement.isShowResultRequired')),
    roomId: Yup.string().required(t('ExamManagement.roomIdRequired')),
    guideLines: Yup.string()
      .optional()
      .test('is-not-empty', t('ExamManagement.guideLinesRequired'), (value) => {
        const strippedValue = value?.replace(/<[^>]+>/g, '') ?? '';
        return strippedValue.trim().length > 0;
      }),
  });

  const handleSubmitAddNewExam = useCallback(
    async (values: IManageExamFormValue) => {
      try {
        if (examId) {
          await manageExamService.updateExam({
            ...values,
            questionIds: selectedQuestions.map((q) => q.questionId),
            questionBankId: selectedQuestionBankId,
          });
          save(cachedKeys.dataExamTeacher, null);
          save(cachedKeys.forceRefetchExamTeacher, true);
          showSuccess(t('ExamManagement.updateSuccess'));
          navigate(-1);
          return;
        }
        await manageExamService.addNewExam({
          ...values,
          questionIds: selectedQuestions.map((q) => q.questionId),
          questionBankId: selectedQuestionBankId,
        });
        save(cachedKeys.dataExamTeacher, null);
        save(cachedKeys.forceRefetchExamTeacher, true);
        showSuccess(t('ExamManagement.addSuccess'));
        navigate(-1);
      } catch (error) {
        showError(error);
      }
    },
    [selectedQuestions, selectedQuestionBankId, navigate, examId, save, t],
  );

  const handleQuestionBankSubmit = useCallback(
    (data: { questionBankId: string; selectedQuestions: any[] }) => {
      setSelectedQuestionBankId(data.questionBankId);
      setSelectedQuestions(data.selectedQuestions);
    },
    [],
  );

  const handleRemoveQuestion = useCallback((questionId: string) => {
    setSelectedQuestions((prev) => prev.filter((q) => q.questionId !== questionId));
  }, []);

  const handleMoveQuestion = useCallback(
    (fromIndex: number, toIndex: number) => {
      if (toIndex < 0 || toIndex >= selectedQuestions.length) return;
      setSelectedQuestions((prev) => {
        const newQuestions = [...prev];
        const [movedQuestion] = newQuestions.splice(fromIndex, 1);
        newQuestions.splice(toIndex, 0, movedQuestion);
        return newQuestions;
      });
    },
    [selectedQuestions.length],
  );

  return (
    <PageWrapper name="Quản lý đề thi" className="bg-white dark:bg-gray-900">
      <div className="min-h-screen bg-gradient-to-br from-slate-50 via-blue-50 to-indigo-50">
        <div className="h-full w-full">
          {shouldRenderDialogAddQuestionBankTeacher && (
            <DialogAddQuestionBankTeacher
              isOpen={isOpenDialogAddQuestionBankTeacher}
              toggle={toggleDialogAddQuestionBankTeacher}
              onSubmit={handleQuestionBankSubmit}
              initialSelectedQuestions={selectedQuestions}
            />
          )}

          <div className="mb-4">
            <div className="mb-1 flex items-center gap-2">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-gradient-to-r from-blue-600 to-indigo-600 text-white shadow-md">
                <GraduationCap className="h-5 w-5" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">
                  {examId ? t('ExamManagement.updateTitle') : t('ExamManagement.createTitle')}
                </h1>
                <p className="mt-0.5 text-sm text-gray-600">{t('ExamManagement.description')}</p>
              </div>
            </div>
          </div>

          <Card className="min-h-[calc(100vh-6rem)] w-full border-0 bg-white/80 shadow-md backdrop-blur-sm">
            <CardContent className="h-full p-6">
              <Formik
                initialValues={initialValues}
                validationSchema={validationSchema}
                onSubmit={handleSubmitAddNewExam}
                enableReinitialize
              >
                {({ isSubmitting, values, errors }) => {
                  return (
                    <Fragment>
                      <Form className="space-y-6">
                        {/* Question Bank Section */}
                        <div className="space-y-4">
                          <div className="flex items-center gap-2">
                            <div className="flex h-7 w-7 items-center justify-center rounded-md bg-emerald-100 text-emerald-600">
                              <BookOpen className="h-3.5 w-3.5" />
                            </div>
                            <h2 className="text-lg font-semibold text-gray-900">
                              {t('ExamManagement.questionBank')}
                            </h2>
                            {selectedQuestions.length > 0 && (
                              <Badge
                                variant="secondary"
                                className="bg-emerald-100 px-2 py-0.5 text-xs text-emerald-700"
                              >
                                {selectedQuestions.length} questions
                              </Badge>
                            )}
                          </div>

                          <div className="rounded-lg border border-gray-200 bg-gray-50/50 p-4">
                            <div className="space-y-3">
                              <div>
                                <label className="mb-2 block text-xs font-medium text-gray-700">
                                  {t('ExamManagement.selectQuestionBank')}{' '}
                                  <span className="text-red-500">*</span>
                                </label>
                                <Button
                                  type="button"
                                  variant="outline"
                                  onClick={toggleDialogAddQuestionBankTeacher}
                                  className="h-10 w-full justify-start border-2 border-dashed border-gray-300 bg-white transition-all duration-200 hover:border-blue-400 hover:bg-blue-50"
                                >
                                  <Plus className="mr-2 h-3.5 w-3.5" />
                                  {selectedQuestions.length > 0
                                    ? `Selected ${selectedQuestions.length} questions`
                                    : 'Select questions from the question bank...'}
                                </Button>
                              </div>

                              <SelectedQuestionsDisplay
                                questions={selectedQuestions}
                                onRemoveQuestion={handleRemoveQuestion}
                                onMoveQuestion={handleMoveQuestion}
                              />

                              {errors.questionIds && (
                                <div className="text-xs text-red-500">{errors.questionIds}</div>
                              )}
                            </div>
                          </div>
                        </div>

                        <Separator className="my-6" />

                        {/* Basic Information Section */}
                        <div className="space-y-4">
                          <div className="flex items-center gap-2">
                            <div className="flex h-7 w-7 items-center justify-center rounded-md bg-blue-100 text-blue-600">
                              <FileText className="h-3.5 w-3.5" />
                            </div>
                            <h2 className="text-lg font-semibold text-gray-900">
                              {t('ExamManagement.basicInformation')}
                            </h2>
                          </div>

                          <div className="grid gap-6 lg:grid-cols-2">
                            <div className="space-y-1.5">
                              <FormikField
                                component={SelectField}
                                name="roomId"
                                label={t('ExamManagement.room')}
                                placeholder={t('ExamManagement.selectRoom')}
                                required
                                options={
                                  dataRoomList?.map((item) => ({
                                    value: item.roomId,
                                    label: item.roomCode,
                                  })) || []
                                }
                                shouldHideSearch
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={SelectField}
                                name="examType"
                                placeholder={t('ExamManagement.selectExamType')}
                                label={t('ExamManagement.examType')}
                                required
                                options={ExamType.map((item) => ({
                                  value: item.value,
                                  label: item.label,
                                }))}
                                shouldHideSearch
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={InputField}
                                name="title"
                                placeholder={t('ExamManagement.enterExamTitle')}
                                label={t('ExamManagement.examTitle')}
                                required
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={InputField}
                                name="description"
                                placeholder={t('ExamManagement.enterExamDescription')}
                                label={t('ExamManagement.examDescription')}
                              />
                            </div>
                          </div>
                        </div>

                        <Separator className="my-6" />

                        {/* Time Settings Section */}
                        <div className="space-y-4">
                          <div className="flex items-center gap-2">
                            <div className="flex h-7 w-7 items-center justify-center rounded-md bg-orange-100 text-orange-600">
                              <Clock className="h-3.5 w-3.5" />
                            </div>
                            <h2 className="text-lg font-semibold text-gray-900">
                              {t('ExamManagement.timeSettings')}
                            </h2>
                          </div>

                          <div className="grid gap-6 lg:grid-cols-3">
                            <div className="space-y-1.5">
                              <FormikField
                                nextText={'(minutes)'}
                                component={InputField}
                                name="duration"
                                placeholder={t('ExamManagement.enterExamDuration')}
                                label={t('ExamManagement.examDuration')}
                                required
                                isNumberic
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={DateTimePickerField}
                                name="startTime"
                                placeholder={t('ExamManagement.enterStartTime')}
                                label={t('ExamManagement.startTime')}
                                required
                                disableCallback={(date: any) => {
                                  return date <= new Date();
                                }}
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={DateTimePickerField}
                                name="endTime"
                                placeholder={t('ExamManagement.enterEndTime')}
                                label={t('ExamManagement.endTime')}
                                required
                                disableCallback={(date: any) => {
                                  if (values.startTime) {
                                    return date <= new Date(values.startTime);
                                  }
                                  return false;
                                }}
                              />
                            </div>
                          </div>
                        </div>

                        <Separator className="my-6" />

                        {/* Display Settings Section */}
                        <div className="space-y-4">
                          <div className="flex items-center gap-2">
                            <div className="flex h-7 w-7 items-center justify-center rounded-md bg-purple-100 text-purple-600">
                              <Settings className="h-3.5 w-3.5" />
                            </div>
                            <h2 className="text-lg font-semibold text-gray-900">
                              {t('ExamManagement.displaySettings')}
                            </h2>
                          </div>

                          <div className="grid gap-6 lg:grid-cols-4">
                            <div className="space-y-1.5">
                              <FormikField
                                component={SelectField}
                                name="isShowResult"
                                placeholder={t('ExamManagement.showResult')}
                                label={t('ExamManagement.showResult')}
                                required
                                options={[
                                  { value: true, label: t('ExamManagement.showResult') },
                                  { value: false, label: t('ExamManagement.hideResult') },
                                ]}
                                shouldHideSearch
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={SelectField}
                                name="isShowCorrectAnswer"
                                placeholder={t('ExamManagement.showCorrectAnswer')}
                                label={t('ExamManagement.showCorrectAnswer')}
                                required
                                options={[
                                  { value: true, label: t('ExamManagement.showCorrectAnswer') },
                                  { value: false, label: t('ExamManagement.hideCorrectAnswer') },
                                ]}
                                shouldHideSearch
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={SelectField}
                                name="status"
                                placeholder={t('ExamManagement.status')}
                                label={t('ExamManagement.status')}
                                required
                                options={ExamStatus.map((item) => ({
                                  value: item.value,
                                  label: item.label,
                                }))}
                                shouldHideSearch
                              />
                            </div>

                            <div className="space-y-1.5">
                              <FormikField
                                component={SelectField}
                                name="verifyCamera"
                                placeholder={t('ExamManagement.verifyCamera')}
                                label={t('ExamManagement.verifyCamera')}
                                required
                                options={[
                                  { value: true, label: t('ExamManagement.verifyCamera') },
                                  { value: false, label: t('ExamManagement.dontVerifyCamera') },
                                ]}
                                shouldHideSearch
                              />
                            </div>
                          </div>

                          <div className="space-y-1.5">
                            <CKEditorField
                              label={t('ExamManagement.guideLines')}
                              name="guideLines"
                            />
                            {errors.guideLines && (
                              <div className="text-xs text-red-500">{errors.guideLines}</div>
                            )}
                          </div>
                        </div>

                        {/* Action Buttons */}
                        <div className="flex flex-col-reverse gap-2 pt-6 sm:flex-row sm:justify-end">
                          <Button
                            variant="outline"
                            type="button"
                            className="h-10 bg-transparent px-6 font-medium"
                          >
                            {t('Close')}
                          </Button>
                          <Button
                            type="submit"
                            isLoading={isSubmitting}
                            className="h-10 bg-gradient-to-r from-blue-600 to-indigo-600 px-6 font-medium shadow-md hover:from-blue-700 hover:to-indigo-700"
                          >
                            <CheckCircle className="mr-2 h-3.5 w-3.5" />
                            {examId
                              ? t('ExamManagement.updateExam')
                              : t('ExamManagement.createExam')}
                          </Button>
                        </div>
                      </Form>
                    </Fragment>
                  );
                }}
              </Formik>
            </CardContent>
          </Card>
        </div>
      </div>
    </PageWrapper>
  );
};

export default AddNewExamLecture;
