import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import cachedKeys from '@/consts/cachedKeys';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetListPermission from '@/services/modules/permission/hooks/useGetListPermission';
import { PermissionList } from '@/services/modules/permission/interfaces/permission.interface';
import { useGet, useSave } from '@/stores/useStores';
import { Edit, LockIcon, MoreHorizontal, Trash2 } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { GenericFilters, IValueFormPageHeader } from '../manageuser/components/generic-filters';
import { UserStats } from '../manageuser/components/user-stats';
import DialogAddNewCategoryPermission from './dialogs/DialogAddNewCatePermission';
import permissionService, {
  CreateUpdatePermissionData,
} from '@/services/modules/permission/permission.service';
import { showError, showSuccess } from '@/helpers/toast';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Button } from '@/components/ui/button';
import { useTranslation } from 'react-i18next';

const PermissionPage = () => {
  //!State
  const { t } = useTranslation('shared');
  const save = useSave();
  const defaultData = useGet('dataPermissionsList');
  const totalPermissionsListCount = useGet('totalPermissionsListCount');
  const totalPagePermissionsListCount = useGet('totalPagePermissionsListCount');
  const cachesFilterPermissionsList = useGet('cachesFilterPermissionsList');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultData));
  const [
    openAddNewCategoryPermission,
    toggleAddNewCategoryPermission,
    shouldRenderAddNewCategoryPermission,
  ] = useToggleDialog();
  const [permissionId, setPermissionId] = useState<string | null>(null);
  const handleToggleAddNewCategoryPermission = useCallback(() => {
    toggleAddNewCategoryPermission();
    if (openAddNewCategoryPermission) {
      setPermissionId(null);
    }
  }, [openAddNewCategoryPermission, toggleAddNewCategoryPermission]);

  const { filters, setFilters } = useFiltersHandler({
    PageSize: cachesFilterPermissionsList?.PageSize ?? 50,
    CurrentPage: cachesFilterPermissionsList?.CurrentPage ?? 1,
    TextSearch: cachesFilterPermissionsList?.TextSearch ?? '',
  });

  const {
    data: dataPermissions,
    loading,
    refetch,
  } = useGetListPermission(filters, {
    isTrigger: isTrigger,
    refetchKey: cachesFilterPermissionsList?.refetchKey,
    saveData: true,
  });

  useEffect(() => {
    if (dataPermissions && isTrigger) {
      save(cachedKeys.dataPermissions, dataPermissions);
    }
  }, [dataPermissions, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? dataPermissions : defaultData),
    [dataPermissions, defaultData, isTrigger],
  );

  const columns = [
    {
      label: t('PermissionManagement.PermissionName'),
      accessor: 'description',
      sortable: false,
      Cell: (row: PermissionList) => (
        <span className="text-sm font-medium text-gray-900 dark:text-gray-100">
          {row.description}
        </span>
      ),
    },
    {
      label: t('PermissionManagement.Action'),
      accessor: 'actions',
      width: 120,
      sortable: false,
      Cell: (row: PermissionList) => (
        <div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                onClick={() => {
                  setPermissionId(row.categoryId);
                  toggleAddNewCategoryPermission();
                }}
                className="cursor-pointer"
              >
                <Edit className="mr-2 h-4 w-4" /> {t('Edit')}
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={async () => {
                  try {
                    await permissionService.deletePermission(row.categoryId);
                    showSuccess(t('PermissionManagement.DeleteSuccess'));
                    refetch();
                  } catch (error) {
                    showError(error);
                  }
                }}
                className="cursor-pointer text-red-600"
              >
                <Trash2 className="mr-2 h-4 w-4" /> {t('Delete')}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ];

  const statItems = useMemo(() => {
    return [
      {
        title: t('PermissionManagement.TotalPermissions'),
        value: totalPermissionsListCount ?? 0,
        icon: <LockIcon className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
    ];
  }, [totalPermissionsListCount, t]);

  //!Functions
  const handleChangePage = useCallback(
    (page: number) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          CurrentPage: page,
        };
        save(cachedKeys.cachesFilterPermissionsList, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleChangePageSize = useCallback(
    (size: number) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          PageSize: size,
          CurrentPage: 1,
        };
        save(cachedKeys.cachesFilterPermissionsList, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleSearch = useCallback(
    (value: IValueFormPageHeader) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          TextSearch: value?.textSearch,
          CurrentPage: 1,
        };
        save(cachedKeys.cachesFilterPermissionsList, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleAddEditCategoryPermission = async (values: CreateUpdatePermissionData) => {
    try {
      await permissionService.createUpdatePermission(values);
      showSuccess(
        permissionId
          ? t('PermissionManagement.EditSuccess')
          : t('PermissionManagement.CreateSuccess'),
      );
      toggleAddNewCategoryPermission();
      setPermissionId(null);
      refetch();
    } catch (error) {
      showError(error);
    }
  };

  //!Render
  return (
    <PageWrapper name={t('PermissionManagement.Title')} className="bg-white dark:bg-gray-900">
      <ExamHeader
        title={t('PermissionManagement.Title')}
        subtitle={t('PermissionManagement.Subtitle')}
        icon={<LockIcon className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-red-600 to-blue-700 px-6 py-6 shadow-lg"
      />
      <div className="space-y-6 p-4">
        <UserStats statItems={statItems} className="lg:grid-cols-1" />
        <GenericFilters
          className="md:grid-cols-3 lg:grid-cols-3"
          searchPlaceholder={t('PermissionManagement.SearchPlaceholder')}
          onSearch={handleSearch}
          initialSearchQuery={cachesFilterPermissionsList?.TextSearch ?? ''}
          initialFilterValues={{}}
          onAddNew={toggleAddNewCategoryPermission}
          addNewButtonText={t('PermissionManagement.AddNewPermission')}
        />
        <MemoizedTablePaging
          columns={columns}
          data={dataMain ?? []}
          currentPage={filters?.CurrentPage ?? 1}
          currentSize={filters?.PageSize ?? 50}
          totalPage={totalPagePermissionsListCount ?? 1}
          total={totalPermissionsListCount ?? 0}
          loading={loading}
          handleChangePage={handleChangePage}
          handleChangeSize={handleChangePageSize}
        />
        {shouldRenderAddNewCategoryPermission && (
          <DialogAddNewCategoryPermission
            isOpen={openAddNewCategoryPermission}
            toggle={handleToggleAddNewCategoryPermission}
            onSubmit={handleAddEditCategoryPermission}
            permissionId={permissionId}
          />
        )}
      </div>
    </PageWrapper>
  );
};

export default PermissionPage;
