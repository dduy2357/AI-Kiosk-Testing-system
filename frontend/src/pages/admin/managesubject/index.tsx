import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
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
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import useGetListSubject from '@/services/modules/subject/hooks/useGetAllSubject';
import {
  ISubjectForm,
  ISubjectRequest,
  SubjectList,
} from '@/services/modules/subject/interfaces/subject.interface';
import subjectService from '@/services/modules/subject/subject.service';
import { useGet, useSave } from '@/stores/useStores';
import { Book, BookOpen, BookOpenIcon, Code, Edit, MoreHorizontal } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { GenericFilters, IValueFormPageHeader } from '../manageuser/components/generic-filters';
import { UserStats } from '../manageuser/components/user-stats';
import DialogAddNewSubject from './dialogs/DialogAddNewSubject';

const ManageSubject = () => {
  //!State
  const { t } = useTranslation('shared');
  const [openAskAddNewSubject, toggleAskAddNewSubject, shouldRenderAskNewSubject] =
    useToggleDialog();
  const defaultData = useGet('dataSubject');
  const totalSubjectCount = useGet('totalSubjectCount');
  const totalPageSubjectCount = useGet('totalPageSubjectCount');
  const cachedFiltersSubject = useGet('cachesFilterSubject');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultData));
  const save = useSave();
  const [editSubject, setEditSubject] = useState<ISubjectForm | null>(null);

  const { filters, setFilters } = useFiltersHandler({
    pageSize: cachedFiltersSubject?.pageSize ?? 50,
    currentPage: cachedFiltersSubject?.currentPage ?? 1,
    textSearch: cachedFiltersSubject?.textSearch ?? '',
    status: cachedFiltersSubject?.status ?? undefined,
  });

  const handleToggleAskAddNewSubject = useCallback(() => {
    toggleAskAddNewSubject();
    if (openAskAddNewSubject) {
      setEditSubject(null);
    }
  }, [toggleAskAddNewSubject, openAskAddNewSubject]);

  const stableFilters = useMemo(() => filters as ISubjectRequest, [filters]);

  const {
    data: dataSubjects,
    loading,
    refetch,
  } = useGetListSubject(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchSubject,
    saveData: true,
  });

  const dataMain = useMemo(
    () => (isTrigger ? dataSubjects : defaultData) ?? [],
    [isTrigger, defaultData, dataSubjects],
  );

  useEffect(() => {
    if (dataSubjects && isTrigger) {
      save(cachedKeys.dataSubject, dataSubjects);
    }
  }, [dataSubjects, isTrigger, save]);

  const columns = [
    {
      label: t('SubjectManagement.SubjectName'),
      accessor: 'subjectName',
      sortable: false,
      Cell: (row: SubjectList) => (
        <div className="flex items-center space-x-3 py-2">
          <Avatar className="h-10 w-10 bg-gradient-to-br from-blue-500 to-purple-600">
            <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-600 font-semibold text-white">
              {row.subjectName.charAt(0).toUpperCase()}
            </AvatarFallback>
          </Avatar>
          <div className="flex flex-col">
            <span className="font-semibold text-gray-900 dark:text-gray-100">
              {row.subjectName}
            </span>
          </div>
        </div>
      ),
    },
    {
      label: t('SubjectManagement.SubjectCode'),
      accessor: 'subjectCode',
      sortable: false,
      Cell: (row: SubjectList) => (
        <div className="flex items-center space-x-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gray-100 dark:bg-gray-800">
            <Code className="h-4 w-4 text-gray-600 dark:text-gray-400" />
          </div>
          <span className="rounded-md bg-gray-50 px-2 py-1 font-mono text-sm font-medium text-gray-900 dark:bg-gray-800 dark:text-gray-100">
            {row.subjectCode}
          </span>
        </div>
      ),
    },
    {
      label: t('SubjectManagement.SubjectDescription'),
      accessor: 'subjectDescription',
      sortable: false,
      Cell: (row: SubjectList) => (
        <div className="flex max-w-xs items-start space-x-2">
          <div className="mt-0.5 flex h-8 w-8 items-center justify-center rounded-lg bg-amber-100 dark:bg-amber-900/20">
            <BookOpen className="h-4 w-4 text-amber-600 dark:text-amber-400" />
          </div>
          <div className="flex flex-col">
            <p className="line-clamp-2 text-sm text-gray-900 dark:text-gray-100">
              {row.subjectDescription ?? 'No description available'}
            </p>
            {!row.subjectDescription && (
              <span className="text-xs italic text-gray-400">
                {t('SubjectManagement.SubjectDescriptionRequired')}
              </span>
            )}
          </div>
        </div>
      ),
    },
    {
      label: t('SubjectManagement.SubjectStatus'),
      accessor: 'status',
      width: 140,
      sortable: false,
      Cell: (row: SubjectList) => (
        <div className="flex items-center justify-center">
          <Badge
            variant={row.status ? 'default' : 'secondary'}
            className={`rounded-full border-0 px-3 py-1 text-xs font-medium shadow-sm ${
              row.status
                ? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-900/20 dark:text-emerald-400'
                : 'bg-red-100 text-red-800 dark:bg-red-900/20 dark:text-red-400'
            } `}
          >
            <div
              className={`mr-2 h-2 w-2 rounded-full ${row.status ? 'bg-emerald-500' : 'bg-red-500'}`}
            />
            {row.status ? t('SubjectManagement.Active') : t('SubjectManagement.Inactive')}
          </Badge>
        </div>
      ),
    },
    {
      label: t('SubjectManagement.SubjectStatus'),
      accessor: 'toggleStatus',
      width: 120,
      sortable: false,
      Cell: (row: SubjectList) => (
        <Switch
          checked={row.status}
          onCheckedChange={async (checked) => {
            try {
              await subjectService.changeActiveSubject(row.subjectId);
              showSuccess(
                checked
                  ? t('SubjectManagement.SubjectActivated')
                  : t('SubjectManagement.SubjectDeactivated'),
              );
              refetch();
            } catch (error) {
              showError(error);
            }
          }}
          className="h-6 w-11"
        />
      ),
    },
    {
      label: t('SubjectManagement.SubjectActions'),
      accessor: 'actions',
      width: 100,
      sortable: false,
      Cell: (row: SubjectList) => (
        <div className="flex items-center justify-center">
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button
                variant="ghost"
                className="h-9 w-9 rounded-lg p-0 transition-colors hover:bg-gray-100 dark:hover:bg-gray-800"
              >
                <MoreHorizontal className="h-4 w-4 text-gray-600 dark:text-gray-400" />
                <span className="sr-only">{t('SubjectManagement.OpenMenuActions')}</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-48">
              <DropdownMenuItem
                onClick={() => {
                  toggleAskAddNewSubject();
                  setEditSubject(row);
                }}
                className="cursor-pointer hover:bg-gray-50 dark:hover:bg-gray-800"
              >
                <Edit className="mr-2 h-4 w-4 text-amber-500" />
                <span>{t('Edit')}</span>
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ];

  const statItems = useMemo(() => {
    const totalSubject = dataMain?.length ?? 0;
    const activeSubjects = dataMain?.filter((subject: any) => subject.status).length ?? 0;
    const inactiveSubjects = totalSubject - activeSubjects;

    return [
      {
        title: t('SubjectManagement.TotalSubjects'),
        value: totalSubject,
        icon: <BookOpenIcon className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
      {
        title: t('SubjectManagement.ActiveSubjects'),
        value: activeSubjects,
        icon: <BookOpenIcon className="h-6 w-6 text-green-500" />,
        bgColor: 'bg-green-50',
      },
      {
        title: t('SubjectManagement.InactiveSubjects'),
        value: inactiveSubjects,
        icon: <BookOpenIcon className="h-6 w-6 text-yellow-500" />,
        bgColor: 'bg-yellow-50',
      },
    ];
  }, [dataMain, t]);

  //!Functions
  const handleAddEditSubject = async (values: ISubjectForm) => {
    try {
      if (editSubject) {
        await subjectService.updateSubject({
          ...values,
          subjectId: editSubject.subjectId,
        });
        setEditSubject(null);
      } else {
        await subjectService.createSubject(values);
      }
      showSuccess(
        editSubject ? t('SubjectManagement.SubjectUpdated') : t('SubjectManagement.SubjectCreated'),
      );
      toggleAskAddNewSubject();
      refetch();
    } catch (error) {
      showError(error);
    }
  };

  const handleChangePageSize = useCallback(
    (size: number) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          pageSize: size,
          currentPage: 1,
        };
        save(cachedKeys.cachesFilterSubject, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleChangePage = useCallback(
    (page: number) => {
      setTimeout(() => {
        setFilters((prev: any) => {
          const newParams = {
            ...prev,
            currentPage: page,
          };
          save(cachedKeys.cachesFilterSubject, newParams);
          return newParams;
        });
      }, 0);
    },
    [setFilters, save],
  );

  const handleSearch = useCallback(
    (value: IValueFormPageHeader) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          textSearch: value.textSearch ?? '',
          currentPage: 1,
        };
        save(cachedKeys.cachesFilterSubject, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  if (loading) {
    return <Loading />;
  }
  //!Render
  return (
    <PageWrapper name={t('SubjectManagement.Title')} className="bg-white dark:bg-gray-900">
      <ExamHeader
        title={t('SubjectManagement.Title')}
        subtitle={t('SubjectManagement.Subtitle')}
        icon={<Book className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-purple-600 to-blue-700 px-6 py-6 shadow-lg"
      />
      <div className="space-y-6 p-4">
        <UserStats statItems={statItems} className="lg:grid-cols-3" />
        {shouldRenderAskNewSubject && (
          <DialogAddNewSubject
            isOpen={openAskAddNewSubject}
            toggle={handleToggleAskAddNewSubject}
            onSubmit={handleAddEditSubject}
            editSubject={editSubject}
          />
        )}
        <GenericFilters
          className="md:grid-cols-4"
          searchPlaceholder={t('SubjectManagement.SearchPlaceholder')}
          onSearch={handleSearch}
          filters={[
            {
              key: 'status',
              placeholder: t('SubjectManagement.StatusPlaceholder'),
              options: [
                { value: undefined, label: t('SubjectManagement.AllStatuses') },
                { value: true, label: t('SubjectManagement.Active') },
                { value: false, label: t('SubjectManagement.Inactive') },
              ],
            },
          ]}
          initialFilterValues={cachedFiltersSubject ?? {}}
          initialSearchQuery={cachedFiltersSubject?.textSearch ?? ''}
          onFilterChange={(
            newFilters: Record<string, string | number | boolean | null | undefined>,
          ) => {
            setIsTrigger(true);
            setFilters((prev) => {
              const updatedFilters = {
                ...prev,
                ...newFilters,
              };
              save(cachedKeys.cachesFilterSubject, updatedFilters);
              return updatedFilters;
            });
          }}
          onAddNew={toggleAskAddNewSubject}
          addNewButtonText={t('SubjectManagement.AddNewSubject')}
        />
        <MemoizedTablePaging
          columns={columns}
          data={dataMain ?? []}
          currentPage={filters?.currentPage ?? 1}
          currentSize={filters?.pageSize ?? 50}
          totalPage={totalPageSubjectCount ?? 1}
          total={totalSubjectCount ?? 0}
          loading={loading}
          handleChangePage={handleChangePage}
          handleChangeSize={handleChangePageSize}
          noResultText={t('SubjectManagement.NoDataFound')}
        />
      </div>
    </PageWrapper>
  );
};
export default ManageSubject;
