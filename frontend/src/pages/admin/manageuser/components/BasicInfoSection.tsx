import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { useTranslation } from 'react-i18next';

interface BasicInfoProps {
  values: any;
  handleChange: (e: React.ChangeEvent<any>) => void;
  handleBlur: (e: React.FocusEvent<any>) => void;
  mockDataRoles: { label: string; value: number }[];
  userId?: string;
}

const BasicInfoSection = ({
  values,
  handleChange,
  handleBlur,
  mockDataRoles,
  userId,
}: BasicInfoProps) => {
  const { t } = useTranslation('shared');

  return (
    <div className="space-y-6">
      <h3 className="border-b pb-2 text-lg font-semibold text-gray-900">
        {t('UserManagement.BasicInfo')}
      </h3>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <FormikField
            component={InputField}
            name="userCode"
            label={t('UserManagement.UserCode')}
            placeholder={t('UserManagement.UserCodePlaceholder')}
            value={values.userCode}
            onChange={handleChange}
            onBlur={handleBlur}
            required
          />
        </div>
        <div className="space-y-2">
          <FormikField
            component={InputField}
            name="fullName"
            label={t('UserManagement.FullName')}
            placeholder={t('UserManagement.FullNamePlaceholder')}
            value={values.fullName}
            onChange={handleChange}
            onBlur={handleBlur}
            required
          />
        </div>
      </div>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <FormikField
            component={InputField}
            name="email"
            label={t('UserManagement.Email')}
            placeholder={t('UserManagement.EmailPlaceholder')}
            value={values.email}
            onChange={handleChange}
            onBlur={handleBlur}
            required
            readOnly={!!userId}
          />
        </div>
        <div className="space-y-2">
          <FormikField
            component={SelectField}
            name="roleId"
            label={t('UserManagement.Role')}
            placeholder={t('UserManagement.RolePlaceholder')}
            options={mockDataRoles}
            required
            shouldHideSearch
          />
        </div>
      </div>
    </div>
  );
};

export default BasicInfoSection;
