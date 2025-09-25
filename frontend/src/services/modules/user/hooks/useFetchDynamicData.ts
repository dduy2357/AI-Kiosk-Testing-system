import { useState, useEffect, useRef } from "react";

const useFetchDynamicData = (
  fetchFunction: () => Promise<any>,
  dependency: any,
) => {
  const [data, setData] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const hasFetchedRef = useRef(false);

  useEffect(() => {
    const fetchData = async () => {
      if (!hasFetchedRef.current) {
        try {
          setLoading(true);
          const response = await fetchFunction();
          setData(response.data);
        } catch (error) {
          console.error("Failed to fetch data:", error);
        } finally {
          setLoading(false);
          hasFetchedRef.current = true;
        }
      }
    };

    fetchData();
  }, [fetchFunction, dependency]);

  return { data, loading };
};

export default useFetchDynamicData;
