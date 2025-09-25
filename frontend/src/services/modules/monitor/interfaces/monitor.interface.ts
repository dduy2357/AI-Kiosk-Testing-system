export interface IMonitorRequest {
  SubjectId?: string;
  ExamStatus?: number;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
  ExamId?: string;
  Status?: number;
}

export interface IMonitor {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: MonitorList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface MonitorList {
  examId: string;
  roomId: string;
  roomCode: string;
  classId: string;
  classCode: string;
  subjectName: string;
  title: string;
  description: string;
  duration: number;
  startTime: Date;
  endTime: Date;
  examType: number;
  status: number;
  createUserId: string;
  createUserName: string;
  createEmail: string;
  maxCapacity: number;
  studentDoing: number;
  studentCompleted: number;
  isCompleted: boolean;
  studentIds: string[];
}

export interface data<T> {
  data: T;
}

export type MonitorListResponse = data<IMonitor>;

export interface IStatistic {
  success: boolean;
  message: string;
  data: ListStatistic[];
}

export interface ListStatistic {
  angry: number;
  disgust: number;
  fear: number;
  happy: number;
  neutral: number;
  sad: number;
  surprise: number;
  message: string;
}

export type ListStatisticResponse = data<IStatistic>;
