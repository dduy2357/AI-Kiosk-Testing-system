import { momentInstance } from '@/helpers/common';
import type { AdditionalFormikProps } from '@/interfaces/common';
import { cn } from '@/lib/utils';
import { get, isString } from 'lodash';
import type React from 'react';
import { useState } from 'react';
import type { SelectSingleEventHandler } from 'react-day-picker';
import { twMerge } from 'tailwind-merge';
import CommonIcons from '../commonIcons';
import { Button } from '../ui/button';
import { Calendar } from '../ui/calendar';
import type { InputProps } from '../ui/input';
import { Label } from '../ui/label';
import { Popover, PopoverContent, PopoverTrigger } from '../ui/popover';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../ui/select';
import CalendarCaption from './CalendarCaption';

interface DateTimePickerFieldProps extends InputProps {
  label?: string | React.ReactNode;
  required?: boolean;
  classNameLabel?: string;
  classNameContainer?: string;
  afterOnChange?: (date: Date | undefined) => void;
  hideDatePicker?: boolean;
  hideTimePicker?: boolean;
  disableCallback?: (date: Date) => boolean;
  timeFormat?: '12' | '24'; // Add support for 12/24 hour format
}

const DateTimePickerField = (props: DateTimePickerFieldProps & AdditionalFormikProps) => {
  //! State
  const {
    label,
    classNameLabel,
    classNameContainer,
    form,
    field,
    className,
    required,
    afterOnChange,
    disableCallback,
    timeFormat = '24',
  } = props;

  const [calendarOpen, setCalendarOpen] = useState(false);
  const { name, value } = field;
  const { errors, touched, setFieldTouched, setFieldValue } = form;
  const msgError = get(touched, name) && (get(errors, name) as string);

  //! Function
  const onHandleChange: SelectSingleEventHandler = (date) => {
    const nextDate = date;
    if (value) {
      const prevDate = momentInstance(value).toDate();
      nextDate?.setHours(prevDate.getHours());
      nextDate?.setMinutes(prevDate.getMinutes());
      nextDate?.setSeconds(prevDate.getSeconds());
      nextDate?.setMilliseconds(prevDate.getMilliseconds());
    } else {
      nextDate?.setHours(new Date().getHours());
      nextDate?.setMinutes(new Date().getMinutes());
      nextDate?.setSeconds(new Date().getSeconds());
      nextDate?.setMilliseconds(new Date().getMilliseconds());
    }
    afterOnChange && afterOnChange(nextDate);
    setFieldValue(name, nextDate);
  };

  const onTimeChange = (type: 'hour' | 'minute', newValue: string) => {
    const currentDate = value ? momentInstance(value).toDate() : new Date();
    const nextDate = new Date(currentDate);

    if (type === 'hour') {
      nextDate.setHours(Number.parseInt(newValue));
    } else {
      nextDate.setMinutes(Number.parseInt(newValue));
    }

    afterOnChange && afterOnChange(nextDate);
    setFieldValue(name, nextDate);
    setFieldTouched(name, true);
  };

  const [currentMonth, setCurrentMonth] = useState<Date | undefined>(value ?? new Date());

  // Generate hour options
  const hourOptions = Array.from({ length: timeFormat === '12' ? 12 : 24 }, (_, i) => {
    let hour = i;
    let value = i;

    if (timeFormat === '12') {
      hour = i === 0 ? 12 : i;
      value = i === 0 ? 0 : i;
    }

    return {
      value: value.toString(),
      label: hour.toString().padStart(2, '0'),
    };
  });

  // Generate minute options (every 5 minutes)
  const minuteOptions = Array.from({ length: 12 }, (_, i) => {
    const minute = i * 5;
    return {
      value: minute.toString(),
      label: minute.toString().padStart(2, '0'),
    };
  });

  const getCurrentHour = () => {
    if (!value) return '';

    const hour = momentInstance(value).hour();

    if (timeFormat === '12') {
      let displayHour = hour;
      if (hour === 0) displayHour = 0;
      else if (hour > 12) displayHour = hour - 12;
      return displayHour.toString();
    }

    return hour.toString();
  };

  const getCurrentMinute = () => {
    if (!value) return '';
    const minute = momentInstance(value).minute();
    // Round to nearest 5 minutes
    return (Math.round(minute / 5) * 5).toString();
  };

  const getAMPM = () => {
    if (!value || timeFormat === '24') return '';
    return momentInstance(value).hour() >= 12 ? 'PM' : 'AM';
  };

  const onAMPMChange = (period: string) => {
    if (!value) return;
    const currentDate = momentInstance(value).toDate();
    const nextDate = new Date(currentDate);
    const currentHour = nextDate.getHours();

    if (period === 'AM' && currentHour >= 12) {
      nextDate.setHours(currentHour - 12);
    } else if (period === 'PM' && currentHour < 12) {
      nextDate.setHours(currentHour + 12);
    }

    afterOnChange && afterOnChange(nextDate);
    setFieldValue(name, nextDate);
    setFieldTouched(name, true);
  };

  //! Render
  return (
    <div className={twMerge('grid w-full items-center gap-1.5', classNameContainer)}>
      {label && (
        <Label htmlFor={name} className={twMerge('mb-1', required && 'required', classNameLabel)}>
          {label}
        </Label>
      )}

      <div className={twMerge('flex gap-2', className)}>
        {!props.hideDatePicker && (
          <Popover
            open={calendarOpen}
            onOpenChange={(open) => {
              if (!open) {
                setFieldTouched(name, true);
              }
              setCalendarOpen(open);
            }}
          >
            <PopoverTrigger className="flex-1" asChild>
              <Button
                variant="outline"
                className={cn(
                  'w-full min-w-[200px] flex-1 pl-3 text-left font-normal',
                  !value && 'text-muted-foreground',
                  msgError && 'border-red-500',
                )}
              >
                {value ? (
                  <span>
                    {momentInstance(value).format('ll')}
                    {!props.hideTimePicker && (
                      <span className="ml-2 text-sm text-muted-foreground">
                        {momentInstance(value).format(timeFormat === '12' ? 'hh:mm A' : 'HH:mm')}
                      </span>
                    )}
                  </span>
                ) : (
                  <span>Pick a date{!props.hideTimePicker && ' and time'}</span>
                )}
                <CommonIcons.CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0" align="start">
              <Calendar
                mode="single"
                selected={value}
                onSelect={onHandleChange}
                month={currentMonth}
                onMonthChange={(newMonth) => setCurrentMonth(newMonth)}
                components={{
                  Caption: (captionProps) => (
                    <CalendarCaption props={captionProps} setMonth={setCurrentMonth} />
                  ),
                }}
                disabled={disableCallback}
              />

              {!props.hideTimePicker && (
                <div className="border-t p-3">
                  <div className="flex items-center gap-2">
                    <Label className="text-sm font-medium">Time:</Label>

                    {/* Hour Selector */}
                    <Select
                      value={getCurrentHour()}
                      onValueChange={(value) => onTimeChange('hour', value)}
                    >
                      <SelectTrigger className="w-16">
                        <SelectValue placeholder="HH" />
                      </SelectTrigger>
                      <SelectContent className="max-h-[200px]">
                        {hourOptions.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>

                    <span className="text-sm">:</span>

                    {/* Minute Selector */}
                    <Select
                      value={getCurrentMinute()}
                      onValueChange={(value) => onTimeChange('minute', value)}
                    >
                      <SelectTrigger className="w-16">
                        <SelectValue placeholder="MM" />
                      </SelectTrigger>
                      <SelectContent className="max-h-[200px]">
                        {minuteOptions.map((option) => (
                          <SelectItem key={option.value} value={option.value}>
                            {option.label}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>

                    {/* AM/PM Selector for 12-hour format */}
                    {timeFormat === '12' && (
                      <Select value={getAMPM()} onValueChange={onAMPMChange}>
                        <SelectTrigger className="w-16">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent className="max-h-[200px]">
                          <SelectItem value="AM">AM</SelectItem>
                          <SelectItem value="PM">PM</SelectItem>
                        </SelectContent>
                      </Select>
                    )}
                  </div>
                </div>
              )}
            </PopoverContent>
          </Popover>
        )}

        {/* Standalone Time Picker (when date picker is hidden) */}
        {props.hideDatePicker && !props.hideTimePicker && (
          <div className="flex items-center gap-2 rounded-md border px-3 py-2">
            <CommonIcons.Clock className="h-4 w-4 opacity-50" />

            <Select value={getCurrentHour()} onValueChange={(value) => onTimeChange('hour', value)}>
              <SelectTrigger className="h-auto w-16 border-0 p-0">
                <SelectValue placeholder="HH" />
              </SelectTrigger>
              <SelectContent className="max-h-[200px]">
                {hourOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <span>:</span>

            <Select
              value={getCurrentMinute()}
              onValueChange={(value) => onTimeChange('minute', value)}
            >
              <SelectTrigger className="h-auto w-16 border-0 p-0">
                <SelectValue placeholder="MM" />
              </SelectTrigger>
              <SelectContent className="max-h-[200px]">
                {minuteOptions.map((option) => (
                  <SelectItem key={option.value} value={option.value}>
                    {option.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {timeFormat === '12' && (
              <Select value={getAMPM()} onValueChange={onAMPMChange}>
                <SelectTrigger className="h-auto w-16 border-0 p-0">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent className="max-h-[200px]">
                  <SelectItem value="AM">AM</SelectItem>
                  <SelectItem value="PM">PM</SelectItem>
                </SelectContent>
              </Select>
            )}
          </div>
        )}

        <Button variant="outline" onClick={() => setFieldValue(name, undefined)}>
          <CommonIcons.X className="ml-auto h-4 w-4 opacity-50" />
        </Button>
      </div>

      {isString(msgError) && <span className="invalid-text">{msgError}</span>}
    </div>
  );
};

export default DateTimePickerField;
