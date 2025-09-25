import PageWrapper from '@/components/PageWrapper/PageWrapper';
import MemoizedTablePaging from '@/components/tableCommon/v2/tablePaging';
import BaseUrl from '@/consts/baseUrl';
import cachedKeys from '@/consts/cachedKeys';
import { ActiveStatusExamTeacher } from '@/consts/common';
import { showError, showSuccess } from '@/helpers/toast';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useToggleDialog from '@/hooks/useToggleDialog';
import {
  GenericFilters,
  IValueFormPageHeader,
} from '@/pages/admin/manageuser/components/generic-filters';
import { UserStats } from '@/pages/admin/manageuser/components/user-stats';
import httpService, { OTP_DATA_KEY } from '@/services/httpService';
import useGetManageExamList from '@/services/modules/manageexam/hooks/useGetManageExamList';
import {
  IAssignOtpToExam,
  IManageExamRequest,
  ManageExamList,
} from '@/services/modules/manageexam/interfaces/manageExam.interface';
import manageExamService from '@/services/modules/manageexam/manageExam.service';
import { useGet, useSave } from '@/stores/useStores';
import { AlbumIcon } from 'lucide-react';
import moment from 'moment';
import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import ExamHeader from '../examsupervision/components/ExamHeader';
import { getManageExamColumns } from './components/ExamTableColumns';
import DialogAssignOtpExamTeacher from './dialogs/DialogAssignOtp';
import DialogExamDetail from './dialogs/DialogExamDetail';
import { useTranslation } from 'react-i18next';

const ManageExamLecture = () => {
  //!State
  const { t } = useTranslation('shared');
  const navigate = useNavigate();
  const defaultData = useGet('dataExamTeacher');
  const totalExamTeacherCount = useGet('totalExamTeacherCount');
  const totalPageExamTeacherCount = useGet('totalPageExamTeacherCount');
  const cachesFilterExamTeacher = useGet('cachesFilterExamTeacher');
  const forceRefetch = useGet('forceRefetchExamTeacher');
  const [isTrigger, setIsTrigger] = useState(Boolean(!defaultData) ?? forceRefetch);
  const save = useSave();
  const [examId, setExamId] = useState<string | null>(null);
  const [
    isOpenDialogAssignOtpToExamTeacher,
    toggleDialogAssignOtpToExamTeacher,
    shouldRenderDialogAssignOtpToExamTeacher,
  ] = useToggleDialog();
  const roleId = Number(httpService.getUserStorage()?.roleId);

  const [openDialogViewExamDetail, toggleDialogViewExamDetail, shouldRenderDialogViewExamDetail] =
    useToggleDialog();
  const [viewingOtp, setViewingOtp] = useState<IAssignOtpToExam | null>(null);
  const [dataOtps, setDataOtps] = useState<IAssignOtpToExam[]>([]);

  // Load OTP data from localStorage when examId changes
  useEffect(() => {
    const otpDataList: IAssignOtpToExam[] = [];
    // Iterate through all localStorage keys
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key && key.startsWith(`${OTP_DATA_KEY}_`)) {
        const otpData = localStorage.getItem(key);
        if (otpData) {
          try {
            const parsedData = JSON.parse(otpData);
            // Optionally, check if OTP is still valid based on expiration
            const otpExpired = httpService.getOtpExpiredStorage(
              key.replace(`${OTP_DATA_KEY}_`, ''),
            );
            if (otpExpired && moment(otpExpired).isAfter(moment())) {
              otpDataList.push(parsedData);
            } else {
              // Remove expired OTP from localStorage
              localStorage.removeItem(key);
              httpService.saveOtpExpiredStorage(key.replace(`${OTP_DATA_KEY}_`, ''), '');
            }
          } catch (error) {
            console.error(`Failed to parse OTP data for key ${key}:`, error);
          }
        }
      }
    }
    setDataOtps(otpDataList);
  }, []);

  const { filters, setFilters } = useFiltersHandler({
    pageSize: cachesFilterExamTeacher?.pageSize ?? 50,
    currentPage: cachesFilterExamTeacher?.currentPage ?? 1,
    textSearch: '',
    status: cachesFilterExamTeacher?.status ?? undefined,
    isMyQuestion: cachesFilterExamTeacher?.isMyQuestion ?? undefined,
  });

  const stableFilters = useMemo(() => filters as IManageExamRequest, [filters]);

  const {
    data: dataMangeExamTeacher,
    loading,
    refetch,
  } = useGetManageExamList(stableFilters, {
    isTrigger: isTrigger,
    refetchKey: cachedKeys.refetchExamTeacher,
    saveData: true,
  });

  useEffect(() => {
    if (forceRefetch) {
      setIsTrigger(true);
      refetch();
      save(cachedKeys.forceRefetchExamTeacher, false);
    }
  }, [forceRefetch, refetch, save]);

  useEffect(() => {
    if (dataMangeExamTeacher && isTrigger) {
      save(cachedKeys.dataExamTeacher, dataMangeExamTeacher);
    }
  }, [dataMangeExamTeacher, isTrigger, save]);

  const dataMain = useMemo(
    () => (isTrigger ? dataMangeExamTeacher : defaultData),
    [dataMangeExamTeacher, defaultData, isTrigger],
  );

  const statItems = useMemo(() => {
    const totalExams = dataMain?.length ?? 0;
    const totalDrafts = dataMain?.filter(
      (exam: any) => exam.status === ActiveStatusExamTeacher.Draft,
    ).length;
    const totalPublished = dataMain?.filter(
      (exam: any) => exam.status === ActiveStatusExamTeacher.Published,
    ).length;
    const totalFinished = dataMain?.filter(
      (exam: any) => exam.status === ActiveStatusExamTeacher.Finished,
    ).length;

    return [
      {
        title: t('ExamManagement.TotalExam'),
        value: totalExams,
        icon: <AlbumIcon className="h-6 w-6 text-blue-500" />,
        bgColor: 'bg-blue-50',
      },
      {
        title: t('ExamManagement.Published'),
        value: totalPublished,
        icon: <AlbumIcon className="h-6 w-6 text-yellow-500" />,
        bgColor: 'bg-yellow-50',
      },
      {
        title: t('ExamManagement.Finished'),
        value: totalFinished,
        icon: <AlbumIcon className="h-6 w-6 text-red-500" />,
        bgColor: 'bg-red-50',
      },
      {
        title: t('ExamManagement.Draft'),
        value: totalDrafts,
        icon: <AlbumIcon className="h-6 w-6 text-gray-500" />,
        bgColor: 'bg-gray-50',
      },
    ];
  }, [dataMain, t]);

  const handleToggleDetailExam = useCallback(
    (data: ManageExamList) => {
      const userStorage = httpService.getUserStorage();

      if (Number(userStorage?.roleId) === 4) {
        navigate(`${BaseUrl.AdminExamSupervisor}/${data.examId}`);
      } else {
        navigate(`${BaseUrl.ExamSupervisor}/${data.examId}`);
      }
    },
    [navigate],
  );

  const columns = useMemo(
    () =>
      getManageExamColumns({
        navigate,
        setExamId,
        toggleDialogViewExamDetail,
        toggleDialogAssignOtpToExamTeacher,
        dataOtps,
        setViewingOtp,
        roleId,
        handleToggleDetailExam,
        refreshExamList: () => refetch(),
      }),
    [
      navigate,
      setExamId,
      toggleDialogViewExamDetail,
      toggleDialogAssignOtpToExamTeacher,
      dataOtps,
      setViewingOtp,
      roleId,
      handleToggleDetailExam,
      refetch,
    ],
  );

  //!Functions

  const handleToggleDialogAssignOtpToExamTeacher = useCallback(() => {
    toggleDialogAssignOtpToExamTeacher();
    if (!isOpenDialogAssignOtpToExamTeacher) {
      setExamId(null);
      setViewingOtp(null);
    }
  }, [toggleDialogAssignOtpToExamTeacher, isOpenDialogAssignOtpToExamTeacher]);

  const handleChangePageSize = useCallback(
    (size: number) => {
      setIsTrigger(true);
      setFilters((prev) => {
        const newParams = { ...prev, currentPage: 1, pageSize: size };
        save(cachedKeys.cachesFilterExamTeacher, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleChangePage = useCallback(
    (page: number) => {
      setTimeout(() => {
        setFilters((prev) => {
          const newParams = { ...prev, currentPage: page };
          save(cachedKeys.cachesFilterExamTeacher, newParams);
          return newParams;
        });
      }, 0);
    },
    [setFilters, save],
  );

  const handleSearch = useCallback(
    (value: IValueFormPageHeader) => {
      setIsTrigger(true);
      setFilters((prev) => {
        const newParams = { ...prev, textSearch: value.textSearch ?? '', currentPage: 1 };
        save(cachedKeys.cachesFilterExamTeacher, newParams);
        return newParams;
      });
    },
    [setFilters, save],
  );

  const handleAssignOtpToExam = useCallback(
    async (values: IAssignOtpToExam) => {
      try {
        if (!examId) {
          showError(t('ExamManagement.SelectExamError'));
          return;
        }
        const response = await manageExamService.assignOtpToExam({
          ...values,
          examId: examId,
        });
        const newOtpData = response.data.data;
        // Update dataOtps by replacing or adding the new OTP
        setDataOtps((prev) => [...prev.filter((otp) => otp.examId !== examId), newOtpData]);
        httpService.saveOtpDataStorage(examId, JSON.stringify(newOtpData));
        httpService.saveOtpExpiredStorage(examId, newOtpData.expiredAt);
        showSuccess('Gán mã OTP thành công!');
        toggleDialogAssignOtpToExamTeacher();
        setIsTrigger(true);
      } catch (error) {
        showError(error);
      }
    },
    [examId, toggleDialogAssignOtpToExamTeacher, setIsTrigger, t],
  );

  //!Render
  return (
    <PageWrapper name={t('ExamManagement.Title')} className="bg-white dark:bg-gray-900">
      <div className="space-y-6">
        <ExamHeader
          title={t('ExamManagement.Title')}
          subtitle={t('ExamManagement.Subtitle')}
          icon={<AlbumIcon className="h-8 w-8 text-white" />}
          className="border-b border-white/20 bg-gradient-to-r from-blue-600 to-green-700 px-6 py-6 shadow-lg"
        />
        <UserStats statItems={statItems} className="lg:grid-cols-4" />
        <GenericFilters
          className="md:grid-cols-3 lg:grid-cols-5"
          searchPlaceholder={t('ExamManagement.SearchPlaceholder')}
          onSearch={handleSearch}
          filters={[
            {
              key: 'status',
              placeholder: t('ExamManagement.StatusPlaceholder'),
              options: [
                { value: undefined, label: t('ExamManagement.AllStatus') },
                {
                  value: ActiveStatusExamTeacher.Published,
                  label: t('ExamManagement.PublishedStatus'),
                },
                { value: ActiveStatusExamTeacher.Draft, label: t('ExamManagement.DraftStatus') },
                {
                  value: ActiveStatusExamTeacher.Finished,
                  label: t('ExamManagement.FinishedStatus'),
                },
              ],
            },
            {
              key: 'isMyQuestion',
              placeholder: t('ExamManagement.CreatorFilterPlaceholder'),
              options: [
                { value: undefined, label: t('ExamManagement.AllQuestions') },
                { value: true, label: t('ExamManagement.MyQuestion') },
                { value: false, label: t('ExamManagement.NotMyQuestion') },
              ],
            },
          ]}
          onFilterChange={(
            newFilters: Record<string, string | number | boolean | null | undefined>,
          ) => {
            setIsTrigger(true);
            setFilters((prev) => {
              const updatedFilters = {
                ...prev,
                status: Number(newFilters.status) ?? undefined,
                isMyQuestion:
                  newFilters.isMyQuestion === 'true'
                    ? true
                    : newFilters.isMyQuestion === 'false'
                      ? false
                      : newFilters.isMyQuestion,
              };
              save(cachedKeys.cachesFilterExamTeacher, updatedFilters);
              return updatedFilters;
            });
          }}
          onAddNew={() => {
            roleId === 4 ? navigate(BaseUrl.AdminAddNewExam) : navigate(BaseUrl.AddNewExamLecture);
          }}
          addNewButtonText={t('ExamManagement.AddNewExamButton')}
        />
        <MemoizedTablePaging
          columns={columns}
          data={dataMain ?? []}
          currentPage={filters?.currentPage ?? 1}
          currentSize={filters?.pageSize ?? 50}
          totalPage={totalPageExamTeacherCount ?? 1}
          total={totalExamTeacherCount ?? 0}
          loading={loading}
          handleChangePage={handleChangePage}
          handleChangeSize={handleChangePageSize}
        />

        {shouldRenderDialogAssignOtpToExamTeacher && (
          <DialogAssignOtpExamTeacher
            isOpen={isOpenDialogAssignOtpToExamTeacher}
            toggle={handleToggleDialogAssignOtpToExamTeacher}
            onSubmit={handleAssignOtpToExam}
            viewingOtp={viewingOtp}
          />
        )}

        {shouldRenderDialogViewExamDetail && (
          <DialogExamDetail
            isOpen={openDialogViewExamDetail}
            toggle={toggleDialogViewExamDetail}
            examId={examId ?? ''}
          />
        )}
      </div>
    </PageWrapper>
  );
};

export default ManageExamLecture;
