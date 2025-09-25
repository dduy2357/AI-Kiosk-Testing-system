import PageWrapper from '@/components/PageWrapper/PageWrapper';
import TablePaging from '@/components/tableCommon/v2/tablePaging';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { PASSWORD_AFTER_RESET } from '@/consts/common';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import roleService from '@/services/modules/authorize/role.Service';
import useGetListUser from '@/services/modules/user/hooks/useGetListUser';
import {
  InitialFilterUser,
  IUserResetPass,
  UserList,
} from '@/services/modules/user/interfaces/user.interface';
import userService from '@/services/modules/user/user.Service';
import { useGet, useSave } from '@/stores/useStores';
import { Activity, BookOpen, GraduationCap, Lock, Shield, User, Users } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { GenericFilters, IValueFormPageHeader } from './components/generic-filters';
import PermissionTab from './components/permmision-tab';
import RoleTab from './components/role-tab';
import { UserStats } from './components/user-stats';
import { createUserColumns } from './components/user-table-columns';
import DialogPreCheckImportUser, { UserData } from './dialogs/DialogPreCheckImportUser';

const AdminManageUser = () => {
  const { t } = useTranslation('shared');
  const [activeTab, setActiveTab] = useState(() => localStorage.getItem('activeTab') ?? 'users');
  const navigate = useNavigate();
  const forceRefetch = useGet('forceRefetchUser');
  const defaultData = useGet('dataUser');
  const totalUserCount = useGet('totalUserCount');
  const cachedFilterUser = useGet('cachesFilterUser');
  const totalPageUser = useGet('totalPageUserCount');
  const [isTrigger, setTrigger] = useState(Boolean(!defaultData) ?? forceRefetch);
  const save = useSave();
  const [openPrecheckImportUser, togglePrecheckImportUser, shouldRenderPrecheckImportUser] =
    useToggleDialog();

  useEffect(() => {
    localStorage.setItem('activeTab', activeTab);
  }, [activeTab]);

  const { filters, setFilters } = useFiltersHandler({
    pageSize: cachedFilterUser?.pageSize ?? 50,
    currentPage: cachedFilterUser?.currentPage ?? 1,
    textSearch: cachedFilterUser?.textSearch ?? '',
    roleId: cachedFilterUser?.roleId ?? null,
    status: cachedFilterUser?.status ?? null,
    campusId: cachedFilterUser?.campusId ?? null,
    sortType: cachedFilterUser?.sortType ?? null,
  });

  const stableFilters = useMemo(() => filters as InitialFilterUser, [filters]);

  const {
    data: dataUser,
    loading,
    refetch,
  } = useGetListUser(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchUser,
    saveData: true,
  });

  useEffect(() => {
    if (forceRefetch) {
      setTrigger(true);
      refetch();
      save(cachedKeys.forceRefetchUser, false);
    }
  }, [forceRefetch, refetch, save]);

  useEffect(() => {
    if (dataUser && isTrigger) {
      save(cachedKeys.dataUser, dataUser);
    }
  }, [dataUser, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? dataUser : defaultData) ?? [],
    [isTrigger, defaultData, dataUser],
  );

  const statItems = useMemo(() => {
    const totalUsers = dataMain?.length ?? 0;
    const adminCount =
      dataMain?.filter(
        (user: UserList) =>
          Array.isArray(user.roleId) && (user.roleId.includes(3) ?? user.roleId.includes(4)),
      ).length ?? 0;
    const teacherCount =
      dataMain?.filter((user: UserList) => Array.isArray(user.roleId) && user.roleId.includes(2))
        .length ?? 0;
    const studentCount =
      dataMain?.filter((user: UserList) => Array.isArray(user.roleId) && user.roleId.includes(1))
        .length ?? 0;

    return [
      {
        title: t('UserManagement.TotalUsers'),
        value: totalUsers,
        icon: <Users className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
      {
        title: t('UserManagement.Admin'),
        value: adminCount,
        icon: <Activity className="h-6 w-6 text-green-500" />,
        bgColor: 'bg-green-50',
      },
      {
        title: t('UserManagement.Lecturer'),
        value: teacherCount,
        icon: <GraduationCap className="h-6 w-6 text-yellow-500" />,
        bgColor: 'bg-yellow-50',
      },
      {
        title: t('UserManagement.Student'),
        value: studentCount,
        icon: <BookOpen className="h-6 w-6 text-purple-500" />,
        bgColor: 'bg-purple-50',
      },
    ];
  }, [dataMain, t]);

  const columns = createUserColumns({
    navigate,
    BaseUrl,
    onDeactivateUser: async (userId: string) => {
      try {
        await userService.toggleActiveUser(userId);
        showSuccess(t('UserManagement.UpdateSuccess'));
        refetch();
      } catch (error) {
        showError(error);
      }
    },
    onResetPassword: async (userId: string, newPass: string) => {
      const body: IUserResetPass = {
        userId,
        password: newPass,
        rePassword: newPass,
      };
      try {
        await roleService.resetPassword(body);
        showSuccess(`${t('UserManagement.ResetPasswordSuccess')} ${PASSWORD_AFTER_RESET}`);
      } catch (error) {
        showError(error);
      }
    },
  });

  const handleChangePageSize = useCallback(
    (size: number) => {
      setTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          pageSize: size,
          currentPage: 1,
        };
        save(cachedKeys.cachesFilterUser, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  // Updated handleChangePage
  const handleChangePage = useCallback(
    (page: number) => {
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          currentPage: page,
        };
        save(cachedKeys.cachesFilterUser, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleImportUser = useCallback(
    async (file?: File) => {
      try {
        if (file) {
          if (!file.name.match(/\.(csv|xlsx)$/)) {
            showError(t('UserManagement.InvalidFileType'));
            return;
          }

          const formData = new FormData();
          formData.append('file', file);
          const validationResponse = await userService.importUser(formData);
          const userData: UserData = validationResponse.data.data.map((item: any) => ({
            fullName: item.user.fullName,
            phone: item.user.phone,
            userCode: item.user.userCode,
            sex: item.user.sex,
            roleId: item.user.roleId,
            dob: item.user.dob,
            address: item.user.address,
            campusId: item.user.campusId,
            departmentId: item.user.departmentId,
            positionId: item.user.positionId,
            majorId: item.user.majorId,
            specializationId: item.user.specializationId,
            status: item.user.status,
            avatar: '',
            email: item.user.email,
          }));

          await userService.addAfterImportUser(userData);

          showSuccess('Import user successfully');
          refetch();
        } else {
          const input = document.createElement('input');
          input.type = 'file';
          input.accept = '.csv, .xlsx';
          input.onchange = async (event: any) => {
            const selectedFile = event.target.files[0];
            if (!selectedFile) return;

            if (!selectedFile.name.match(/\.(csv|xlsx)$/)) {
              showError(t('UserManagement.InvalidFileType'));
              return;
            }

            const formData = new FormData();
            formData.append('file', selectedFile);
            const validationResponse = await userService.importUser(formData);

            const userData: UserData = validationResponse.data;

            await userService.addAfterImportUser(userData);

            showSuccess('Import user successfully');
            refetch();
          };
          input.click();
        }
      } catch (error) {
        showError(error);
      }
    },
    [refetch, t],
  );

  const handleExportUser = useCallback(async () => {
    try {
      await userService.exportUser();
      showSuccess(t('UserManagement.ExportUserSuccess'));
    } catch (error) {
      showError(error);
    }
  }, [t]);

  // Debounced handleSearch
  const handleSearch = useCallback(
    (value: IValueFormPageHeader) => {
      setTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          textSearch: value.textSearch ?? '',
          currentPage: 1,
        };
        save(cachedKeys.cachesFilterUser, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  return (
    <PageWrapper name={t('UserManagement.Title')} className="bg-white dark:bg-gray-900">
      <ExamHeader
        title={t('UserManagement.Title')}
        subtitle={t('UserManagement.Subtitle')}
        icon={<User className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
      />
      <div className="space-y-6 p-4">
        <Tabs defaultValue="users" value={activeTab} onValueChange={setActiveTab}>
          <TabsList variant="default" fullWidth className="w-full">
            <TabsTrigger variant="gradient" value="users" icon={<User size={16} />}>
              {t('UserManagement.Users')}
            </TabsTrigger>
            <TabsTrigger variant="gradient" value="roles" icon={<Shield size={16} />}>
              {t('UserManagement.Roles')}
            </TabsTrigger>
            <TabsTrigger variant="gradient" value="permissions" icon={<Lock size={16} />}>
              {t('UserManagement.Permissions')}
            </TabsTrigger>
          </TabsList>

          <TabsContent value="users" className="space-y-6">
            <UserStats
              statItems={statItems}
              className="grid-cols-1 md:grid-cols-2 lg:grid-cols-4"
            />
            <GenericFilters
              className="md:grid-cols-9"
              searchPlaceholder={t('UserManagement.SearchPlaceholder')}
              onSearch={handleSearch}
              initialSearchQuery={filters?.textSearch ?? ''}
              filters={[
                {
                  key: 'roleId',
                  placeholder: t('UserManagement.SelectRole'),
                  options: [
                    { value: null, label: t('UserManagement.AllRoles') },
                    { value: 1, label: t('UserManagement.Student') },
                    { value: 2, label: t('UserManagement.Lecturer') },
                    { value: 3, label: t('UserManagement.Supervisor') },
                    { value: 4, label: t('UserManagement.Admin') },
                  ],
                },
                {
                  key: 'status',
                  placeholder: t('UserManagement.SelectStatus'),
                  options: [
                    { value: null, label: t('UserManagement.AllStatus') },
                    { value: '1', label: t('UserManagement.Active') },
                    { value: '0', label: t('UserManagement.Disabled') },
                  ],
                },
                {
                  key: 'campusId',
                  placeholder: t('UserManagement.SelectCampus'),
                  options: [
                    { value: null, label: t('UserManagement.AllCampus') },
                    {
                      value: '11111111-aaaa-bbbb-cccc-111111111111',
                      label: t('UserManagement.HanoiCampus'),
                    },
                    {
                      value: '22222222-aaaa-bbbb-cccc-222222222222',
                      label: t('UserManagement.HCMCampus'),
                    },
                  ],
                },
                {
                  key: 'sortType',
                  placeholder: t('UserManagement.SelectSortType'),
                  options: [
                    { value: null, label: t('UserManagement.AllSortType') },
                    { value: '0', label: t('UserManagement.Ascending') },
                    { value: '1', label: t('UserManagement.Descending') },
                  ],
                },
              ]}
              onFilterChange={(
                newFilters: Record<string, string | number | boolean | null | undefined>,
              ) => {
                setTrigger(true);
                setFilters((prev) => {
                  const updatedFilters = {
                    ...prev,
                    ...newFilters,
                  };
                  save(cachedKeys.cachesFilterSubject, updatedFilters);
                  return updatedFilters;
                });
              }}
              onAddNew={() => navigate(BaseUrl.AdminAddNewUser)}
              addNewButtonText={t('UserManagement.AddNewUser')}
              importButtonText={t('UserManagement.ImportUser')}
              onImport={togglePrecheckImportUser}
              onExport={handleExportUser}
              exportButtonText={t('UserManagement.ExportUser')}
            />
            <TablePaging
              columns={columns}
              loading={loading}
              keyRow="userId"
              data={dataMain ?? []}
              noResultText={t('UserManagement.NoDataFound')}
              total={totalUserCount ?? 0}
              currentPage={filters?.currentPage ?? 1}
              currentSize={filters?.pageSize ?? 1}
              totalPage={totalPageUser ?? 1}
              handleChangeSize={handleChangePageSize}
              handleChangePage={handleChangePage}
            />
          </TabsContent>

          <TabsContent value="roles">
            <RoleTab />
          </TabsContent>

          <TabsContent value="permissions">
            <PermissionTab />
          </TabsContent>
        </Tabs>
      </div>

      {shouldRenderPrecheckImportUser && (
        <DialogPreCheckImportUser
          isOpen={openPrecheckImportUser}
          toggle={togglePrecheckImportUser}
          onSubmit={handleImportUser}
        />
      )}
    </PageWrapper>
  );
};

export default AdminManageUser;
