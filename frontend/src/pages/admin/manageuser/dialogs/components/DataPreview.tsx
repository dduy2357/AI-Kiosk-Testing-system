import { Card, CardContent } from '@/components/ui/card';
import { UserData, ValidationError } from '../DialogPreCheckImportUser';
import { AlertTriangle, CheckCircle, FileSpreadsheet, Users } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import React from 'react';
import { useTranslation } from 'react-i18next';

const DataPreview: React.FC<{
  userData: UserData[];
  validationErrors: ValidationError[];
  importReport: { successCount: number; failedCount: number; errors: ValidationError[] } | null;
  downloadErrorReport: () => void;
}> = ({ userData, validationErrors, importReport, downloadErrorReport }) => {
  const { t } = useTranslation('shared');

  return (
    <Card className="m-8 w-full border-0 shadow-lg">
      <CardContent className="p-8">
        <div className="mb-6 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-100">
              <Users className="h-6 w-6 text-blue-600" />
            </div>
            <div>
              <h3 className="text-xl font-bold text-gray-900">{t('UserManagement.PreviewData')}</h3>
              <p className="text-gray-600">{t('UserManagement.PreviewDataDescription')}</p>
            </div>
          </div>
          {userData.length > 0 && (
            <Badge variant="secondary" className="bg-blue-100 px-4 py-2 text-blue-700">
              <Users className="mr-1 h-4 w-4" />
              {userData.length} {t('UserManagement.TotalUsers')}
            </Badge>
          )}
        </div>
        {validationErrors.length > 0 && (
          <Alert variant="destructive" className="mb-6 border-red-200 bg-red-50">
            <AlertTriangle className="h-4 w-4" />
            <AlertDescription className="text-red-800">
              <p className="font-semibold">
                {t('UserManagement.FoundErrors', { count: validationErrors.length })}
              </p>
              <ul className="mt-2 list-disc pl-5">
                {validationErrors.map((err, index) => (
                  <li key={index} className="text-sm">
                    Row {err.row}: {err.field} - {err.message}
                  </li>
                ))}
              </ul>
              <Button
                variant="link"
                onClick={downloadErrorReport}
                className="mt-2 text-red-800 underline"
              >
                {t('UserManagement.DownloadErrorReport')}
              </Button>
              <p className="mt-2 text-sm">{t('UserManagement.CorrectErrors')}</p>
            </AlertDescription>
          </Alert>
        )}
        {userData.length > 0 ? (
          <div
            className="w-full overflow-x-auto rounded-xl border border-gray-200"
            style={{ minWidth: '800px', maxWidth: '100%' }}
          >
            <Table className="w-full min-w-full">
              <TableHeader>
                <TableRow className="bg-gray-50">
                  <TableHead className="whitespace-nowrap py-2 text-sm font-semibold text-gray-900">
                    Row
                  </TableHead>
                  {[
                    'Họ tên',
                    'Mã người dùng',
                    'Số điện thoại',
                    'Email',
                    'Giới tính',
                    'Trạng thái',
                    'Ngày sinh',
                    'Địa chỉ',
                    'Campus ID',
                    'Department ID',
                    'Position ID',
                    'Major ID',
                    'Specialization ID',
                    'Role ID',
                  ].map((header, index) => (
                    <TableHead
                      key={index}
                      className="whitespace-nowrap py-2 text-sm font-semibold text-gray-900"
                    >
                      {header}
                    </TableHead>
                  ))}
                </TableRow>
              </TableHeader>
              <TableBody>
                {userData.map((user, index) => {
                  const rowErrors = validationErrors.filter((err) => err.row === index + 2);
                  return (
                    <TableRow
                      key={index}
                      className={`transition-colors ${rowErrors.length > 0 ? 'bg-red-50 hover:bg-red-100' : 'hover:bg-gray-50'}`}
                    >
                      <TableCell className="py-2 text-sm font-medium">{index + 2}</TableCell>
                      <TableCell className="min-w-[100px] max-w-[150px] truncate py-2 text-sm font-medium">
                        {user.FullName}
                        {rowErrors.some((err) => err.field === 'FullName') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'FullName')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[80px] max-w-[120px] truncate py-2 text-sm">
                        {user.UserCode}
                        {rowErrors.some((err) => err.field === 'UserCode') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'UserCode')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[80px] max-w-[120px] truncate py-2 text-sm">
                        {user.Phone}
                        {rowErrors.some((err) => err.field === 'Phone') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'Phone')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[120px] max-w-[180px] truncate py-2 text-sm text-blue-600">
                        {user.Email}
                        {rowErrors.some((err) => err.field === 'Email') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'Email')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] py-2">
                        <Badge
                          variant={user.Sex === 'Male' ? 'default' : 'secondary'}
                          className="text-xs"
                        >
                          {user.Sex}
                        </Badge>
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] py-2">
                        <Badge
                          variant={user.Status === 'Active' ? 'default' : 'secondary'}
                          className="text-xs"
                        >
                          {user.Status}
                        </Badge>
                      </TableCell>
                      <TableCell className="min-w-[80px] max-w-[120px] truncate py-2 text-sm text-gray-600">
                        {user.Dob}
                        {rowErrors.some((err) => err.field === 'Dob') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'Dob')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[100px] max-w-[150px] truncate py-2 text-sm">
                        {user.Address}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] truncate py-2 text-sm">
                        {user.CampusId}
                        {rowErrors.some((err) => err.field === 'CampusId') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'CampusId')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] truncate py-2 text-sm">
                        {user.DepartmentId}
                        {rowErrors.some((err) => err.field === 'DepartmentId') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'DepartmentId')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] truncate py-2 text-sm">
                        {user.PositionId}
                        {rowErrors.some((err) => err.field === 'PositionId') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'PositionId')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] truncate py-2 text-sm">
                        {user.MajorId}
                        {rowErrors.some((err) => err.field === 'MajorId') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'MajorId')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] truncate py-2 text-sm">
                        {user.SpecializationId}
                        {rowErrors.some((err) => err.field === 'SpecializationId') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'SpecializationId')?.message}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="min-w-[60px] max-w-[100px] truncate py-2 text-sm">
                        {user.RoleId}
                        {rowErrors.some((err) => err.field === 'RoleId') && (
                          <span className="ml-2 text-xs text-red-600">
                            {rowErrors.find((err) => err.field === 'RoleId')?.message}
                          </span>
                        )}
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="py-12 text-center">
            <div className="mx-auto mb-4 flex h-20 w-20 items-center justify-center rounded-2xl bg-gray-100">
              <FileSpreadsheet className="h-10 w-10 text-gray-400" />
            </div>
            <p className="text-lg text-gray-600">{t('NoDataFound')}</p>
            <p className="mt-1 text-sm text-gray-500">
              {t('UserManagement.PreviewDataDescription')}
            </p>
          </div>
        )}
        {importReport && (
          <Alert variant="default" className="mt-6 border-emerald-200 bg-emerald-50">
            <CheckCircle className="h-5 w-5 text-emerald-600" />
            <AlertDescription className="text-emerald-800">
              Import completed: {importReport.successCount} users successfully created,{' '}
              {importReport.failedCount} failed.
              {importReport.failedCount > 0 && (
                <Button
                  variant="link"
                  onClick={downloadErrorReport}
                  className="ml-2 text-emerald-800 underline"
                >
                  {t('UserManagement.DownloadErrorReport')}
                </Button>
              )}
            </AlertDescription>
          </Alert>
        )}
      </CardContent>
    </Card>
  );
};

export default React.memo(DataPreview);
