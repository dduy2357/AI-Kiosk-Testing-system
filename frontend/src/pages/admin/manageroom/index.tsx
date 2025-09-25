import PageWrapper from '@/components/PageWrapper/PageWrapper';
import TablePaging from '@/components/tableCommon/v2/tablePaging';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Switch } from '@/components/ui/switch';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import ExamHeader from '@/pages/teacher/examsupervision/components/ExamHeader';
import httpService from '@/services/httpService';
import useGetListAllRooms from '@/services/modules/room/hooks/useGetAllRooms';
import { IRoomRequest, RoomList } from '@/services/modules/room/interfaces/room.interface';
import roomService from '@/services/modules/room/room.service';
import { useGet, useSave } from '@/stores/useStores';
import { Edit, Eye, Home, HomeIcon, MoreHorizontal } from 'lucide-react';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { GenericFilters, IValueFormPageHeader } from '../manageuser/components/generic-filters';
import { UserStats } from '../manageuser/components/user-stats';
import DialogAddNewRoom, { RoomFormValues } from './dialogs/DialogAddNewRoom';

const ManageRoom = () => {
  //!State
  const { t } = useTranslation('shared');
  const [openAskAddNewRoom, toggleAskAddNewRoom, shouldRenderAskNewRoom] = useToggleDialog();
  const defaultRoom = useGet('dataRoom');
  const totalRoomCount = useGet('totalRoomCount');
  const totalPageRoomCount = useGet('totalPageRoomCount');
  const cachesFilterRoom = useGet('cachesFilterRoom');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultRoom));
  const save = useSave();
  const [editRoom, setEditRoom] = useState<RoomList | null>(null);
  const navigate = useNavigate();
  const roleId = httpService.getUserStorage()?.roleId;

  const { filters, setFilters } = useFiltersHandler({
    PageSize: cachesFilterRoom?.PageSize ?? 50,
    CurrentPage: cachesFilterRoom?.CurrentPage ?? 1,
    TextSearch: cachesFilterRoom?.TextSearch ?? '',
    IsActive: cachesFilterRoom?.IsActive !== undefined ? cachesFilterRoom?.IsActive : null,
  });

  const stableFilters = useMemo(() => filters as IRoomRequest, [filters]);

  const {
    data: dataRooms,
    loading,
    refetch,
  } = useGetListAllRooms(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachesFilterRoom?.refetchKey,
    saveData: true,
  });

  useEffect(() => {
    if (dataRooms && isTrigger) {
      save(cachedKeys.dataRoom, dataRooms);
    }
  }, [dataRooms, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? dataRooms : defaultRoom),
    [dataRooms, defaultRoom, isTrigger],
  );

  const columns = [
    {
      label: t('ExamRoomManagement.RoomCode'),
      accessor: 'roomCode',
      sortable: false,
      Cell: (row: RoomList) => <span className="font-medium">{row.roomCode}</span>,
    },
    {
      label: t('ExamRoomManagement.ClassCode'),
      accessor: 'classCode',
      sortable: false,
      Cell: (row: RoomList) => <span className="font-medium">{row.classCode}</span>,
    },
    {
      label: t('ExamRoomManagement.RoomDescription'),
      accessor: 'roomDescription',
      sortable: false,
      Cell: (row: RoomList) => (
        <span className="text-sm text-gray-600">{row.roomDescription ?? 'N/A'}</span>
      ),
    },
    {
      label: 'Số lượng học sinh',
      accessor: 'roomMaxStudent',
      sortable: false,
      Cell: (row: RoomList) => (
        <span className="text-sm text-gray-600">{row.capacity ?? 'N/A'}</span>
      ),
    },
    {
      label: t('ExamRoomManagement.Subject'),
      accessor: 'subjectName',
      sortable: false,
      Cell: (row: RoomList) => <span className="font-medium">{row.subjectName ?? 'N/A'}</span>,
    },
    {
      label: t('ExamRoomManagement.SubjectDescription'),
      accessor: 'subjectDescription',
      sortable: false,
      Cell: (row: RoomList) => (
        <span className="text-sm text-gray-600">{row.subjectDescription ?? 'N/A'}</span>
      ),
    },
    {
      label: t('ExamRoomManagement.Status'),
      accessor: 'status',
      sortable: false,
      Cell: (row: RoomList) => (
        <span
          className={`rounded-full px-2 py-1 text-xs font-medium ${
            row.isRoomActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
          }`}
        >
          {row.isRoomActive ? t('ExamRoomManagement.Active') : t('ExamRoomManagement.Inactive')}
        </span>
      ),
    },
    {
      label: t('ExamRoomManagement.ActiveDeactive'),
      accessor: 'isRoomActive',
      // width: 120,
      sortable: false,
      Cell: (row: RoomList) => (
        <Switch
          checked={row.isRoomActive}
          onCheckedChange={async () => {
            try {
              await roomService.changeActiveRoom(row.roomId);
              showSuccess(`Room ${row.isRoomActive ? 'deactivated' : 'activated'} successfully!`);
              refetch();
            } catch (error) {
              showError(error);
            }
          }}
          className="h-6 w-11"
          disabled={loading}
        />
      ),
    },
    {
      label: t('ExamRoomManagement.Actions'),
      accessor: 'actions',
      width: 120,
      sortable: false,
      Cell: (row: RoomList) => (
        <div>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" className="h-8 w-8 p-0">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem
                onClick={() => handleOnclickDetail(row.roomId)}
                className="cursor-pointer text-blue-600"
              >
                <Eye className="mr-2 h-4 w-4" />
                <span>{t('ExamRoomManagement.ViewDetails')}</span>
              </DropdownMenuItem>
              <DropdownMenuItem
                onClick={() => {
                  setEditRoom(row);
                  toggleAskAddNewRoom();
                }}
                className="cursor-pointer text-green-600"
              >
                <Edit className="mr-2 h-4 w-4" /> {t('Edit')}
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </div>
      ),
    },
  ];

  const handleToggleEditRoom = useCallback(() => {
    toggleAskAddNewRoom();
    if (openAskAddNewRoom) {
      setEditRoom(null);
    }
  }, [toggleAskAddNewRoom, openAskAddNewRoom]);

  const statItems = useMemo(() => {
    const totalRoom = dataMain.length ?? 0;
    const activeRooms = dataMain.filter((room: RoomList) => room.isRoomActive).length ?? 0;
    const inactiveRooms = totalRoom - activeRooms;

    return [
      {
        title: t('ExamRoomManagement.TotalRooms'),
        value: totalRoom,
        icon: <Home className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
      {
        title: t('ExamRoomManagement.ActiveRooms'),
        value: activeRooms,
        icon: <Home className="h-6 w-6 text-green-500" />,
        bgColor: 'bg-green-50',
      },
      {
        title: t('ExamRoomManagement.InactiveRooms'),
        value: inactiveRooms,
        icon: <Home className="h-6 w-6 text-yellow-500" />,
        bgColor: 'bg-yellow-50',
      },
    ];
  }, [dataMain, t]);

  //!Functions
  const handleChangePageSize = useCallback(
    (size: number) => {
      setIsTrigger(true);
      setFilters((prev: any) => {
        const newParams = {
          ...prev,
          CurrentPage: 1,
          PageSize: size,
        };
        save(cachedKeys.cachesFilterClass, newParams);
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
            CurrentPage: page,
          };
          save(cachedKeys.cachesFilterClass, newParams);
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
          TextSearch: value.textSearch ?? '',
          CurrentPage: 1,
        };
        save(cachedKeys.cachesFilterClass, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleAddEditRoom = async (value: RoomFormValues) => {
    try {
      if (editRoom) {
        await roomService.createRoom({
          ...value,
          roomId: editRoom.roomId,
        });
        setEditRoom(null);
        refetch();
      } else {
        await roomService.createRoom({
          ...value,
          roomId: '',
        });
        refetch();
      }
      toggleAskAddNewRoom();
      showSuccess(
        editRoom
          ? t('ExamRoomManagement.RoomUpdatedSuccessfully')
          : t('ExamRoomManagement.RoomCreatedSuccessfully'),
      );
      // refetch();
    } catch (error) {
      showError(error);
    }
  };

  const handleOnclickDetail = (roomId: string) => {
    if (Number(roleId) === 4) {
      navigate(`${BaseUrl.AdminManageRoom}/${roomId}`);
      return;
    }
    navigate(`${BaseUrl.TeacherManageRoom}/${roomId}`);
  };

  //!Render
  return (
    <PageWrapper name="Quản lý phòng thi" className="bg-white dark:bg-gray-900">
      <ExamHeader
        title={t('ExamRoomManagement.Title')}
        subtitle={t('ExamRoomManagement.Subtitle')}
        icon={<HomeIcon className="h-8 w-8 text-white" />}
        className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
      />
      <div className="space-y-6 p-4">
        <UserStats statItems={statItems} className="lg:grid-cols-3" />
        {shouldRenderAskNewRoom && (
          <DialogAddNewRoom
            isOpen={openAskAddNewRoom}
            toggle={handleToggleEditRoom}
            onSubmit={handleAddEditRoom}
            editRoom={editRoom}
          />
        )}
        <GenericFilters
          className="md:grid-cols-4"
          searchPlaceholder={t('ExamRoomManagement.SearchPlaceholder')}
          onSearch={handleSearch}
          initialSearchQuery={filters.TextSearch ?? ''}
          filters={[
            {
              key: 'IsActive',
              placeholder: t('ExamRoomManagement.Status'),
              options: [
                { value: null, label: t('All') },
                { value: true, label: t('ExamRoomManagement.ActiveRooms') },
                { value: false, label: t('ExamRoomManagement.InactiveRooms') },
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
              save(cachedKeys.cachesFilterClass, newParams);
              return newParams;
            });
          }}
          onAddNew={toggleAskAddNewRoom}
          addNewButtonText={t('ExamRoomManagement.AddNewRoom')}
        />
        <TablePaging
          columns={columns}
          data={dataMain ?? []}
          keyRow="roomId"
          loading={loading}
          currentPage={filters.CurrentPage ?? 1}
          currentSize={filters.PageSize ?? 50}
          totalPage={totalPageRoomCount ?? 1}
          total={totalRoomCount ?? 0}
          handleChangePage={handleChangePage}
          handleChangeSize={handleChangePageSize}
          noResultText={t('NoDataFound')}
        />
      </div>
    </PageWrapper>
  );
};

export default ManageRoom;
