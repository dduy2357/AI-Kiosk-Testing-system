import { Button } from '@/components/ui/button';
import { Loader2, Download, RefreshCw } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface StudentInfo {
  name: string;
  studentId: string;
}

interface ExamHeaderProps {
  studentInfo: StudentInfo;
  onExport: () => void;
  isExporting: boolean;
}

export function ExamHeader({ studentInfo, onExport, isExporting }: Readonly<ExamHeaderProps>) {
  const { t } = useTranslation('shared');

  return (
    <div className="border-b border-gray-200 bg-white p-4">
      <div className="flex items-center justify-between">
        <div className="flex items-center space-x-4">
          <div>
            <h1 className="text-xl font-semibold text-gray-900">{studentInfo.name}</h1>
            <p className="text-sm text-gray-500">{studentInfo.studentId}</p>
          </div>
        </div>
        <div className="flex items-center space-x-2">
          <Button variant="outline" size="sm" onClick={onExport} disabled={isExporting}>
            {isExporting ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Download className="mr-2 h-4 w-4" />
            )}
            {t('ExamActivityLog.exportReport')}
          </Button>
          <Button variant="outline" size="sm" onClick={() => window.location.reload()}>
            <RefreshCw className="mr-2 h-4 w-4" />
            {t('ExamActivityLog.refresh')}
          </Button>
        </div>
      </div>
    </div>
  );
}
