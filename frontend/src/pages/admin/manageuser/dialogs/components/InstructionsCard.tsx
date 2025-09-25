import { Alert, AlertDescription } from '@/components/ui/alert';
import { Card, CardContent } from '@/components/ui/card';
import { AlertTriangle, CheckCircle, FileSpreadsheet, Info } from 'lucide-react';
import React from 'react';
import { useTranslation } from 'react-i18next';

const InstructionsCard: React.FC = () => {
  const { t } = useTranslation('shared');
  return (
    <Card className="border-0 bg-gradient-to-r from-blue-50 to-indigo-50 shadow-lg">
      <CardContent className="p-8">
        <div className="mb-6 flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-blue-100">
            <Info className="h-5 w-5 text-blue-600" />
          </div>
          <h3 className="text-xl font-bold text-gray-900">
            {t('UserManagement.ImportInstructions')}
          </h3>
        </div>
        <div className="grid gap-8 lg:grid-cols-2">
          <div className="space-y-4">
            <h4 className="flex items-center gap-2 text-lg font-semibold text-gray-900">
              <FileSpreadsheet className="h-5 w-5 text-emerald-600" />
              {t('UserManagement.SupportedFileFormats')}
            </h4>
            <div className="space-y-3">
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-emerald-500"></div>
                <span className="text-gray-700">{t('UserManagement.Excel')}</span>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-emerald-500"></div>
                <span className="text-gray-700">{t('UserManagement.CSV')}</span>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-amber-500"></div>
                <span className="text-gray-700">
                  {t('UserManagement.MaxFileSize')}: <strong>10MB</strong>
                </span>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-amber-500"></div>
                <span className="text-gray-700">
                  {t('UserManagement.MaxRecords')}: <strong>1000 bản ghi</strong>
                </span>
              </div>
            </div>
          </div>
          <div className="space-y-4">
            <h4 className="flex items-center gap-2 text-lg font-semibold text-gray-900">
              <CheckCircle className="h-5 w-5 text-blue-600" />
              {t('UserManagement.DataRequirements')}
            </h4>
            <div className="space-y-3">
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-red-500"></div>
                <span className="text-gray-700">
                  {t('UserManagement.RequiredFields')}: FullName, UserCode, Email, CampusId, RoleId
                </span>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-red-500"></div>
                <span className="text-gray-700">{t('UserManagement.UniqueEmail')}</span>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-blue-500"></div>
                <span className="text-gray-700">
                  {t('UserManagement.DateFormat')}:{' '}
                  <code className="rounded bg-gray-100 px-2 py-1">YYYY-MM-DD or DD/MM/YYYY</code>
                </span>
              </div>
              <div className="flex items-center gap-3 rounded-lg bg-white p-3 shadow-sm">
                <div className="h-2 w-2 rounded-full bg-purple-500"></div>
                <span className="text-gray-700">
                  {t('UserManagement.Roles')}: {t('UserManagement.Student')} (1),{' '}
                  {t('UserManagement.Lecturer')} (2), {t('UserManagement.Supervisor')} (3),{' '}
                  {t('UserManagement.Admin')} (4)
                </span>
              </div>
            </div>
          </div>
        </div>
        <Alert className="mt-6 border-amber-200 bg-gradient-to-r from-amber-50 to-orange-50">
          <AlertTriangle className="h-5 w-5 text-amber-600" />
          <AlertDescription className="text-amber-800">
            <strong className="text-amber-900">{t('UserManagement.ImportantNote')}</strong>
            <br />
            {t('UserManagement.TemplateInstructions')}
          </AlertDescription>
        </Alert>
      </CardContent>
    </Card>
  );
};

export default React.memo(InstructionsCard);
