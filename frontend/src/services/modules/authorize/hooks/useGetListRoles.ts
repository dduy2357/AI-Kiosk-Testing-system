import { showError } from '@/helpers/toast';
import { useCallback, useEffect, useState } from 'react';
import { Datum } from '../interfaces/role.interface';
import authorizeService from '../role.Service';

const useGetListRoles = () => {
  //!State
  const [data, setData] = useState<Datum[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await authorizeService.getListRoles();
      setData(response.data.data);
    } catch (error) {
      showError(error);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  //!Render
  return {
    data,
    loading,
    fetchData,
  };
};

export default useGetListRoles;
