import PageWrapper from '@/components/PageWrapper/PageWrapper';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import Loading from '@/components/ui/loading';
import cachedKeys from '@/consts/cachedKeys';
import { showError, showSuccess } from '@/helpers/toast';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetDetailUser from '@/services/modules/user/hooks/useGetDetailUser';
import userService from '@/services/modules/user/user.Service';
import { useSave } from '@/stores/useStores';
import { Form, Formik } from 'formik';
import { Edit3Icon, UserPlus } from 'lucide-react';
import type React from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate, useParams } from 'react-router-dom';
import * as Yup from 'yup';
import BasicInfoSection from '../components/BasicInfoSection';
import PersonalInfoSection from '../components/PersonalInfoSection';
import WorkStudyInfoSection from '../components/WorkStudyInfoSection';
import FormSubmitButton from '../form/FormSubmitButton';

// Utility function to convert a URL to a File object
const urlToFile = async (url: string, filename: string): Promise<File> => {
  try {
    const response = await fetch(url);
    const blob = await response.blob();
    return new File([blob], filename, { type: blob.type });
  } catch (error) {
    throw new Error('Failed to convert URL to File');
  }
};

export interface FormDataUser {
  userId?: string;
  userCode: string;
  fullName: string;
  email: string;
  roleId: string | number; // Changed to single value
  dob: Date | undefined | null;
  sex: number;
  phone: string;
  address: string;
  campusId: string;
  departmentId: string;
  positionId: string;
  specializationId: string;
  majorId: string;
  status: number;
  avatar: File | string | null;
}

export const mockDataRoles = [
  { label: 'Student', value: 1 },
  { label: 'Lecturer', value: 2 },
  { label: 'Supervisor', value: 3 },
  { label: 'Admin', value: 4 },
];

export const statusOptions = [
  { label: 'Active', value: 1 },
  { label: 'Inactive', value: 0 },
];

const AddNewUser = () => {
  const { t } = useTranslation('shared');
  const { userId } = useParams();
  const { data: userDetail, isLoading: loadingUserDetail } = useGetDetailUser(userId as string, {
    isTrigger: !!userId,
  });
  const save = useSave();
  const navigate = useNavigate();

  const initialValues: FormDataUser = {
    userCode: userDetail?.userCode ?? '',
    fullName: userDetail?.fullName ?? '',
    email: userDetail?.email ?? '',
    roleId: userDetail?.roleId
      ? Array.isArray(userDetail.roleId)
        ? (userDetail.roleId[0]?.toString() ?? '')
        : userDetail.roleId.toString()
      : '',
    sex: userDetail?.sex ?? 0,
    phone: userDetail?.phone ?? '',
    address: userDetail?.address ?? '',
    campusId: userDetail?.campus ?? '',
    departmentId: userDetail?.department ?? '',
    positionId: userDetail?.position ?? '',
    specializationId: userDetail?.specialization ?? '',
    majorId: userDetail?.major ?? '',
    status: userDetail?.status ?? 1,
    dob: userDetail?.dob ? new Date(userDetail.dob) : null,
    avatar: userDetail?.avatarUrl ?? null,
  };

  const validationSchema = Yup.object({
    userCode: Yup.string()
      .required(t('UserManagement.Validation.UserCodeRequired'))
      .min(3, t('UserManagement.Validation.UserCodeMinLength')),

    fullName: Yup.string()
      .required(t('UserManagement.Validation.FullNameRequired'))
      .min(2, t('UserManagement.Validation.FullNameMinLength')),

    email: Yup.string()
      .email(t('UserManagement.Validation.EmailInvalid'))
      .required(t('UserManagement.Validation.EmailRequired')),

    dob: Yup.date()
      .nullable()
      .test('age', t('UserManagement.Validation.AgeRequirement'), (value) => {
        if (!value) return true;
        const today = new Date();
        const dob = new Date(value);
        const age = today.getFullYear() - dob.getFullYear();
        return age >= 16;
      })
      .required(t('UserManagement.Validation.DobRequired')),

    sex: Yup.string().required(t('UserManagement.Validation.SexRequired')),

    phone: Yup.string()
      .matches(/^[0-9]{10}$/, t('UserManagement.Validation.PhoneInvalid'))
      .required(t('UserManagement.Validation.PhoneRequired')),

    address: Yup.string().required(t('UserManagement.Validation.AddressRequired')),

    roleId: Yup.string()
      .required(t('UserManagement.Validation.RoleRequired'))
      .test('is-valid-role', t('UserManagement.Validation.RoleInvalid'), (value) => {
        const validRoleValues = mockDataRoles.map((role) => role.value.toString());
        return validRoleValues.includes(value?.toString() ?? '');
      }),

    campusId: Yup.string().required(t('UserManagement.Validation.CampusIdRequired')),

    departmentId: Yup.string().when('roleId', {
      is: (roleId: string | number) => ['2', '3', '4'].includes(roleId.toString()),
      then: (schema) => schema.required(t('UserManagement.Validation.DepartmentIdRequired')),
      otherwise: (schema) => schema.notRequired(),
    }),

    positionId: Yup.string().when('roleId', {
      is: (roleId: string | number) => ['2', '3', '4'].includes(roleId.toString()),
      then: (schema) => schema.required(t('UserManagement.Validation.PositionIdRequired')),
      otherwise: (schema) => schema.notRequired(),
    }),

    specializationId: Yup.string().when('roleId', {
      is: (roleId: string | number) => roleId.toString() === '2',
      then: (schema) => schema.required(t('UserManagement.Validation.SpecializationIdRequired')),
      otherwise: (schema) => schema.notRequired(),
    }),

    majorId: Yup.string().when('roleId', {
      is: (roleId: string | number) => roleId.toString() === '1',
      then: (schema) => schema.required(t('UserManagement.Validation.MajorIdRequired')),
      otherwise: (schema) => schema.notRequired(),
    }),

    status: Yup.number()
      .oneOf([0, 1], t('UserManagement.Validation.StatusInvalid'))
      .required(t('UserManagement.Validation.StatusRequired')),

    avatar: Yup.mixed()
      .test('fileType', t('UserManagement.Validation.AvatarInvalid'), (value) => {
        if (!value) return true;
        if (typeof value === 'string') return true;
        if (value instanceof File) {
          return ['image/jpeg', 'image/png', 'image/gif'].includes(value.type);
        }
        return false;
      })
      .test('fileSize', t('UserManagement.Validation.AvatarSize'), (value) => {
        if (!value || typeof value === 'string') return true;
        if (value instanceof File) {
          return value.size <= 5 * 1024 * 1024; // 5MB limit
        }
        return false;
      }),
  });

  const handleSubmit = async (values: FormDataUser, { setSubmitting }: any) => {
    try {
      const formData = new FormData();
      formData.append('userCode', values.userCode);
      formData.append('fullName', values.fullName);
      formData.append('email', values.email);
      formData.append('roleId', values.roleId.toString()); // Send as single value
      formData.append('sex', values.sex.toString());
      formData.append('phone', values.phone);
      formData.append('address', values.address);
      formData.append('campusId', values.campusId);
      formData.append('departmentId', values.departmentId);
      formData.append('positionId', values.positionId);
      formData.append('specializationId', values.specializationId);
      formData.append('majorId', values.majorId);
      formData.append('status', values.status.toString());
      if (values.dob) {
        formData.append('dob', values.dob.toISOString());
      }
      if (values.avatar instanceof File) {
        formData.append('avatar', values.avatar);
      } else if (typeof values.avatar === 'string' && values.avatar) {
        const filename = values.avatar.split('/').pop() ?? 'avatar.jpg';
        const file = await urlToFile(values.avatar, filename);
        formData.append('avatar', file);
      }

      if (userId) {
        formData.append('userId', userId);
        await userService.updateUser(formData);
        setSubmitting(false);
        save(cachedKeys.dataUser, null);
        save(cachedKeys.forceRefetchUser, true);
        navigate(-1);
        showSuccess(t('UserManagement.UpdateSuccess'));
      } else {
        await userService.createUser(formData);
        setSubmitting(false);
        save(cachedKeys.dataUser, null);
        save(cachedKeys.forceRefetchUser, true);
        navigate(-1);
        showSuccess(t('UserManagement.CreateSuccess'));
      }
    } catch (error) {
      showError(error);
    }
  };

  if (loadingUserDetail) {
    return <Loading />;
  }

  return (
    <PageWrapper name={t('UserManagement.AddNewUser')} className="bg-white dark:bg-gray-900">
      <ExamHeader
        title={t('UserManagement.Title')}
        subtitle={t('UserManagement.Subtitle')}
        icon={
          userId ? (
            <Edit3Icon className="text-wh-600 h-6 w-6" />
          ) : (
            <UserPlus className="h-6 w-6 text-green-600" />
          )
        }
        className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
      />
      <div className="mx-auto">
        <Card className="shadow-lg">
          <CardHeader className="space-y-2"></CardHeader>
          <CardContent className="space-y-8">
            <Formik
              initialValues={initialValues}
              validationSchema={validationSchema}
              onSubmit={handleSubmit}
              enableReinitialize
            >
              {({ values, setFieldValue, handleChange, handleBlur, isSubmitting }) => {
                const isStudent = values.roleId.toString() === '1';
                const isLecturer = values.roleId.toString() === '2';
                const isAdminOrSupervisor = ['3', '4'].includes(values.roleId.toString());

                const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
                  const file = event.target.files?.[0];
                  if (file) {
                    setFieldValue('avatar', file);
                  }
                };

                return (
                  <Form className="space-y-8">
                    <div className="space-y-4">
                      <label className="block text-sm font-semibold text-gray-700 dark:text-gray-300">
                        Avatar
                      </label>
                      <div className="flex items-center space-x-6">
                        <div className="relative">
                          <div className="h-24 w-24 overflow-hidden rounded-full border-4 border-gray-200 bg-gray-50 shadow-lg dark:border-gray-600 dark:bg-gray-800">
                            {values.avatar && typeof values.avatar === 'string' ? (
                              <img
                                src={values.avatar ?? '/placeholder.svg'}
                                alt="User avatar preview"
                                className="h-full w-full object-cover"
                              />
                            ) : values.avatar instanceof File ? (
                              <img
                                src={URL.createObjectURL(values.avatar) ?? '/placeholder.svg'}
                                alt="User avatar preview"
                                className="h-full w-full object-cover"
                              />
                            ) : (
                              <div className="flex h-full w-full items-center justify-center bg-gradient-to-br from-blue-100 to-purple-100 dark:from-blue-900 dark:to-purple-900">
                                <UserPlus className="h-8 w-8 text-gray-400 dark:text-gray-500" />
                              </div>
                            )}
                          </div>
                          <label
                            htmlFor="avatar"
                            className="group absolute inset-0 flex cursor-pointer items-center justify-center rounded-full bg-black bg-opacity-0 transition-all duration-200 hover:bg-opacity-40"
                          >
                            <div className="opacity-0 transition-opacity duration-200 group-hover:opacity-100">
                              <Edit3Icon className="h-5 w-5 text-white" />
                            </div>
                          </label>
                        </div>
                        <div className="flex-1 space-y-3">
                          <div className="flex items-center space-x-3">
                            <label
                              htmlFor="avatar"
                              className="inline-flex cursor-pointer items-center rounded-lg border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 shadow-sm transition-colors duration-200 hover:bg-gray-50 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-300 dark:hover:bg-gray-700"
                            >
                              <Edit3Icon className="mr-2 h-4 w-4" />
                              {values.avatar ? 'Change Avatar' : 'Upload Avatar'}
                            </label>
                            {values.avatar && (
                              <button
                                type="button"
                                onClick={() => setFieldValue('avatar', null)}
                                className="inline-flex items-center px-3 py-2 text-sm font-medium text-red-600 transition-colors duration-200 hover:text-red-700 dark:text-red-400 dark:hover:text-red-300"
                              >
                                Remove
                              </button>
                            )}
                          </div>
                          <div className="space-y-1 text-xs text-gray-500 dark:text-gray-400">
                            <p>JPG, PNG or GIF (max. 5MB)</p>
                            <p>Recommended: Square image, at least 200x200px</p>
                          </div>
                        </div>
                      </div>
                      <Input
                        type="file"
                        id="avatar"
                        accept="image/jpeg,image/png,image/gif"
                        onChange={handleFileChange}
                        className="hidden"
                      />
                    </div>
                    <BasicInfoSection
                      values={values}
                      handleChange={handleChange}
                      handleBlur={handleBlur}
                      mockDataRoles={mockDataRoles}
                      userId={userId}
                    />
                    <PersonalInfoSection
                      values={values}
                      handleChange={handleChange}
                      handleBlur={handleBlur}
                    />
                    <WorkStudyInfoSection
                      userId={userId}
                      values={values}
                      isStudent={isStudent}
                      isLecturer={isLecturer}
                      isAdminOrSupervisor={isAdminOrSupervisor}
                    />
                    <FormSubmitButton isSubmitting={isSubmitting} userId={userId} />
                  </Form>
                );
              }}
            </Formik>
          </CardContent>
        </Card>
      </div>
    </PageWrapper>
  );
};

export default AddNewUser;
