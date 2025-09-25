import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import Loading from '@/components/ui/loading';
import { Switch } from '@/components/ui/switch';
import cachedKeys from '@/consts/cachedKeys';
import { DateTimeFormat } from '@/consts/dates';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetListProhibited from '@/services/modules/prohibited/hooks/useGetListProhibited';
import {
  IProhibitedRequest,
  ProhbitedList,
} from '@/services/modules/prohibited/interfaces/prohibited.interface';
import prohibitedService from '@/services/modules/prohibited/prohibited.service';
import { useGet, useSave } from '@/stores/useStores';
import { Edit, MoreHorizontal, ShieldAlert, Trash2 } from 'lucide-react';
import moment from 'moment';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { GenericFilters, IValueFormPageHeader } from '../manageuser/components/generic-filters';
import { UserStats } from '../manageuser/components/user-stats';
import DialogAddNewProhibitedApp, {
  ProhibitedFormValues,
} from './dialogs/DialogAddNewProhibitedApp';
import { useTranslation } from 'react-i18next';

const ProhibitedPage = () => {
  //!State
  const { t } = useTranslation('shared');
  const defaultProhibited = useGet('dataProhibited');
  const totalProhibitedCount = useGet('totalProhibitedCount');
  const totalPageProhibitedCount = useGet('totalPageProhibitedCount');
  const cachesFilterProhibited = useGet('cachesFilterProhibited');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultProhibited));
  const save = useSave();
  const [editProhibited, setEditProhibited] = useState<ProhbitedList | null>(null);
  const [openAskAddNewProhibited, toggleAskAddNewProhibited, shouldRenderAskAddNewProhibited] =
    useToggleDialog();

  const { filters, setFilters } = useFiltersHandler({
    PageSize: cachesFilterProhibited?.PageSize ?? 50,
    CurrentPage: cachesFilterProhibited?.CurrentPage ?? 1,
    TextSearch: cachesFilterProhibited?.TextSearch ?? '',
    IsActive:
      cachesFilterProhibited?.IsActive !== undefined ? cachesFilterProhibited.IsActive : undefined,
    RiskLevel:
      cachesFilterProhibited?.RiskLevel !== undefined
        ? cachesFilterProhibited.RiskLevel
        : undefined,
    Category:
      cachesFilterProhibited?.Category !== undefined ? cachesFilterProhibited.Category : undefined,
    TypeApp:
      cachesFilterProhibited?.TypeApp !== undefined ? cachesFilterProhibited.TypeApp : undefined,
  });

  const stableFilters = useMemo(() => filters as IProhibitedRequest, [filters]);

  const {
    data: dataProhibited,
    loading,
    refetch,
  } = useGetListProhibited(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachesFilterProhibited?.refetchKey,
    saveData: true,
  });

  useEffect(() => {
    if (dataProhibited && isTrigger) {
      save(cachedKeys.dataProhibited, dataProhibited);
    }
  }, [dataProhibited, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? dataProhibited : defaultProhibited),
    [dataProhibited, defaultProhibited, isTrigger],
  );

  const statItems = useMemo(() => {
    const totalApp = dataMain.length ?? 0;
    const activeCount = dataMain.filter((item: any) => item.isActive).length ?? 0;
    const highRiskCount = dataMain.filter((item: any) => item.riskLevel === 2).length ?? 0;

    return [
      {
        title: t('ProhibitedManagement.TotalItems'),
        value: totalApp,
        icon: <ShieldAlert className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
      {
        title: t('ProhibitedManagement.ActiveItems'),
        value: activeCount,
        icon: <ShieldAlert className="h-6 w-6 text-green-500" />,
        bgColor: 'bg-green-50',
      },
      {
        title: t('ProhibitedManagement.HighRiskItems'),
        value: highRiskCount,
        icon: <ShieldAlert className="h-6 w-6 text-yellow-500" />,
        bgColor: 'bg-yellow-50',
      },
    ];
  }, [dataMain, t]);

  const columns = [
    {
      label: t('ProhibitedManagement.AppName'),
      accessor: 'appName',
      sortable: false,
      Cell: (row: ProhbitedList) => <span className="font-medium">{row.appName ?? 'N/A'}</span>,
    },
    {
      label: t('ProhibitedManagement.ProcessName'),
      accessor: 'processName',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <span className="text-sm text-gray-600">{row.processName ?? 'N/A'}</span>
      ),
    },
    {
      label: t('ProhibitedManagement.Category'),
      accessor: 'category',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <span className="text-sm text-gray-600">{row.category === 1 ? 'Website' : 'Tool'}</span>
      ),
    },
    {
      label: t('ProhibitedManagement.RiskLevel'),
      accessor: 'riskLevel',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <span
          className={`rounded-full px-2 py-1 text-xs font-medium ${
            row.riskLevel === 0
              ? 'bg-green-100 text-green-800'
              : row.riskLevel === 1
                ? 'bg-yellow-100 text-yellow-800'
                : 'bg-red-100 text-red-800'
          }`}
        >
          {row.riskLevel === 0
            ? t('ProhibitedManagement.LowRisk')
            : row.riskLevel === 1
              ? t('ProhibitedManagement.MediumRisk')
              : t('ProhibitedManagement.HighRisk')}
        </span>
      ),
    },
    {
      label: t('ProhibitedManagement.Status'),
      accessor: 'isActive',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <span
          className={`rounded-full px-2 py-1 text-xs font-medium ${
            row.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
          }`}
        >
          {row.isActive ? t('ProhibitedManagement.Active') : t('ProhibitedManagement.Inactive')}
        </span>
      ),
    },
    {
      label: t('ProhibitedManagement.LastUpdated'),
      accessor: 'updatedAt',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <span className="text-sm text-gray-600">
          {moment(row.updatedAt).format(DateTimeFormat.DayMonthYear) ?? 'Không có mô tả'}
        </span>
      ),
    },
    {
      label: t('ProhibitedManagement.TypeApp'),
      accessor: 'typeApp',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <span
          className={`rounded-full px-2 py-1 text-xs font-medium ${
            row.typeApp === 0 ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'
          }`}
        >
          {row.typeApp === 0
            ? t('ProhibitedManagement.BlackList')
            : t('ProhibitedManagement.WhiteList')}
        </span>
      ),
    },
    {
      label: t('ProhibitedManagement.ChangeStatus'),
      accessor: 'changeStatus',
      sortable: false,
      Cell: (row: ProhbitedList) => (
        <Switch
          checked={row.isActive}
          onCheckedChange={async () => {
            try {
              await prohibitedService.changeStatusProhibited([row.appId]);
              showSuccess(t('ProhibitedManagement.ChangeStatusSuccess'));
              save(cachedKeys.dataProhibited, null);
              setIsTrigger(true);
              refetch();
            } catch (error) {
              showError(error);
            }
          }}
        />
      ),
    },
    {
      label: t('ProhibitedManagement.Actions'),
      accessor: 'actions',
      width: 120,
      sortable: false,
      Cell: (row: ProhbitedList) => (
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
                  setEditProhibited(row);
                  toggleAskAddNewProhibited();
                }}
                className="cursor-pointer"
              >
                <Edit className="mr-2 h-4 w-4" /> {t('Edit')}
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={async () => {
                  try {
                    await prohibitedService.deleteProhibited([row.appId]);
                    showSuccess(t('ProhibitedManagement.DeleteSuccess'));
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

  //!Functions
  const handleChangePage = useCallback(
    (page: number) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          CurrentPage: page,
        };
        save(cachedKeys.cachesFilterProhibited, newParams);
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
        save(cachedKeys.cachesFilterProhibited, newParams);
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
        save(cachedKeys.cachesFilterProhibited, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleToggleAskAddNewProhibited = useCallback(() => {
    toggleAskAddNewProhibited();
    if (openAskAddNewProhibited) {
      setEditProhibited(null);
    }
  }, [toggleAskAddNewProhibited, openAskAddNewProhibited]);

  const handAddEditProhibited = async (values: ProhibitedFormValues) => {
    try {
      const bodyUpload = new FormData();
      bodyUpload.append('AppId', values.AppId ?? '');
      bodyUpload.append('AppName', values.AppName);
      bodyUpload.append('ProcessName', values.ProcessName);
      bodyUpload.append('Description', values.Description ?? '');
      bodyUpload.append('AppIconUrl', values.AppIconUrl ?? '');
      bodyUpload.append('IsActive', values.IsActive ? 'true' : 'false');
      bodyUpload.append(
        'RiskLevel',
        values.RiskLevel !== undefined ? values.RiskLevel.toString() : '',
      );
      bodyUpload.append(
        'Category',
        values.Category !== undefined ? values.Category.toString() : '',
      );
      bodyUpload.append('TypeApp', values.TypeApp ?? '');
      if (editProhibited) {
        await prohibitedService.createUpdateProhibited(bodyUpload);
        setEditProhibited(null);
      } else {
        await prohibitedService.createUpdateProhibited(bodyUpload);
      }
      toggleAskAddNewProhibited();
      showSuccess(
        editProhibited
          ? t('ProhibitedManagement.UpdateSuccess')
          : t('ProhibitedManagement.CreateSuccess'),
      );
      refetch();
    } catch (error) {
      showError(error);
    }
  };

  if (loading) {
    return <Loading />;
  }
  //!Render
  return (
    <PageWrapper name={t('ProhibitedManagement.Title')} className="bg-white dark:bg-gray-900">
      <ExamHeader
        title={t('ProhibitedManagement.Title')}
        subtitle={t('ProhibitedManagement.Subtitle')}
        icon={<ShieldAlert className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-red-600 to-blue-700 px-6 py-6 shadow-lg"
      />
      <div className="space-y-6 p-4">
        <UserStats statItems={statItems} className="lg:grid-cols-3" />
        {shouldRenderAskAddNewProhibited && (
          <DialogAddNewProhibitedApp
            isOpen={openAskAddNewProhibited}
            toggle={handleToggleAskAddNewProhibited}
            onSubmit={handAddEditProhibited}
            editProhibited={editProhibited}
          />
        )}
        <GenericFilters
          className="md:grid-cols-3 lg:grid-cols-7"
          searchPlaceholder={t('ProhibitedManagement.SearchPlaceholder')}
          onSearch={handleSearch}
          initialSearchQuery={filters?.TextSearch ?? ''}
          initialFilterValues={cachesFilterProhibited ?? {}}
          filters={[
            {
              key: 'IsActive',
              placeholder: t('ProhibitedManagement.StatusPlaceholder'),
              options: [
                { value: undefined, label: t('ProhibitedManagement.AllStatus') },
                { value: true, label: t('ProhibitedManagement.Active') },
                { value: false, label: t('ProhibitedManagement.Inactive') },
              ],
            },
            {
              key: 'RiskLevel',
              placeholder: t('ProhibitedManagement.RiskLevel'),
              options: [
                { value: undefined, label: t('ProhibitedManagement.AllRiskLevels') },
                { value: 0, label: t('ProhibitedManagement.LowRisk') },
                { value: 1, label: t('ProhibitedManagement.MediumRisk') },
                { value: 2, label: t('ProhibitedManagement.HighRisk') },
              ],
            },
            {
              key: 'Category',
              placeholder: t('ProhibitedManagement.Category'),
              options: [
                { value: undefined, label: t('ProhibitedManagement.Category') },
                { value: 1, label: t('ProhibitedManagement.Website') },
                { value: 2, label: t('ProhibitedManagement.Tool') },
              ],
            },
            {
              key: 'TypeApp',
              placeholder: t('ProhibitedManagement.TypeApp'),
              options: [
                { value: undefined, label: t('ProhibitedManagement.AllTypeApps') },
                { value: 0, label: t('ProhibitedManagement.BlackList') },
                { value: 1, label: t('ProhibitedManagement.WhiteList') },
              ],
            },
          ]}
          onFilterChange={(
            newFilters: Record<string, string | number | boolean | null | undefined>,
          ) => {
            setIsTrigger(true);
            setFilters((prev: any) => {
              const newParams = {
                ...prev,
                ...newFilters,
              };
              save(cachedKeys.cachesFilterProhibited, newParams);
              return newParams;
            });
          }}
          onAddNew={handleToggleAskAddNewProhibited}
          addNewButtonText={t('ProhibitedManagement.AddNewProhibitedApp')}
        />
        <MemoizedTablePaging
          columns={columns}
          data={dataMain ?? []}
          currentPage={filters?.CurrentPage ?? 1}
          currentSize={filters?.PageSize ?? 50}
          totalPage={totalPageProhibitedCount ?? 1}
          total={totalProhibitedCount ?? 0}
          loading={loading}
          handleChangePage={handleChangePage}
          handleChangeSize={handleChangePageSize}
          noResultText={t('NoDataFound')}
        />
      </div>
    </PageWrapper>
  );
};

export default ProhibitedPage;
