import { useEffect, useState } from "react";
import { ICampus } from "../interfaces/campus.interface";
import campusService from "../campus.Service";

const useGetListCampus = ({
  isTrigger = true,
}: { isTrigger?: boolean } = {}) => {
  const [data, setData] = useState<ICampus[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const response = await campusService.getCampusList();
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

export default useGetListCampus;
