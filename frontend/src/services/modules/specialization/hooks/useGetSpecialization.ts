import { useEffect, useState } from "react";
import { ISpecialization } from "../interfaces/specialization.interface";
import specializationService from "../specialization.service";

const useGetListSpecialization = ({
  isTrigger = true,
}: { isTrigger?: boolean } = {}) => {
  const [data, setData] = useState<ISpecialization[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const response = await specializationService.getListSpecializations();
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

export default useGetListSpecialization;
