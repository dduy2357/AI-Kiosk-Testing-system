import React, { useEffect, useState } from 'react';

interface CountUpProps {
  end: number;
  duration?: number; // Duration in milliseconds
}

const CountUp: React.FC<CountUpProps> = ({ end, duration = 1.2 }) => {
  const [count, setCount] = useState(0);

  useEffect(() => {
    let start = 0;
    const increment = end / (duration / 16);
    const stepTime = Math.max(16, duration / end);

    const timer = setInterval(() => {
      start += increment;
      if (start >= end) {
        setCount(end);
        clearInterval(timer);
      } else {
        setCount(Math.ceil(start));
      }
    }, stepTime);

    return () => clearInterval(timer);
  }, [end, duration]);

  return <span>{count}</span>;
};

export default CountUp;
