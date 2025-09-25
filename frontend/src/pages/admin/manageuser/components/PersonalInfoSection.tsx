import DateTimePickerField from '@/components/customFieldsFormik/DateTimePickerField';
import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import RadioField from '@/components/customFieldsFormik/RadioField';
import { useTranslation } from 'react-i18next';

interface PersonalInfoProps {
  values: any;
  handleChange: (e: React.ChangeEvent<any>) => void;
  handleBlur: (e: React.FocusEvent<any>) => void;
}

const PersonalInfoSection = ({ values, handleChange, handleBlur }: PersonalInfoProps) => {
  const { t } = useTranslation('shared');
  return (
    <div className="space-y-6">
      <h3 className="border-b pb-2 text-lg font-semibold text-gray-900">
        {t('UserManagement.PersonalInfo')}
      </h3>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <FormikField
            component={DateTimePickerField}
            name="dob"
            label={t('UserManagement.Dob')}
            placeholder={t('UserManagement.DobPlaceholder')}
            required
            hideTimePicker
          />
        </div>
        <div className="space-y-3">
          <FormikField
            component={RadioField}
            name="sex"
            label={t('UserManagement.Sex')}
            options={[
              { label: t('UserManagement.Male'), value: 0 },
              { label: t('UserManagement.Female'), value: 1 },
              { label: t('UserManagement.Other'), value: 2 },
            ]}
            className="flex space-x-6"
            required
          />
        </div>
      </div>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <FormikField
            component={InputField}
            name="phone"
            label={t('UserManagement.Phone')}
            placeholder={t('UserManagement.PhonePlaceholder')}
            value={values.phone}
            onChange={handleChange}
            onBlur={handleBlur}
            required
            isNumberic
          />
        </div>
        <div className="space-y-2">
          <FormikField
            component={InputField}
            name="address"
            label={t('UserManagement.Address')}
            placeholder={t('UserManagement.AddressPlaceholder')}
            value={values.address}
            onChange={handleChange}
            onBlur={handleBlur}
            required
          />
        </div>
      </div>
    </div>
  );
};

export default PersonalInfoSection;
