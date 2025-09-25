import { useMemo, useState, useEffect } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { BookOpen } from 'lucide-react';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import { ISubjectRequest } from '@/services/modules/subject/interfaces/subject.interface';
import { useGet } from '@/stores/useStores';
import useGetAllSubjectV2 from '@/services/modules/subject/hooks/useGetAllSubjectV2';
import { useTranslation } from 'react-i18next';

interface AdditionalInfoProps {
  difficulty: number;
  onDifficultyChange: (difficulty: number) => void;
  selectedSubjectName: string;
  subject: string;
  onSubjectChange: (subject: string) => void;
  explanation: string;
  onExplanationChange: (explanation: string) => void;
  tag: string;
  onTagChange: (tag: string) => void;
  isSubjectDisabled: boolean;
}

export function AdditionalInfoSection({
  difficulty,
  onDifficultyChange,
  selectedSubjectName,
  subject,
  onSubjectChange,
  explanation,
  onExplanationChange,
  tag,
  onTagChange,
  isSubjectDisabled,
}: Readonly<AdditionalInfoProps>) {
  const defaultData = useGet('dataSubject');
  const { t } = useTranslation('shared');
  const cachedFiltersSubject = useGet('cachesListSubject');
  const [isTrigger] = useState(Boolean(!defaultData));
  const { filters } = useFiltersHandler({
    pageSize: cachedFiltersSubject?.pageSize ?? 50,
    currentPage: cachedFiltersSubject?.currentPage ?? 1,
    textSearch: cachedFiltersSubject?.textSearch ?? '',
  });
  const stableFilters = useMemo(() => filters as ISubjectRequest, [filters]);
  const { data: dataSubjects } = useGetAllSubjectV2(stableFilters, {
    isTrigger: isTrigger,
  });

  // Tìm subjectId tương ứng với selectedSubjectName
  useEffect(() => {
    if (selectedSubjectName && dataSubjects) {
      const matchedSubject = dataSubjects.find((subj) => subj.subjectName === selectedSubjectName);
      if (matchedSubject && matchedSubject.subjectId !== subject) {
        onSubjectChange(matchedSubject.subjectId);
      }
    }
  }, [selectedSubjectName, dataSubjects, subject, onSubjectChange]);

  return (
    <Card>
      <CardHeader>
        <CardTitle className="text-lg font-medium">
          {t('AddQuestion.AdditionalInformation')}
        </CardTitle>
        <p className="text-sm text-gray-500">{t('AddQuestion.AdditionalInformationDescription')}</p>
      </CardHeader>
      <CardContent className="space-y-6">
        <div className="grid grid-cols-3 gap-4">
          <div>
            <label className="mb-2 block text-sm font-medium text-gray-700">
              {t('AddQuestion.Difficulty')}
            </label>
            <Select
              value={difficulty.toString()}
              onValueChange={(val) => onDifficultyChange(Number(val))}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="1">
                  <div className="flex items-center space-x-2">
                    <div className="h-3 w-3 rounded-full bg-green-500"></div>
                    <span>{t('AddQuestion.Easy')}</span>
                  </div>
                </SelectItem>
                <SelectItem value="2">
                  <div className="flex items-center space-x-2">
                    <div className="h-3 w-3 rounded-full bg-yellow-500"></div>
                    <span>{t('AddQuestion.Medium')}</span>
                  </div>
                </SelectItem>
                <SelectItem value="3">
                  <div className="flex items-center space-x-2">
                    <div className="h-3 w-3 rounded-full bg-red-500"></div>
                    <span>{t('AddQuestion.Hard')}</span>
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div>
            <label className="mb-2 block text-sm font-medium text-gray-700">
              {t('AddQuestion.Subject')}
            </label>
            <Select
              value={subject}
              onValueChange={onSubjectChange}
              disabled={isSubjectDisabled || selectedSubjectName !== ''}
            >
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {dataSubjects?.map((subject) => (
                  <SelectItem value={subject?.subjectId} key={subject.subjectId}>
                    <div className="flex items-center space-x-2">
                      <BookOpen className="h-4 w-4" />
                      <span>{subject?.subjectName}</span>
                    </div>
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        </div>

        <div>
          <label className="mb-2 block text-sm font-medium text-gray-700">
            {t('AddQuestion.Tags')}
          </label>
          <div className="flex space-x-2">
            <Input
              placeholder={t('AddQuestion.EnterTag')}
              value={tag}
              onChange={(e) => onTagChange(e.target.value)}
            />
          </div>
        </div>

        <div>
          <label className="mb-2 block text-sm font-medium text-gray-700">
            {t('AddQuestion.AnswerExplanation')}
          </label>
          <textarea
            placeholder={t('AddQuestion.EnterAnswerExplanation')}
            value={explanation}
            onChange={(e) => onExplanationChange(e.target.value)}
            className="min-h-[100px] w-full rounded-md border border-gray-300 px-3 py-2 shadow-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500"
            rows={4}
          />
        </div>
      </CardContent>
    </Card>
  );
}
