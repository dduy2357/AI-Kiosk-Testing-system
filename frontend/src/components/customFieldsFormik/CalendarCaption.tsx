import { CustomCaption } from './CustomCaption';

// Tách component riêng
interface CaptionProps {
  props: any;
  setMonth: (date: Date) => void;
}

const CalendarCaption = ({ props, setMonth }: CaptionProps) => {
  return <CustomCaption {...props} setMonth={setMonth} />;
};

export default CalendarCaption;
