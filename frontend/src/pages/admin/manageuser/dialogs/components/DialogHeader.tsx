import { Badge } from '@/components/ui/badge';
import { DialogDescription, DialogTitle } from '@/components/ui/dialog';
import { CheckCircle, Upload } from 'lucide-react';
import React from 'react';
import { useTranslation } from 'react-i18next';

const DialogHeader: React.FC<{ selectedFile: File | null }> = ({ selectedFile }) => {
  const { t } = useTranslation('shared');

  return (
    <div className="sticky top-0 z-10 border-b bg-white/95 px-8 py-6 shadow-sm backdrop-blur-sm">
      <div className="flex items-center gap-4">
        <div className="flex h-16 w-16 items-center justify-center rounded-2xl bg-gradient-to-br from-emerald-500 to-emerald-600 shadow-lg">
          <Upload className="h-8 w-8 text-white" />
        </div>
        <div className="flex-1">
          <DialogTitle className="mb-1 text-2xl font-bold text-gray-900">
            {t('UserManagement.ImportUserData')}
          </DialogTitle>
          <DialogDescription className="text-base text-gray-600">
            {t('UserManagement.ImportUserDataDescription')}
          </DialogDescription>
        </div>
        {selectedFile && (
          <Badge variant="secondary" className="bg-emerald-100 px-3 py-1 text-emerald-700">
            <CheckCircle className="mr-1 h-4 w-4" />
            {t('UserManagement.FileReady', { fileName: selectedFile.name })}
          </Badge>
        )}
      </div>
    </div>
  );
};

export default React.memo(DialogHeader);
