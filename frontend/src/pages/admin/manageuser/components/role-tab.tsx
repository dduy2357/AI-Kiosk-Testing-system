import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from '@/components/ui/accordion';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { DropdownMenuItem } from '@/components/ui/dropdown-menu';
import { Switch } from '@/components/ui/switch';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { containerVariants, itemVariants } from '@/helpers/common';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import useGetAllRolePermission from '@/services/modules/authorize/hooks/useGetAllRolePermission';
import type {
  IPermissionRequest,
  PermissionList,
} from '@/services/modules/authorize/interfaces/role.interface';
import authorizeService from '@/services/modules/authorize/role.Service';
import { useGet, useSave } from '@/stores/useStores';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuTrigger,
} from '@radix-ui/react-dropdown-menu';
import { motion } from 'framer-motion';
import {
  Edit,
  Eye,
  MoreHorizontal,
  Plus,
  Settings,
  Shield,
  Trash2,
  UserCog,
  Users,
} from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import DialogAddNewRole, { type RoleFormValues } from '../dialogs/DialogAddNewRole';
import { UserStats } from './user-stats';
import DialogConfirm from '@/components/dialogs/DialogConfirm';
import { useTranslation } from 'react-i18next';

const RoleTab = () => {
  //! State
  const { t } = useTranslation('shared');
  const [openAskDeleteRole, toggleAskDeleteRole, shouldRenderAskDeleteRole] = useToggleDialog();
  const navigate = useNavigate();
  const defaultData = useGet('dataPermission');
  const cachesFilterPermission = useGet('cachesFilterPermission');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultData));
  const save = useSave();
  const [openAddNewRole, toggleAddNewRole, shouldRenderAddNewRole] = useToggleDialog();
  const [editRole, setEditRole] = useState<PermissionList | null>(null);
  const { filters } = useFiltersHandler({
    pageSize: cachesFilterPermission?.pageSize ?? 50,
    currentPage: cachesFilterPermission?.currentPage ?? 1,
    textSearch: '',
  });
  const stableFilters = useMemo(() => filters as IPermissionRequest, [filters]);
  const { data: dataPermission, refetch } = useGetAllRolePermission(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchPermission,
    saveData: true,
  });

  useEffect(() => {
    if (dataPermission && isTrigger) {
      save(cachedKeys.dataPermission, dataPermission);
    }
  }, [dataPermission, isTrigger, save]);
  const dataMain = useMemo(
    () => (isTrigger ? dataPermission : defaultData),
    [dataPermission, defaultData, isTrigger],
  );
  const rolesItems = useMemo(() => {
    const totalRoles = dataMain.length;
    const adminCount = dataMain.filter(
      (r: { roleName: string }) => r.roleName === 'Administrator',
    ).length;
    const totalPermissions = dataMain.reduce((sum: any, role: { categories: any[] }) => {
      const categoryPermissions = role.categories.reduce(
        (catSum, cat) => catSum + cat.permissions.length,
        0,
      );
      return sum + categoryPermissions;
    }, 0);
    return [
      {
        title: t('UserManagement.TotalRoles'),
        value: totalRoles,
        icon: <UserCog className="h-6 w-6 text-gray-500" />,
        bgColor: 'bg-gray-50',
      },
      {
        title: t('UserManagement.Admin'),
        value: adminCount,
        icon: <Shield className="h-6 w-6 text-red-500" />,
        bgColor: 'bg-red-50',
      },
      {
        title: t('UserManagement.TotalPermissions'),
        value: totalPermissions,
        icon: <Shield className="h-6 w-6 text-green-500" />,
        bgColor: 'bg-green-50',
      },
    ];
  }, [dataMain, t]);

  //! Functions
  const getRoleIcon = (roleName: string) => {
    if (roleName.includes('Administrator')) return Shield;
    if (roleName.includes('Lecture')) return Settings;
    if (roleName.includes('Student')) return Eye;
    return Users;
  };
  const getRoleColor = (roleName: string) => {
    if (roleName.includes('Administrator')) return 'bg-red-50 text-red-700 border-red-200';
    if (roleName.includes('Lecture')) return 'bg-blue-50 text-blue-700 border-blue-200';
    if (roleName.includes('Student')) return 'bg-purple-50 text-purple-700 border-purple-200';
    return 'bg-green-50 text-green-700 border-green-200';
  };
  const handleAddEditRole = useCallback(
    async (values: RoleFormValues) => {
      try {
        if (editRole !== null) {
          await authorizeService.crateUpdateRole({ ...values });
          showSuccess(t('UserManagement.RoleUpdated'));
          setIsTrigger(true);
          refetch();
          toggleAddNewRole();
          setEditRole(null);
          return;
        }
        await authorizeService.crateUpdateRole({ ...values });
        showSuccess(t('UserManagement.RoleCreated'));
        setIsTrigger(true);
        refetch();
        toggleAddNewRole();
      } catch (error) {
        showError(error);
      }
    },
    [editRole, refetch, toggleAddNewRole, t],
  );
  const handleToggleActiveDeactivePermission = useCallback(
    async (permissionId: string) => {
      try {
        await authorizeService.toggleActiveDeactivePermission(permissionId);
        showSuccess(t('UserManagement.PermissionStatusUpdated'));
        refetch();
      } catch (error) {
        showError(error);
      }
    },
    [refetch, t],
  );
  const handleToggleRoleStatus = useCallback(
    async (roleId: string) => {
      try {
        await authorizeService.toggleActiveDeactiveRole(roleId);
        showSuccess(t('UserManagement.RoleStatusUpdated'));
        refetch();
      } catch (error) {
        showError(error);
      }
    },
    [refetch, t],
  );
  const handleToggleDeleteRole = useCallback(() => {
    toggleAskDeleteRole();
    if (editRole) {
      setEditRole(null);
    }
  }, [toggleAskDeleteRole, editRole]);

  const handleDeletePermission = useCallback(
    async (permissionId: string, roleId: number) => {
      try {
        await authorizeService.removePermissionFromRole({
          roleId: roleId,
          permissions: [permissionId],
        });
        showSuccess(t('UserManagement.PermissionDeleted'));
        refetch();
      } catch (error) {
        showError(error);
      }
    },
    [refetch, t],
  );

  // Render
  return (
    <div className="space-y-6">
      {shouldRenderAddNewRole && (
        <DialogAddNewRole
          isOpen={openAddNewRole}
          toggle={toggleAddNewRole}
          onSubmit={handleAddEditRole}
          editRole={editRole}
        />
      )}
      {shouldRenderAskDeleteRole && (
        <DialogConfirm
          title={t('UserManagement.ConfirmDeleteRole')}
          content={t('UserManagement.ConfirmDeleteRoleContent')}
          isOpen={openAskDeleteRole}
          toggle={handleToggleDeleteRole}
          onSubmit={async () => {
            try {
              if (editRole && editRole.roleId) {
                await authorizeService.deleteRole(Number(editRole.roleId));
                showSuccess(t('UserManagement.RoleDeleted'));
                toggleAskDeleteRole();
                setEditRole(null);
                setIsTrigger(true);
                refetch();
              }
            } catch (error) {
              showError(error);
            }
          }}
          variantYes="destructive"
        />
      )}
      {/* Stats Overview */}
      <UserStats statItems={rolesItems} className="lg:grid-cols-3" />
      {/* Role List */}
      <Card className="border-0 bg-white/80 shadow-xl backdrop-blur-sm">
        <CardContent className="p-8">
          {/* Header Section */}
          <div className="mb-8 flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="space-y-2">
              <div className="flex items-center gap-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-xl bg-gradient-to-br from-blue-500 to-indigo-600 text-white shadow-lg">
                  <Shield className="h-6 w-6" />
                </div>
                <div>
                  <h1 className="text-3xl font-bold text-gray-900">
                    {t('UserManagement.RoleList')}
                  </h1>
                  <p className="text-sm text-gray-600">{t('UserManagement.RoleListDescription')}</p>
                </div>
              </div>
            </div>
            <Button
              onClick={toggleAddNewRole}
              className="bg-gradient-to-r from-blue-600 to-indigo-600 shadow-lg transition-all duration-200 hover:from-blue-700 hover:to-indigo-700 hover:shadow-xl"
              size="lg"
            >
              <Plus className="mr-2 h-4 w-4" /> {t('UserManagement.AddNewRole')}
            </Button>
          </div>
          {/* Roles Grid */}
          <motion.div
            className="grid grid-cols-1 gap-6 lg:grid-cols-2 xl:grid-cols-3"
            variants={containerVariants}
            initial="hidden"
            animate="visible"
          >
            {dataMain.map((role: PermissionList) => {
              const IconComponent = getRoleIcon(role.roleName);
              const roleColorClass = getRoleColor(role.roleName);
              return (
                <motion.div key={role.roleId} variants={itemVariants}>
                  <Card className="group border-0 bg-white shadow-md transition-all duration-300 hover:scale-[1.01] hover:shadow-lg">
                    <CardContent className="p-6">
                      <div className="mb-4 flex items-start justify-between">
                        <div className="flex items-center gap-3">
                          <div
                            className={`flex h-10 w-10 items-center justify-center rounded-full ${roleColorClass.split(' ')[0]} shadow-sm`}
                          >
                            <IconComponent className="h-5 w-5" />
                          </div>
                          <div className="flex items-center gap-2">
                            <Badge variant="outline" className={`${roleColorClass} font-medium`}>
                              {role.roleName}
                            </Badge>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Switch
                            checked={role.isActive}
                            onCheckedChange={() => handleToggleRoleStatus(String(role.roleId))}
                            aria-label={`Toggle ${role.roleName} status`}
                          />
                          <DropdownMenu>
                            <DropdownMenuTrigger asChild>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 opacity-0 transition-opacity group-hover:opacity-100"
                              >
                                <MoreHorizontal className="h-4 w-4" />
                              </Button>
                            </DropdownMenuTrigger>
                            <DropdownMenuContent
                              align="end"
                              className="w-56 rounded-lg p-1 shadow-lg"
                            >
                              <DropdownMenuItem
                                className="flex cursor-pointer items-center rounded-md px-3 py-2 text-sm transition-colors duration-150 hover:bg-accent hover:text-accent-foreground"
                                onClick={() =>
                                  navigate(`${BaseUrl.AdminAddNewRole}/${role.roleId}`)
                                }
                              >
                                <Plus className="mr-2 h-4 w-4" />{' '}
                                {t('UserManagement.AddPermission')}
                              </DropdownMenuItem>
                              <DropdownMenuItem
                                className="flex cursor-pointer items-center rounded-md px-3 py-2 text-sm transition-colors duration-150 hover:bg-accent hover:text-accent-foreground"
                                onClick={() => {
                                  setEditRole(role);
                                  toggleAddNewRole();
                                }}
                              >
                                <Edit className="mr-2 h-4 w-4" /> {t('Edit')}
                              </DropdownMenuItem>
                              <DropdownMenuItem
                                className="flex cursor-pointer items-center rounded-md px-3 py-2 text-sm text-red-600 transition-colors duration-150 hover:bg-red-50 hover:text-red-700"
                                onClick={() => {
                                  setEditRole(role);
                                  toggleAskDeleteRole();
                                }}
                              >
                                <Trash2 className="mr-2 h-4 w-4" /> {t('Delete')}
                              </DropdownMenuItem>
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </div>
                      </div>
                      {/* Permissions */}
                      <div>
                        <h4 className="mb-3 text-sm font-semibold text-gray-700">
                          {t('UserManagement.Permissions')} ({role.categories.length})
                        </h4>
                        <Accordion type="multiple" className="w-full">
                          {role.categories.map((category) => (
                            <AccordionItem key={category.categoryId} value={category.categoryId}>
                              <AccordionTrigger className="w-full rounded-md px-3 py-2 text-left text-xs text-gray-700 transition-colors hover:bg-gray-100">
                                {category.description}
                              </AccordionTrigger>
                              <AccordionContent className="ml-4 mt-2 space-y-2">
                                {category.permissions.map((permission) => (
                                  <div
                                    key={permission.id}
                                    className="flex items-center justify-between rounded-md bg-gray-50 p-2 text-xs text-gray-600"
                                  >
                                    <span>{permission.name}</span>
                                    <div className="flex items-center gap-2">
                                      <Badge
                                        variant="outline"
                                        className={`${
                                          permission.isActive
                                            ? 'bg-green-100 text-green-700'
                                            : 'bg-red-100 text-red-700'
                                        } px-2 py-1`}
                                      >
                                        <Button
                                          variant="ghost"
                                          size="sm"
                                          className="h-auto p-0 text-xs font-medium"
                                          onClick={() =>
                                            handleToggleActiveDeactivePermission(permission.id)
                                          }
                                        >
                                          {permission.isActive
                                            ? t('UserManagement.Active')
                                            : t('UserManagement.Inactive')}
                                        </Button>
                                      </Badge>
                                      <Button
                                        variant="ghost"
                                        size="icon"
                                        className="h-6 w-6 text-red-600 hover:bg-red-50 hover:text-red-700"
                                        onClick={() =>
                                          handleDeletePermission(permission.id, role.roleId)
                                        }
                                        aria-label={`Delete permission ${permission.name}`}
                                      >
                                        <Trash2 className="h-4 w-4" />
                                      </Button>
                                    </div>
                                  </div>
                                ))}
                              </AccordionContent>
                            </AccordionItem>
                          ))}
                        </Accordion>
                      </div>
                    </CardContent>
                  </Card>
                </motion.div>
              );
            })}
          </motion.div>
        </CardContent>
      </Card>
    </div>
  );
};

export default RoleTab;
