import { useEffect, useState } from "react";
import { IDepartment } from "../interfaces/department.interface";
import departmentService from "../department.Service";

const useGetListDepartment = ({
  isTrigger = true,
}: { isTrigger?: boolean } = {}) => {
  const [data, setData] = useState<IDepartment[]>([]);
  const [loading, setLoading] = useState(false);

  const fetchData = async () => {
    setLoading(true);
    try {
      const response = await departmentService.getListDepartments();
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

export default useGetListDepartment;
