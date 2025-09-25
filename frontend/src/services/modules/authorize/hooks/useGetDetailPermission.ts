import { PermissionFormValues } from "@/pages/admin/manageuser/dialogs/DialogAddNewPermision";
import { useEffect, useState, useCallback } from "react";
import authorizeService from "@/services/modules/authorize/role.Service";

const useGetDetailPermission = (id: string) => {
  const [data, setData] = useState<PermissionFormValues[]>([]);
  const [loading, setLoading] = useState<boolean>(false);

  const fetchData = useCallback(async () => {
    if (!id) {
      return;
    }
    setLoading(true);
    try {
      const response = await authorizeService.getDetailPermission(id);
      setData(response.data.data);
    } catch (error) {
      console.error("Error fetching permission details:", error);
    } finally {
      setLoading(false);
    }
  }, [id]);

  useEffect(() => {
    if (id) {
      fetchData();
    }
  }, [id, fetchData]);

  return {
    data,
    loading,
    refetch: fetchData,
  };
};

export default useGetDetailPermission;
