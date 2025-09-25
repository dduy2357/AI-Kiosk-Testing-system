import { useCallback, useEffect, useState } from "react";
import { IPosition } from "../interfaces/position.interface";
import positionService from "../position.service";

const useGetListPosition = ({
  isTrigger = true,
  departmentId = "",
}: { isTrigger?: boolean; departmentId?: string } = {}) => {
  const [data, setData] = useState<IPosition[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchData = useCallback(async () => {
    setLoading(true);
    try {
      const response = await positionService.getListPosition(departmentId);
      setData(response.data);
    } catch (error) {
      console.error("Failed to fetch majors:", error);
    } finally {
      setLoading(false);
    }
  }, [departmentId]);

  useEffect(() => {
    if (isTrigger) {
      fetchData();
    }
  }, [isTrigger, fetchData]);

  return { data, loading, refetch: fetchData };
};

export default useGetListPosition;
