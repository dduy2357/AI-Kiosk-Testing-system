import { useEffect, useState } from "react";
import { IMajor } from "../interfaces/major.interface";
import majorService from "../major.Service";

const useGetListMajor = ({
  isTrigger = true,
}: { isTrigger?: boolean } = {}) => {
  const [data, setData] = useState<IMajor[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const response = await majorService.getMajorList();
      setData(response.data);
    } catch (error) {
      console.error("Failed to fetch majors:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (isTrigger) {
      fetchData();
    }
  }, [isTrigger]);

  return { data, loading, refetch: fetchData };
};

export default useGetListMajor;
