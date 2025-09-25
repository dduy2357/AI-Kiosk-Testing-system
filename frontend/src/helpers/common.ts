import { LANG_ENUM, PERMISSION_ENUM } from '@/consts/common';
import { DateTimeFormat } from '@/consts/dates';
import { KEY_LANG } from '@/i18n/config';
import { Variants } from 'framer-motion';
import { toString } from 'lodash';
import moment, { Moment } from 'moment';

export const momentInstance = moment;

export const sleepTime = (ms: number) => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve('');
    }, ms);
  });
};

export const showFormatDateFollowCurrentLang = (
  date: string | Moment,
  currentFormat?: string,
  targetFormatEn?: string,
  targetFormatVi?: string,
) => {
  const currentLng = localStorage.getItem(KEY_LANG) ?? LANG_ENUM.en;
  if (currentLng === LANG_ENUM.en) {
    return moment(date, currentFormat ?? undefined).format(
      targetFormatEn ?? DateTimeFormat.MDYFormat,
    );
  }
  if (currentLng === LANG_ENUM.vi) {
    return moment(date, currentFormat ?? undefined).format(
      targetFormatVi ?? DateTimeFormat.VNFormat,
    );
  }
};

export const addLeadingZero = (number: number) => {
  const stringNumber = `${number}`.padStart(2, '0');
  return stringNumber;
};

export const isDefine = (value: any) => !!toString(value);

export function a11yProps(index: number) {
  return {
    id: `simple-tab-${index}`,
    'aria-controls': `simple-tabpanel-${index}`,
  };
}

export const convertToFormSelect = (
  list: any[],
  fieldForLabel: string | number | undefined = undefined,
  fieldForValue: string | number | undefined = undefined,
  noneOption: boolean | undefined = false,
) => {
  if (!fieldForLabel || !fieldForValue) {
    return [
      ...list.reduce((arr: any, el: any) => {
        return [...arr, { label: el, value: el }];
      }, []),
    ];
  }
  if (typeof list === 'object' && list) {
    const listReturn = [
      ...list.reduce((arr: any, el: any) => {
        return [
          ...arr,
          {
            ...el,
            label: el[fieldForLabel] ?? 'None',
            value: el[fieldForValue] ?? '',
          },
        ];
      }, []),
    ];

    if (noneOption) {
      return [{ label: 'None', value: '' }, ...listReturn];
    }
    return listReturn;
  }
  return [{ label: 'None', value: '' }, ...list];
};

export const getNameRole = (role: string) => {
  let result = '';

  Object.entries(PERMISSION_ENUM).forEach((el) => {
    const [key, value] = el;
    if (role === value) {
      result = key;
    }
  });

  return result;
};

export const isPromise = (value: any) => {
  return Boolean(value && typeof value.then === 'function');
};

export const containerVariants: Variants = {
  hidden: { opacity: 0 },
  visible: {
    opacity: 1,
    transition: {
      staggerChildren: 0.1,
    },
  },
};

export const itemVariants: Variants = {
  hidden: { opacity: 0, y: 20 },
  visible: {
    opacity: 1,
    y: 0,
    transition: {
      type: 'spring',
      stiffness: 100,
      damping: 10,
    },
  },
};

export const formatTime = (seconds: number): string => {
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  return `${minutes}m : ${remainingSeconds.toString().padStart(2, '0')}s`;
};

export const convertUTCToVietnamTime = (
  utcDate: string | moment.Moment | Date | null | undefined,
  format?: DateTimeFormat,
): string | moment.Moment => {
  try {
    // Parse UTC date and convert to Vietnam time (+07:00)
    const vietnamTime = moment.utc(utcDate).utcOffset('+07:00');
    // Return formatted string if format is provided, otherwise return moment object
    return format ? vietnamTime.format(format) : vietnamTime;
  } catch (error) {
    console.error(`Failed to convert UTC to Vietnam time: ${utcDate}`, error);
    return '';
  }
};

export const formatTimeAgo = (date: string | Date) => {
  // Convert input date to Vietnam time
  const vietnamTime = convertUTCToVietnamTime(date);
  if (typeof vietnamTime === 'string' || !vietnamTime) {
    return 'Invalid date';
  }

  const now = moment().utcOffset('+07:00');
  const diffInSeconds = Math.floor(now.diff(vietnamTime) / 1000);

  if (diffInSeconds < 60) {
    return `${diffInSeconds} giây trước`;
  } else if (diffInSeconds < 3600) {
    const minutes = Math.floor(diffInSeconds / 60);
    return `${minutes} phút trước`;
  } else if (diffInSeconds < 86400) {
    const hours = Math.floor(diffInSeconds / 3600);
    return `${hours} giờ trước`;
  } else {
    const days = Math.floor(diffInSeconds / 86400);
    return `${days} ngày trước`;
  }
};

export const normalizeRoleId = (roleId: any): number[] => {
  if (Array.isArray(roleId)) {
    return roleId.map((r) => parseInt(r, 10)).filter((r) => !isNaN(r));
  }
  const parsed = parseInt(roleId, 10);
  return !isNaN(parsed) ? [parsed] : [];
};

export const formatDateToDMY = (date: Date): string => {
  return `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}`;
};

export const getQuestionButtonClass = (
  isCurrent: boolean,
  isSelected: boolean,
  isMarked: boolean,
): string => {
  const baseClasses = 'transition-colors duration-200';
  if (isCurrent) {
    if (isMarked) {
      return `${baseClasses} bg-yellow-100 text-gray-900 ring-2 ring-blue-500`;
    }
    if (isSelected) {
      return `${baseClasses} bg-blue-500 text-white ring-2 ring-blue-500`;
    }
    return `${baseClasses} border-gray-300 ring-2 ring-blue-500`;
  }
  if (isMarked) {
    return `${baseClasses} bg-yellow-100 text-gray-900`;
  }
  if (isSelected) {
    return `${baseClasses} bg-blue-500 text-white`;
  }
  return `${baseClasses} border-gray-300`;
};

export const getFromStorage = (key: string) => {
  const dataLocal = localStorage.getItem(key);
  const dataSession = sessionStorage.getItem(key);

  const data = dataLocal ?? dataSession;
  const from = data ? (dataLocal ? 'localStorage' : 'sessionStorage') : 'notfound';
  return {
    data,
    from,
  };
};

export const setToStorage = (key: string, data: any, to: string) => {
  if (to === 'localStorage') {
    localStorage.setItem(key, data);
  }
  if (to === 'sessionStorage') {
    sessionStorage.setItem(key, data);
  }
};

export const formatScore = (score: number) => {
  if (score > 10) return 10;
  return Math.round(score * 10) / 10;
};
