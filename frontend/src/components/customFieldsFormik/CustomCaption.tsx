import { CaptionProps } from "react-day-picker";

type CustomCaptionProps = CaptionProps & {
  setMonth: (date: Date) => void;
};

export function CustomCaption({ displayMonth, setMonth }: CustomCaptionProps) {
  const currentYear = new Date().getFullYear();
  const years = Array.from({ length: 125 }, (_, i) => currentYear - i);
  const months = Array.from({ length: 12 }, (_, i) =>
    new Date(0, i).toLocaleString("default", { month: "long" }),
  );

  const handleMonthChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newMonth = Number(e.target.value);
    setMonth(new Date(displayMonth.getFullYear(), newMonth));
  };

  const handleYearChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newYear = Number(e.target.value);
    setMonth(new Date(newYear, displayMonth.getMonth()));
  };

  return (
    <div className="flex items-center justify-between px-3 py-2">
      <select
        className="rounded border p-1"
        value={displayMonth.getMonth()}
        onChange={handleMonthChange}
      >
        {months.map((month, idx) => (
          <option key={month} value={idx}>
            {month}
          </option>
        ))}
      </select>
      <select
        className="rounded border p-1"
        value={displayMonth.getFullYear()}
        onChange={handleYearChange}
      >
        {years.map((year) => (
          <option key={year}>{year}</option>
        ))}
      </select>
    </div>
  );
}
