import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardFooter, CardHeader } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { showError, showSuccess } from '@/helpers/toast';
import authorizeService from '@/services/modules/authorize/role.Service';
import { useSave } from '@/stores/useStores';
import { Plus } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { FormDataRoles } from './add-new-role-tab';

interface PreviewTabProps {
  formDataRoles: FormDataRoles | null;
}

const PreviewTab = ({ formDataRoles }: PreviewTabProps) => {
  //! State
  const { t } = useTranslation('shared');
  const permissionIdList = formDataRoles?.permissions?.map((permission) => permission.id) ?? [];
  const navigate = useNavigate();
  const save = useSave();

  //! Functions
  const handleCreateRole = async () => {
    try {
      await authorizeService.addPermissionToRole({
        roleId: formDataRoles?.id ? String(formDataRoles.id) : '',
        permissions: permissionIdList,
      });
      showSuccess(t('UserManagement.CreateRoleSuccess'));
      save('activeTab', 'roles');
      navigate(-1);
    } catch (error) {
      showError(error);
    }
  };

  //! Render
  return (
    <div className="mx-auto w-full max-w-4xl space-y-6 py-6">
      <h1 className="text-2xl font-semibold">{t('UserManagement.PreviewRole')}</h1>

      <Card className="border shadow-sm">
        <CardHeader className="pb-2">
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-gray-100">
              <span className="text-gray-500">O</span>
            </div>
            <div className="flex items-center gap-2">
              <h2 className="text-xl font-semibold">
                {formDataRoles?.name ?? t('UserManagement.RoleNamePlaceholder')}
              </h2>
              <Badge
                variant="outline"
                className="border-green-200 bg-green-50 text-green-700 hover:bg-green-50"
              >
                {t('UserManagement.Active')}
              </Badge>
            </div>
          </div>
        </CardHeader>

        <CardContent className="space-y-6">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div>
              <p className="mb-1 text-sm text-gray-500">{t('UserManagement.MainAction')}:</p>
              <p className="font-medium">{t('UserManagement.Update')}</p>
            </div>
            <div>
              <p className="mb-1 text-sm text-gray-500">{t('UserManagement.MainResource')}:</p>
              <p className="font-medium">{t('UserManagement.User')}</p>
            </div>
          </div>

          <Separator />

          <div>
            <p className="mb-3 text-sm text-gray-500">
              {t('UserManagement.DetailedPermissions')} : {formDataRoles?.permissions?.length ?? 0}
            </p>
            <div className="rounded-md bg-gray-50 p-4 text-sm text-gray-500">
              {(formDataRoles?.permissions?.length ?? 0) > 0 ? (
                <ul className="list-disc pl-5">
                  {formDataRoles?.permissions?.map((permission) => (
                    <li key={permission.id} className="mb-1">
                      {permission.name}
                    </li>
                  ))}
                </ul>
              ) : (
                <p>{t('UserManagement.NoPermissionsSelected')}</p>
              )}
            </div>
          </div>
        </CardContent>

        <CardFooter className="flex justify-between pt-6">
          <Button
            size="lg"
            className="gap-2"
            onClick={handleCreateRole}
            disabled={!formDataRoles || formDataRoles?.permissions?.length === 0}
          >
            <Plus className="h-4 w-4" />
            {t('UserManagement.CreateRole')}
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
};

export default PreviewTab;
