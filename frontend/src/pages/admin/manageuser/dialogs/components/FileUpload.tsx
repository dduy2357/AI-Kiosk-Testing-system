import { Alert, AlertDescription } from '@/components/ui/alert';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { AlertTriangle, CheckCircle, FileSpreadsheet } from 'lucide-react';
import React from 'react';
import { useTranslation } from 'react-i18next';

const FileUpload: React.FC<{
  selectedFile: File | null;
  error: string | null;
  handleFileSelect: (event: React.ChangeEvent<HTMLInputElement>) => void;
}> = ({ selectedFile, error, handleFileSelect }) => {
  const { t } = useTranslation('shared');
  return (
    <Card className="border-0 shadow-lg">
      <CardContent className="p-8">
        {error && (
          <Alert variant="destructive" className="mb-6 border-red-200 bg-red-50">
            <AlertTriangle className="h-4 w-4" />
            <AlertDescription className="text-red-800">{error}</AlertDescription>
          </Alert>
        )}
        <div className="mb-6">
          <h3 className="mb-2 text-xl font-bold text-gray-900">
            {t('UserManagement.FileUploadTitle')}
          </h3>
          <p className="text-gray-600">{t('UserManagement.FileUploadDescription')}</p>
        </div>
        <div className="group relative rounded-2xl border-2 border-dashed border-gray-300 p-12 text-center transition-all duration-300 hover:border-emerald-400 hover:bg-emerald-50/50">
          <input
            type="file"
            accept=".xlsx,.xls,.csv"
            onChange={handleFileSelect}
            className="hidden"
            id="file-upload"
          />
          <label htmlFor="file-upload" className="cursor-pointer">
            {selectedFile ? (
              <div className="flex flex-col items-center gap-4">
                <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-emerald-100 shadow-lg">
                  <FileSpreadsheet className="h-10 w-10 text-emerald-600" />
                </div>
                <div className="text-center">
                  <p className="text-lg font-semibold text-gray-900">{selectedFile.name}</p>
                  <p className="font-medium text-emerald-600">
                    {(selectedFile.size / 1024 / 1024).toFixed(2)} MB
                  </p>
                  <Badge variant="secondary" className="mt-2 bg-emerald-100 text-emerald-700">
                    <CheckCircle className="mr-1 h-3 w-3" />
                    {t('UserManagement.FileReady')}
                  </Badge>
                </div>
              </div>
            ) : (
              <div className="flex flex-col items-center gap-4">
                <div className="flex h-20 w-20 items-center justify-center rounded-2xl bg-gray-100 transition-colors duration-300 group-hover:bg-emerald-100">
                  <FileSpreadsheet className="h-10 w-10 text-gray-400 transition-colors duration-300 group-hover:text-emerald-600" />
                </div>
                <div>
                  <p className="mb-1 text-lg font-semibold text-gray-900">
                    {t('UserManagement.SelectFile')}
                  </p>
                  <p className="text-sm text-gray-500">{t('UserManagement.FileTypes')}</p>
                </div>
              </div>
            )}
          </label>
        </div>
      </CardContent>
    </Card>
  );
};

export default React.memo(FileUpload);
