import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Download } from 'lucide-react';
import { REQUIRED_FIELDS } from '../DialogPreCheckImportUser';
import { saveAs } from 'file-saver';
import * as XLSX from 'xlsx';
import React from 'react';
import { useTranslation } from 'react-i18next';

const TemplateDownload: React.FC = () => {
  const { t } = useTranslation('shared');
  return (
    <Card className="border-0 bg-gradient-to-r from-green-50 to-emerald-50 shadow-lg">
      <CardContent className="p-8">
        <div className="mb-4 flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-emerald-100">
            <Download className="h-6 w-6 text-emerald-600" />
          </div>
          <div>
            <h3 className="text-xl font-bold text-gray-900">{t('UserManagement.Title')}</h3>
            <p className="mt-1 text-gray-600">{t('UserManagement.TemplateDescription')}</p>
          </div>
        </div>
        <Button
          variant="outline"
          className="border-emerald-200 bg-white text-emerald-700 shadow-sm transition-all duration-200 hover:bg-emerald-50 hover:text-emerald-800 hover:shadow-md"
          onClick={() => {
            const templateData = [
              REQUIRED_FIELDS.concat([
                'Phone',
                'Sex',
                'Status',
                'Dob',
                'Address',
                'DepartmentId',
                'PositionId',
                'MajorId',
                'SpecializationId',
                'RoleId',
              ]),
            ];
            const ws = XLSX.utils.json_to_sheet(templateData);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Template');
            const wbout = XLSX.write(wb, { bookType: 'xlsx', type: 'array' });
            saveAs(
              new Blob([wbout], { type: 'application/octet-stream' }),
              'user_import_template.xlsx',
            );
          }}
        >
          <Download className="mr-2 h-4 w-4" />
          {t('UserManagement.DownloadTemplate')}
        </Button>
      </CardContent>
    </Card>
  );
};

export default React.memo(TemplateDownload);
