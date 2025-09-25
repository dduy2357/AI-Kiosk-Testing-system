import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { getStudentInitials } from '@/utils/exam.utils';
import { User, GraduationCap, BadgeIcon as IdCard } from 'lucide-react';
import { useTranslation } from 'react-i18next';

interface StudentInfoCardProps {
  studentName?: string;
  studentCode?: string;
  className?: string;
}

export default function StudentInfoCard({
  studentName,
  studentCode,
  className = '',
}: Readonly<StudentInfoCardProps>) {
  const { t } = useTranslation('shared');
  const initials = getStudentInitials(studentName ?? '');
  const displayName = studentName ?? t('StudentExamResultDetail.noName');
  const displayCode = studentCode ?? t('StudentExamResultDetail.na');

  return (
    <Card
      className={`group relative overflow-hidden transition-all duration-300 hover:shadow-lg ${className}`}
    >
      {/* Background gradient overlay */}
      <div className="absolute inset-0 bg-gradient-to-br from-blue-50/50 via-transparent to-green-50/50 opacity-0 transition-opacity duration-300 group-hover:opacity-100" />

      <CardHeader className="relative pb-3">
        <CardTitle className="flex items-center gap-2 text-gray-800">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-blue-500 to-blue-600 shadow-sm">
            <User className="h-4 w-4 text-white" />
          </div>
          <span className="bg-gradient-to-r from-gray-800 to-gray-600 bg-clip-text text-transparent">
            {t('StudentExamResultDetail.studentInfo')}
          </span>
        </CardTitle>
      </CardHeader>

      <CardContent className="relative space-y-6 pt-2">
        {/* Student Avatar and Basic Info */}
        <div className="flex items-center gap-4">
          <div className="relative">
            <Avatar className="h-16 w-16 border-2 border-white shadow-lg ring-2 ring-blue-100 transition-all duration-300 group-hover:ring-blue-200">
              <AvatarImage src="/placeholder.svg" className="object-cover" />
              <AvatarFallback className="bg-gradient-to-br from-blue-500 to-blue-600 text-lg font-semibold text-white">
                {initials}
              </AvatarFallback>
            </Avatar>
            {/* Online indicator */}
            <div className="absolute -bottom-1 -right-1 h-5 w-5 rounded-full border-2 border-white bg-green-500 shadow-sm" />
          </div>

          <div className="flex-1 space-y-1">
            <div className="flex items-center gap-2">
              <h3 className="text-lg font-bold text-gray-900 transition-colors duration-200 group-hover:text-blue-700">
                {displayName}
              </h3>
              {studentName && (
                <Badge variant="secondary" className="bg-blue-100 text-blue-700 hover:bg-blue-200">
                  <GraduationCap className="mr-1 h-3 w-3" />
                  {t('StudentExamResultDetail.student')}
                </Badge>
              )}
            </div>

            <div className="flex items-center gap-1.5 text-sm text-gray-600">
              <IdCard className="h-4 w-4 text-gray-400" />
              <span className="font-medium">{t('StudentExamResultDetail.studentCodeLabel')}:</span>
              <span className="font-mono text-gray-800">{displayCode}</span>
            </div>
          </div>
        </div>

        {/* Additional Info Cards */}
        <div className="grid grid-cols-1 gap-3">
          {/* Status Card */}
          <div className="rounded-lg border border-gray-100 bg-gradient-to-r from-green-50 to-emerald-50 p-3 transition-all duration-200 hover:shadow-sm">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2">
                <div className="h-2 w-2 rounded-full bg-green-500" />
                <span className="text-sm font-medium text-gray-700">
                  {t('StudentExamResultDetail.status')}
                </span>
              </div>
              <Badge variant="secondary" className="bg-green-100 text-green-700">
                {t('StudentExamResultDetail.completed')}
              </Badge>
            </div>
          </div>
        </div>

        {/* Decorative bottom border */}
        <div className="absolute bottom-0 left-4 right-4 h-0.5 bg-gradient-to-r from-transparent via-blue-200 to-transparent opacity-0 transition-opacity duration-300 group-hover:opacity-100" />
      </CardContent>
    </Card>
  );
}
