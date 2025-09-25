import FormikField from '@/components/customFieldsFormik/FormikField';
import InputField from '@/components/customFieldsFormik/InputField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import cachedKeys from '@/consts/cachedKeys';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useGetListPermission from '@/services/modules/authorize/hooks/useGetAllPermission';
import useGetListRoles from '@/services/modules/authorize/hooks/useGetListRoles';
import {
  IPermissionsRequest,
  PermissionsList,
} from '@/services/modules/authorize/interfaces/permission.interface';
import { useGet, useSave } from '@/stores/useStores';
import { Form, Formik } from 'formik';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useParams } from 'react-router-dom';
import * as yup from 'yup';

export interface FormDataRoles {
  id?: string | number;
  name?: string;
  roleId?: string;
  permissions?: { id: string; name: string }[];
}

export interface FormDataPermissions {
  roleId: string;
  permissions: string[];
}

interface AddNewRoleTabProps {
  onNext: (values: FormDataRoles) => void;
}

const validationSchema = yup.object().shape({
  id: yup.string().required('Tên vai trò là bắt buộc'),
});

const AddNewRoleTab = ({ onNext }: AddNewRoleTabProps) => {
  //! State
  const { t } = useTranslation('shared');
  const { roleId } = useParams();
  const save = useSave();
  const defaultData = useGet('dataPermissions');
  const cachesFilterPermissions = useGet('cachesFilterPermissions');
  const [isTrigger] = useState(Boolean(!defaultData));
  const [expandedCategories, setExpandedCategories] = useState<string[]>([]);
  const [selectedPermissions, setSelectedPermissions] = useState<{ id: string; name: string }[]>(
    [],
  );

  const { filters } = useFiltersHandler({
    PageSize: cachesFilterPermissions?.PageSize ?? 50,
    CurrentPage: cachesFilterPermissions?.CurrentPage ?? 1,
    TextSearch: cachesFilterPermissions?.TextSearch ?? '',
  });

  const { data: mockDataRoles } = useGetListRoles();

  const stableFilters = useMemo(() => filters as IPermissionsRequest, [filters]);

  const { data: permissionsData } = useGetListPermission(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchPermissions,
    saveData: true,
  });

  useEffect(() => {
    if (permissionsData && isTrigger) {
      save(cachedKeys.dataPermissions, permissionsData);
    }
  }, [permissionsData, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? permissionsData : defaultData),
    [permissionsData, defaultData, isTrigger],
  );

  const initialValues: FormDataRoles = {
    id: roleId ?? '',
    name: roleId ? (mockDataRoles.find((role) => String(role.id) === roleId)?.name ?? '') : '',
  };

  //! Functions
  const toggleCategory = (categoryId: string) => {
    setExpandedCategories((prev) =>
      prev.includes(categoryId) ? prev.filter((id) => id !== categoryId) : [...prev, categoryId],
    );
  };

  const handlePermissionToggle = (permissionId: string, permissionName: string) => {
    setSelectedPermissions((prev) => {
      const exists = prev.find((p) => p.id === permissionId);
      if (exists) {
        return prev.filter((p) => p.id !== permissionId);
      }
      return [...prev, { id: permissionId, name: permissionName }];
    });
  };

  const handleCategoryToggle = (category: PermissionsList) => {
    const categoryPermissions = category.permissions.map((p) => ({
      id: p.id,
      name: p.name,
    }));
    const allSelected = categoryPermissions.every((p) =>
      selectedPermissions.some((sp) => sp.id === p.id),
    );

    if (allSelected) {
      // Deselect all permissions in the category
      setSelectedPermissions((prev) =>
        prev.filter((p) => !categoryPermissions.some((cp) => cp.id === p.id)),
      );
    } else {
      // Select all permissions in the category
      setSelectedPermissions((prev) => {
        const newPermissions = [...prev, ...categoryPermissions];
        // Remove duplicates based on id
        return Array.from(new Map(newPermissions.map((p) => [p.id, p])).values());
      });
    }
  };

  //! Render
  return (
    <Formik
      initialValues={initialValues}
      validationSchema={validationSchema}
      onSubmit={(values) => {
        onNext({ ...values, permissions: selectedPermissions });
      }}
    >
      {({ setFieldValue }) => (
        <Form>
          <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
            {/* Left column - Basic information */}
            <div className="space-y-6">
              <div className="space-y-2">
                <FormikField
                  component={SelectField}
                  label={t('UserManagement.RoleName')}
                  placeholder={t('UserManagement.RoleNamePlaceholder')}
                  name="id"
                  options={mockDataRoles.map((role) => ({
                    value: role.id,
                    label: role.name,
                  }))}
                  afterOnChange={(value: any) => {
                    if (Array.isArray(value)) {
                      setFieldValue('name', '');
                    } else {
                      setFieldValue('name', value?.label ?? '');
                    }
                  }}
                  required
                  shouldHideSearch
                  disabled={!!roleId}
                />
                <FormikField
                  component={InputField}
                  label={t('UserManagement.Description')}
                  name="name"
                  disabled
                />
                <p className="text-sm text-muted-foreground">
                  {t('UserManagement.DescriptionHelpText')}
                </p>
              </div>
            </div>

            {/* Right column - Permissions */}
            <div className="space-y-4 rounded-lg border p-4">
              <div className="flex items-center justify-between">
                <div>
                  <h3 className="text-base font-medium">
                    {t('UserManagement.DetailedPermissions')}
                  </h3>
                  <p className="text-sm text-muted-foreground">
                    {t('UserManagement.SelectSpecificPermissions')}
                  </p>
                </div>
                <div className="text-sm font-medium">
                  {t('UserManagement.SelectedPermissions', { count: selectedPermissions.length })}
                </div>
              </div>

              <div className="space-y-1">
                {dataMain.map((item: PermissionsList, index: number) => {
                  if (item.permissions.length === 0) return null;
                  const isExpanded = expandedCategories.includes(item.categoryId);
                  const categoryPermissionIds = item.permissions.map((p) => p.id);
                  const isCategoryChecked = categoryPermissionIds.every((id) =>
                    selectedPermissions.some((sp) => sp.id === id),
                  );

                  return (
                    <div key={item.categoryId} className="border-b py-3 last:border-b-0">
                      <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <Checkbox
                            id={`permission-${index}`}
                            checked={isCategoryChecked}
                            onCheckedChange={() => handleCategoryToggle(item)}
                          />
                          <Label
                            htmlFor={`permission-${index}`}
                            className="flex cursor-pointer items-center gap-2"
                          >
                            <span>{item.description}</span>
                          </Label>
                        </div>
                        <div className="flex items-center gap-2">
                          <span className="text-sm text-muted-foreground">
                            {item.permissions.length}
                          </span>
                          <button
                            type="button"
                            onClick={() => toggleCategory(item.categoryId)}
                            aria-label={isExpanded ? 'Thu gọn' : 'Mở rộng'}
                          >
                            {isExpanded ? (
                              <ChevronDown className="h-5 w-5" />
                            ) : (
                              <ChevronRight className="h-5 w-5" />
                            )}
                          </button>
                        </div>
                      </div>

                      {isExpanded && (
                        <div className="ml-6 mt-2 space-y-2">
                          {item.permissions.map((permission) => (
                            <div key={permission.id} className="flex items-center gap-2">
                              <Checkbox
                                id={`permission-${permission.id}`}
                                checked={selectedPermissions.some((sp) => sp.id === permission.id)}
                                onCheckedChange={() =>
                                  handlePermissionToggle(permission.id, permission.name)
                                }
                                className="cursor-pointer"
                              />
                              <Label
                                htmlFor={`permission-${permission.id}`}
                                className="cursor-pointer text-sm"
                              >
                                {permission.name}
                              </Label>
                            </div>
                          ))}
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            </div>
          </div>
          <Button type="submit" className="mt-6 w-full">
            {t('UserManagement.SaveAndNext')}
          </Button>
        </Form>
      )}
    </Formik>
  );
};

export default AddNewRoleTab;
