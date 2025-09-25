export const LANG_ENUM = {
  vi: 'vi',
  en: 'en',
};

export enum PERMISSION_ENUM {
  PUBLIC = 'PUBLIC',
  ADMIN = 'admin',
  STUDENT = 'student',
  TEACHER = 'teacher',
  APP_MANAGER = 'appmanager',
}

export const PermissionOptions = Object.entries(PERMISSION_ENUM)
  .filter((el) => {
    const [key, value] = el;
    return key !== PERMISSION_ENUM.PUBLIC && value !== PERMISSION_ENUM.ADMIN;
  })
  .map((el) => {
    const [key, value] = el;
    return {
      label: key,
      value: value,
    };
  });

export const NUMBER_DEFAULT_ROW_PER_PAGE = 5;
export const NUMBER_DEFAULT_PAGE = 0;

export const OPTION_RISK_LEVEL = [
  { label: 'Rủi ro thấp', value: 0 },
  { label: 'Rủi ro trung bình', value: 1 },
  { label: 'Rủi ro cao', value: 2 },
];

export enum StudentExamStatus {
  NotStarted = 'NotStarted',
  InProgress = 'InProgress',
  Submitted = 'Submitted',
  Failed = 'Failed',
  Passed = 'Passed',
}

export enum ActiveStatusExamTeacher {
  Draft = 0,
  Published = 1,
  Finished = 2,
}

export enum ActiveStatusExamStudent {
  Draft = 0,
  Published = 1,
  Cancelled = 2,
}

export const ExamStatus = [
  { label: 'Draft', value: 0 },
  { label: 'Published', value: 1 },
  { label: 'Cancelled', value: 2 },
];

export const ExamType = [
  { label: 'Essay', value: 0 },
  { label: 'MutipleChoice', value: 1 },
  { label: 'TrueFalse', value: 2 },
  { label: 'FillInTheBlank', value: 3 },
];

export interface TimeValue {
  hour: number;
  minute: number;
  second: number;
  millisecond: number;
}

export enum ExamLiveStatus {
  Inactive = 0,
  Upcoming = 1,
  Ongoing = 2,
  Completed = 3,
}

export const essay = 'essay';
export const multipleChoice = 'multiple-choice';

export enum RoleEnum {
  Student = 1,
  Lecture = 2,
  Supervisor = 3,
  Administrator = 4,
}

export enum LogType {
  Info = 0,
  Warning = 1,
  Violation = 2, // vi phạm
  Critical = 3, // quan trọng
}

export enum AccessMode {
  View = 0,
  Edit = 1,
}

export const MAX_QUESTION_LENGTH = 500

export const PASSWORD_AFTER_RESET = 'Aa123456789@'