import { CheckCircle, Upload } from 'lucide-react';
import { UserData, ValidationError } from '../DialogPreCheckImportUser';
import { Button } from '@/components/ui/button';
import React from 'react';
import { useTranslation } from 'react-i18next';

const DialogFooter: React.FC<{
  selectedFile: File | null;
  userData: UserData[];
  validationErrors: ValidationError[];
  isSubmitting: boolean;
  toggle: () => void;
  handleSubmit: () => void;
}> = ({ selectedFile, userData, validationErrors, isSubmitting, toggle, handleSubmit }) => {
  const { t } = useTranslation('shared');

  return (
    <div className="sticky bottom-0 border-t bg-white/95 px-8 py-6 shadow-lg backdrop-blur-sm">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2 text-sm text-gray-600">
          {(selectedFile || userData.length > 0) && (
            <>
              <CheckCircle className="h-4 w-4 text-emerald-600" />
              <span>
                {selectedFile
                  ? t('UserManagement.FileReady', { fileName: selectedFile.name })
                  : t('UserManagement.DataReady')}
              </span>
            </>
          )}
        </div>
        <div className="flex gap-3">
          <Button variant="ghost" onClick={toggle} className="hover:bg-gray-100">
            {t('Close')}
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={isSubmitting || !selectedFile || validationErrors.length > 0}
            className="bg-gradient-to-r from-emerald-600 to-emerald-700 px-8 shadow-lg transition-all duration-200 hover:from-emerald-700 hover:to-emerald-800 hover:shadow-xl"
          >
            {isSubmitting ? (
              <>
                <div className="mr-2 h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
                {t('Processing')}
              </>
            ) : (
              <>
                <Upload className="mr-2 h-4 w-4" />
                {t('UserManagement.ImportUserData')}
              </>
            )}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default React.memo(DialogFooter);
