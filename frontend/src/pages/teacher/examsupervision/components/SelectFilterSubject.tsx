import FormikField from '@/components/customFieldsFormik/FormikField';
import SelectField from '@/components/customFieldsFormik/SelectField';
import Timer from '@/helpers/timer';
import useFiltersHandler from '@/hooks/useFiltersHandler';
import useGetListSubject from '@/services/modules/subject/hooks/useGetAllSubject';
import { useMemo, useState } from 'react';
import { useTranslation } from 'react-i18next';

export interface ISelectPlace {
  name: string;
  defaultValue?: string;
  defaultSubject?: {
    label: string;
    value: string;
  };
  onChange?: () => Promise<void>; // Add onChange to the interface
}

const timer = new Timer();

const SelectFilterSubject = ({ name, defaultValue, defaultSubject, onChange }: ISelectPlace) => {
  const { t } = useTranslation('shared');
  const [openSelect, setOpenSelect] = useState(false);

  const { filters, handleChangePage, setFilters } = useFiltersHandler({
    currentPage: 1,
    pageSize: 10,
    textSearch: '',
    status: true,
  });

  const { data, loading, loadingMore, hasMore, refetching } = useGetListSubject(filters, {
    isTrigger: openSelect,
    isLoadmore: true,
    saveData: false,
  });

  const options = useMemo(() => {
    const dataOption = data?.map((e) => ({
      label: e?.subjectName,
      value: e?.subjectId,
    }));
    return defaultSubject ? [defaultSubject, ...dataOption] : dataOption;
  }, [data, defaultSubject]);

  return (
    <FormikField
      name={name}
      component={SelectField}
      defaultValue={defaultValue}
      afterOnChange={onChange}
      onSearchAPI={(values: any) => {
        timer.debounce(() => {
          setFilters({
            currentPage: 1,
            pageSize: 10,
            textSearch: values,
            status: true,
          });
        }, 500);
      }}
      onToggle={(open: boolean) => {
        setOpenSelect(open);
        if (!open) {
          setFilters({
            currentPage: 1,
            pageSize: 10,
            textSearch: '',
            status: true,
          });
        }
      }}
      shouldHideSearch
      placeholder={t('ExamSupervision.SelectSubject') ?? 'Chọn môn học'}
      options={options}
      loadingMore={loadingMore}
      loading={loading ?? refetching}
      hasMore={hasMore}
      messageItemNotFound={''}
      handleLoadMore={async () => {
        handleChangePage(filters.currentPage + 1);
      }}
    />
  );
};

export default SelectFilterSubject;
