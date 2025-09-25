import FormikField from '@/components/customFieldsFormik/FormikField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogDescription,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { showError } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import type { DialogI } from '@/interfaces/common';
import useGetListBankQuestion from '@/services/modules/bankquestion/hooks/useGetAllBankQuestion';
import useGetQuestionBankDetail from '@/services/modules/bankquestion/hooks/useGetQuestionBankDetail';
import type { IManageExamFormValue } from '@/services/modules/manageexam/interfaces/manageExam.interface';
import { Form, Formik, useFormikContext } from 'formik';
import { BookOpen, CheckCircle2, FileText, Filter, Loader2, Search } from 'lucide-react';
import React, { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Fragment } from 'react/jsx-runtime';
import * as Yup from 'yup';

interface DialogBankQuestionProps extends DialogI<any> {
  isOpen: boolean;
  toggle: () => void;
  onSubmit: (values: { questionBankId: string; selectedQuestions: any[] }) => void;
  initialSelectedQuestions?: any[];
  initialQuestionBankId?: string;
}

const QuestionBankIdSync: React.FC<{
  questionBankId: string | null;
  setQuestionBankId: (value: string | null) => void;
}> = ({ questionBankId, setQuestionBankId }) => {
  const { values } = useFormikContext<IManageExamFormValue>();

  useEffect(() => {
    if (values.questionBankId !== questionBankId) {
      setQuestionBankId(values.questionBankId ?? null);
    }
  }, [values.questionBankId, setQuestionBankId, questionBankId]);

  return null;
};

const DialogAddQuestionBankTeacher = (props: DialogBankQuestionProps) => {
  //! State
  const { isOpen, toggle, onSubmit, initialSelectedQuestions = [] } = props;
  const [searchText, setSearchText] = useState('');
  const { t } = useTranslation('shared');

  const { filters } = useFiltersHandler({
    pageSize: 50,
    currentPage: 1,
    textSearch: '',
  });

  const { data: dataQuestionBank } = useGetListBankQuestion(filters, {
    isTrigger: isOpen,
  });

  const initialValues = useMemo(
    () => ({
      questionBankId: '',
      questionIds: initialSelectedQuestions.map((q) => q.questionId) ?? [],
    }),
    [initialSelectedQuestions],
  );

  const [questionBankId, setQuestionBankId] = React.useState<string | null>(null);

  const { data: dataQuestionBankDetail, isLoading: isLoadingQuestions } = useGetQuestionBankDetail(
    questionBankId,
    {
      isTrigger: !!questionBankId,
    },
  );

  const validationSchema = Yup.object({
    questionBankId: Yup.string().required(t('ExamManagement.QuestionBankRequired')),
    questionIds: Yup.array()
      .min(2, t('ExamManagement.MinTwoQuestionsRequired'))
      .required(t('ExamManagement.QuestionRequired')),
  });

  // Reset questionBankId when dialog closes
  useEffect(() => {
    if (!isOpen) {
      setQuestionBankId(null);
      setSearchText('');
    }
  }, [isOpen]);

  // Filter questions based on search and bank filter
  const filteredQuestions = useMemo(() => {
    if (!dataQuestionBankDetail?.questions) return [];

    let filtered = dataQuestionBankDetail.questions;

    if (searchText) {
      filtered = filtered.filter((question) =>
        question.content.toLowerCase().includes(searchText.toLowerCase()),
      );
    }

    return filtered;
  }, [dataQuestionBankDetail?.questions, searchText]);

  //! Function
  const handleSelectAll = (setFieldValue: any, values: any) => {
    if (values.questionIds.length === filteredQuestions.length) {
      // If all are selected, deselect all
      setFieldValue('questionIds', []);
    } else {
      // Select all questions
      setFieldValue(
        'questionIds',
        filteredQuestions.map((q) => q.questionId),
      );
    }
  };

  //! Render
  return (
    <Dialog open={isOpen} onOpenChange={toggle}>
      <DialogOverlay className="fixed inset-0 bg-black/20 backdrop-blur-sm" />
      <DialogPortal>
        <DialogContent className="flex max-h-[85vh] max-w-5xl flex-col overflow-hidden border-0 shadow-2xl">
          <Formik
            initialValues={initialValues}
            validationSchema={validationSchema}
            onSubmit={async (values, { setSubmitting }) => {
              try {
                setSubmitting(true);
                // Get selected questions with full data
                const selectedQuestions = filteredQuestions
                  .filter((q) => values.questionIds.includes(q.questionId))
                  .map((q) => ({
                    ...q,
                    questionBankName: dataQuestionBankDetail?.questionBankName,
                    subjectName: dataQuestionBankDetail?.subjectName,
                  }));

                onSubmit({
                  questionBankId: values.questionBankId,
                  selectedQuestions,
                });
                toggle();
              } catch (error) {
                showError(error);
              } finally {
                setSubmitting(false);
              }
            }}
            enableReinitialize
          >
            {({ isSubmitting, values, setFieldValue }) => {
              return (
                <Fragment>
                  <QuestionBankIdSync
                    questionBankId={questionBankId}
                    setQuestionBankId={setQuestionBankId}
                  />
                  <Form className="flex h-full flex-col">
                    {/* Header */}
                    <div className="-m-6 mb-6 flex-shrink-0 bg-gradient-to-r from-blue-50 to-indigo-50 p-6">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-3">
                          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-r from-blue-600 to-indigo-600 text-white shadow-lg">
                            <BookOpen className="h-5 w-5" />
                          </div>
                          <div>
                            <DialogTitle className="text-xl font-bold text-gray-900">
                              {t('ExamManagement.SelectQuestions')}
                            </DialogTitle>
                            <DialogDescription className="mt-1 text-gray-600">
                              {t('ExamManagement.SelectQuestionsDescription')}
                            </DialogDescription>
                          </div>
                        </div>
                      </div>
                    </div>

                    {/* Search and Filter Section */}
                    <div className="mb-6 flex-shrink-0 space-y-4">
                      <div className="flex items-center gap-3">
                        <div className="flex h-6 w-6 items-center justify-center rounded-md bg-orange-100 text-orange-600">
                          <Filter className="h-3 w-3" />
                        </div>
                        <h3 className="text-lg font-semibold text-gray-900">
                          {t('ExamManagement.FilterAndSearch')}
                        </h3>
                      </div>

                      <div className="grid gap-4 lg:grid-cols-3">
                        <div className="lg:col-span-2">
                          <div className="relative">
                            <Search className="absolute left-4 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
                            <Input
                              placeholder={t('ExamManagement.SearchPlaceholder')}
                              value={searchText}
                              onChange={(e) => setSearchText(e.target.value)}
                              className="h-11 border-gray-200 pl-11 focus:border-blue-400 focus:ring-blue-400"
                            />
                          </div>
                        </div>

                        <div>
                          <FormikField
                            component={SelectField}
                            name="questionBankId"
                            placeholder={t('ExamManagement.SelectQuestionBank')}
                            required
                            options={
                              dataQuestionBank?.map((item) => ({
                                value: item.questionBankId,
                                label: item.title,
                              })) ?? []
                            }
                            shouldHideSearch
                          />
                        </div>
                      </div>

                      {/* Selection Summary */}
                      {values.questionIds.length > 0 && (
                        <div className="flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 p-3">
                          <CheckCircle2 className="h-4 w-4 text-emerald-600" />
                          <span className="text-sm font-medium text-emerald-800">
                            Chosen {values.questionIds.length} questions
                          </span>
                          <Badge
                            variant="secondary"
                            className="ml-auto bg-emerald-100 text-emerald-700"
                          >
                            {values.questionIds.length >= 2
                              ? t('ExamManagement.SelectQuestions')
                              : t('ExamManagement.NeedMoreQuestions')}
                          </Badge>
                        </div>
                      )}
                    </div>

                    {/* Questions List */}
                    {values.questionBankId && (
                      <div className="min-h-0 flex-1 overflow-hidden">
                        <div className="mb-4 flex items-center justify-between gap-3">
                          <div className="flex items-center gap-3">
                            <div className="flex h-6 w-6 items-center justify-center rounded-md bg-purple-100 text-purple-600">
                              <FileText className="h-3 w-3" />
                            </div>
                            <h3 className="text-lg font-semibold text-gray-900">
                              Question List
                              {dataQuestionBankDetail?.questionBankName && (
                                <span className="ml-2 text-sm font-normal text-gray-500">
                                  from "{dataQuestionBankDetail.questionBankName}"
                                </span>
                              )}
                            </h3>
                          </div>
                          <Button
                            variant="outline"
                            className="h-9 px-4"
                            onClick={() => handleSelectAll(setFieldValue, values)}
                            disabled={filteredQuestions.length === 0}
                          >
                            {values.questionIds.length === filteredQuestions.length
                              ? 'Deselect All'
                              : 'Select All'}
                          </Button>
                        </div>

                        <div className="h-full overflow-y-auto rounded-xl border border-gray-200 bg-gray-50/30">
                          {isLoadingQuestions ? (
                            <div className="flex items-center justify-center py-12">
                              <div className="flex items-center gap-3 text-gray-500">
                                <Loader2 className="h-5 w-5 animate-spin" />
                                <span>Loading questions...</span>
                              </div>
                            </div>
                          ) : filteredQuestions.length > 0 ? (
                            <div className="max-h-96 space-y-3 overflow-y-auto p-4">
                              {filteredQuestions.map((question, index) => (
                                <div
                                  key={question.questionId}
                                  className={`group relative flex items-start space-x-4 rounded-xl border bg-white p-4 shadow-sm transition-all duration-200 hover:cursor-pointer hover:border-blue-200 hover:shadow-md ${
                                    values.questionIds.includes(question.questionId)
                                      ? 'border-blue-300 bg-blue-50/50 shadow-md'
                                      : 'border-gray-200'
                                  }`}
                                >
                                  <div className="flex items-center pt-1">
                                    <Checkbox
                                      checked={values.questionIds.includes(question.questionId)}
                                      onCheckedChange={(checked) => {
                                        if (checked) {
                                          setFieldValue('questionIds', [
                                            ...values.questionIds,
                                            question.questionId,
                                          ]);
                                        } else {
                                          setFieldValue(
                                            'questionIds',
                                            values.questionIds.filter(
                                              (id) => id !== question.questionId,
                                            ),
                                          );
                                        }
                                      }}
                                      className="h-5 w-5 data-[state=checked]:border-blue-600 data-[state=checked]:bg-blue-600"
                                    />
                                  </div>

                                  <div className="min-w-0 flex-1">
                                    <div className="flex items-start justify-between gap-4">
                                      <div className="flex-1">
                                        <div className="mb-2 flex items-center gap-2">
                                          <Badge variant="outline" className="text-xs font-medium">
                                            Câu {index + 1}
                                          </Badge>
                                          {values.questionIds.includes(question.questionId) && (
                                            <Badge className="bg-blue-100 text-xs text-blue-700">
                                              {t('ExamManagement.Selected')}
                                            </Badge>
                                          )}
                                        </div>
                                        <div className="mb-3 text-sm font-medium leading-relaxed text-gray-900">
                                          {question.content}
                                        </div>
                                        <div className="flex items-center gap-4 text-xs text-gray-500">
                                          <span className="flex items-center gap-1">
                                            <BookOpen className="h-3 w-3" />
                                            {dataQuestionBankDetail?.questionBankName ??
                                              'Loading...'}
                                          </span>
                                          {dataQuestionBankDetail?.subjectName && (
                                            <span className="flex items-center gap-1">
                                              <FileText className="h-3 w-3" />
                                              {dataQuestionBankDetail.subjectName}
                                            </span>
                                          )}
                                        </div>
                                      </div>
                                    </div>
                                  </div>
                                </div>
                              ))}
                            </div>
                          ) : (
                            <div className="flex flex-col items-center justify-center py-16 text-gray-500">
                              <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-gray-100">
                                <Search className="h-8 w-8 text-gray-400" />
                              </div>
                              <div className="text-center">
                                <p className="mb-1 text-lg font-medium text-gray-900">
                                  {searchText
                                    ? 'No matching questions found'
                                    : 'No questions available'}
                                </p>
                                <p className="text-sm text-gray-500">
                                  {searchText
                                    ? 'Try changing the search keyword'
                                    : 'Select a question bank to view the list of questions'}
                                </p>
                              </div>
                            </div>
                          )}
                        </div>
                      </div>
                    )}

                    {/* Footer */}
                    <div className="-mx-6 -mb-6 mt-6 flex flex-shrink-0 items-center justify-between border-t bg-gray-50/50 px-6 pb-6 pt-6">
                      <div className="text-sm text-gray-600">
                        {values.questionIds.length < 2 && (
                          <span className="font-medium text-amber-600">
                            ⚠️ Need at least 2 questions to create an exam
                          </span>
                        )}
                      </div>
                      <div className="flex gap-3">
                        <DialogClose asChild>
                          <Button
                            variant="outline"
                            type="button"
                            className="h-11 bg-transparent px-6"
                          >
                            Cancel
                          </Button>
                        </DialogClose>
                        <Button
                          type="submit"
                          disabled={isSubmitting || values.questionIds.length < 2}
                          className="h-11 min-w-[140px] bg-gradient-to-r from-blue-600 to-indigo-600 px-6 shadow-lg hover:from-blue-700 hover:to-indigo-700"
                        >
                          {isSubmitting ? (
                            <div className="flex items-center gap-2">
                              <Loader2 className="h-4 w-4 animate-spin" />
                              Processing...
                            </div>
                          ) : (
                            <div className="flex items-center gap-2">
                              <CheckCircle2 className="h-4 w-4" />
                              Confirm ({values.questionIds.length})
                            </div>
                          )}
                        </Button>
                      </div>
                    </div>
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

export default React.memo(DialogAddQuestionBankTeacher);
