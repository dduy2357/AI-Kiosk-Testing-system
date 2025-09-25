import { useEffect, useState } from 'react';

const useTimer = (initialTime: number) => {
  const [timeRemaining, setTimeRemaining] = useState(initialTime);

  useEffect(() => {
    const timer = setInterval(() => {
      setTimeRemaining((prev) => (prev <= 0 ? (clearInterval(timer), 0) : prev - 1));
    }, 1000);
    return () => clearInterval(timer);
  }, []);

  return timeRemaining;
};

export default useTimer;
