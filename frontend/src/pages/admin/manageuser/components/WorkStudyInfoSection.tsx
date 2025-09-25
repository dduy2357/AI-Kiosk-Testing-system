import FormikField from '@/components/customFieldsFormik/FormikField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import useGetListCampus from '@/services/modules/campus/hooks/useGetListCampus';
import useGetListDepartment from '@/services/modules/department/hooks/useGetListDepartment';
import useGetListMajor from '@/services/modules/major/hooks/useGetListMajor';
import positionService from '@/services/modules/position/position.service';
import useGetListSpecialization from '@/services/modules/specialization/hooks/useGetSpecialization';
import { useEffect, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';

interface WorkStudyInfoProps {
  values: any;
  isStudent: boolean;
  isLecturer: boolean;
  isAdminOrSupervisor: boolean;
  userId?: string;
}

const WorkStudyInfoSection = ({
  userId,
  values,
  isStudent,
  isLecturer,
  isAdminOrSupervisor,
}: WorkStudyInfoProps) => {
  const { t } = useTranslation('shared');
  const [mockDataPositionIds, setMockDataPositionIds] = useState<any[]>([]);
  const [loadingPosition, setLoadingPosition] = useState(false);
  const {
    data: mockDataCampus,
    loading: loadingCampus,
    refetch: refetchCampus,
  } = useGetListCampus({
    isTrigger: !!userId,
  });
  const {
    data: mockDataDepartments,
    loading: loadingDepartment,
    refetch: refetchDepartment,
  } = useGetListDepartment({
    isTrigger: !!userId,
  });
  const {
    data: mockDataForMajor,
    loading: loadingMajor,
    refetch: refetchMajor,
  } = useGetListMajor({
    isTrigger: !!userId,
  });
  const {
    data: mockDataspecializationIds,
    loading: loadingSpecialization,
    refetch: refetchSpecialization,
  } = useGetListSpecialization({
    isTrigger: !!userId,
  });

  const hasFetchedCampusRef = useRef(false);
  const hasFetchedDepartmentRef = useRef(false);
  const hasFetchedMajorRef = useRef(false);
  const hasFetchedSpecializationRef = useRef(false);
  const hasFetchedPositionRef = useRef(false);

  const fetchPositions = async (departmentId: string) => {
    try {
      setLoadingPosition(true);
      setMockDataPositionIds([]);
      const response = await positionService.getListPosition(departmentId);
      setMockDataPositionIds(response.data);
    } catch (error) {
      setMockDataPositionIds([]);
    } finally {
      setLoadingPosition(false);
    }
  };

  useEffect(() => {
    if (userId && values.departmentId && !hasFetchedPositionRef.current) {
      hasFetchedPositionRef.current = true;
      fetchPositions(values.departmentId);
    }
  }, [userId, values.departmentId]);

  return (
    <div className="space-y-6">
      <h3 className="border-b pb-2 text-lg font-semibold text-gray-900">
        {t('UserManagement.WorkStudyInfoSectionTitle')}
      </h3>
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        <div className="space-y-2">
          <FormikField
            component={SelectField}
            name="campusId"
            label={t('UserManagement.Campus')}
            placeholder={t('UserManagement.CampusPlaceholder')}
            options={mockDataCampus.map((campus: any) => ({
              label: campus.code,
              value: campus.id,
            }))}
            required
            loading={loadingCampus}
            shouldHideSearch
            onToggle={(open: any) => {
              if (open && !hasFetchedCampusRef.current) {
                hasFetchedCampusRef.current = true;
                refetchCampus();
              }
            }}
          />
        </div>
        {isStudent && (
          <div className="space-y-2">
            <FormikField
              component={SelectField}
              name="majorId"
              label={t('UserManagement.Major')}
              placeholder={t('UserManagement.MajorPlaceholder')}
              required
              loading={loadingMajor}
              options={mockDataForMajor.map((major: any) => ({
                label: major.name,
                value: major.id,
              }))}
              shouldHideSearch
              onToggle={(open: any) => {
                if (open && !hasFetchedMajorRef.current) {
                  hasFetchedMajorRef.current = true;
                  refetchMajor();
                }
              }}
            />
          </div>
        )}
        {isLecturer && (
          <div className="space-y-2">
            <FormikField
              component={SelectField}
              name="specializationId"
              label={t('UserManagement.Specialization')}
              placeholder={t('UserManagement.SpecializationPlaceholder')}
              options={mockDataspecializationIds.map((specialization: any) => ({
                label: specialization.name,
                value: specialization.id,
              }))}
              required
              shouldHideSearch
              loading={loadingSpecialization}
              onToggle={(open: any) => {
                if (open && !hasFetchedSpecializationRef.current) {
                  hasFetchedSpecializationRef.current = true;
                  refetchSpecialization();
                }
              }}
            />
          </div>
        )}
      </div>
      {(isLecturer || isAdminOrSupervisor) && (
        <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
          <div className="space-y-2">
            <FormikField
              component={SelectField}
              name="departmentId"
              label={t('UserManagement.Department')}
              placeholder={t('UserManagement.DepartmentPlaceholder')}
              options={mockDataDepartments.map((department: any) => ({
                label: department.name,
                value: department.id,
              }))}
              required
              loading={loadingDepartment}
              shouldHideSearch
              onToggle={(open: any) => {
                if (open && !hasFetchedDepartmentRef.current) {
                  hasFetchedDepartmentRef.current = true;
                  refetchDepartment();
                }
              }}
            />
          </div>
          <div className="space-y-2">
            <FormikField
              component={SelectField}
              name="positionId"
              label={t('UserManagement.Position')}
              placeholder={t('UserManagement.PositionPlaceholder')}
              required
              shouldHideSearch
              disabled={!values.departmentId}
              loading={loadingPosition}
              options={mockDataPositionIds.map((position: any) => ({
                label: position.name,
                value: position.id,
              }))}
              onToggle={(open: any) => {
                if (open && values.departmentId && !userId) {
                  fetchPositions(values.departmentId);
                }
              }}
            />
          </div>
        </div>
      )}
    </div>
  );
};

export default WorkStudyInfoSection;
