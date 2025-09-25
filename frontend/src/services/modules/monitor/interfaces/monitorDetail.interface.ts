export interface IMonitorDetail {
  success: boolean;
  message: string;
  data: Data;
}

export interface IMonitorDetailRequest {
  StudentExamStatus?: number | null;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface Data {
  result: MonitorDetailList;
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface MonitorDetailList {
  examId: string;
  subjectName: string;
  roomCode: string;
  title: string;
  duration: number;
  examStartTime: Date;
  examEndTime: Date;
  examType: number;
  examLive: string;
  createUserName: string;
  createEmail: string;
  maxCapacity: number;
  studentDoing: number;
  studentCompleted: number;
  students: Student[];
}

export interface Student {
  studentExamId: string;
  userId: string;
  fullName: string;
  userCode: string;
  email: string;
  ipAddress: string;
  browserInfo: string;
  startTime: Date;
  submitTime: Date;
  studentExamStatus: number;
  totalQuestions: number;
  answeredQuestions: number;
  score: number;
  warningCount: number;
  violinCount: number;
}

export interface Response<T> {
  data: T;
}

export type ResponseGetMonitorDetailList = Response<IMonitorDetail>;
