import { Button } from '@/components/ui/button';
import { Loader2, UserPlus } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface FormSubmitButtonProps {
  isSubmitting: boolean;
  userId?: string;
}

const FormSubmitButton = ({ isSubmitting, userId }: FormSubmitButtonProps) => {
  const { t } = useTranslation('shared');
  return (
    <div className="flex justify-end border-t pt-6">
      <Button
        type="submit"
        disabled={isSubmitting}
        className="h-12 w-full bg-gradient-to-r from-blue-600 to-purple-600 text-white transition-all duration-200 hover:from-blue-700 hover:to-purple-700 hover:shadow-lg disabled:opacity-50"
      >
        {isSubmitting ? (
          <div className="flex items-center gap-2">
            <Loader2 className="h-4 w-4 animate-spin" />
            {t('Processing')}
          </div>
        ) : (
          <div className="flex items-center gap-2">
            <UserPlus className="h-4 w-4" />
            {userId ? t('UserManagement.EditUser') : t('UserManagement.AddNewUser')}
          </div>
        )}
      </Button>
    </div>
  );
};

export default FormSubmitButton;
