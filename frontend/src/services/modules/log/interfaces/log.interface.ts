//!WriteUserActivity
export interface WriteUserActivity {
  userId: string;
  actionType: string;
  objectId: string;
  description: string;
  status: number;
  metadata: string;
}

//!WriteExamActivity
export interface WriteExamActivity {
  studentExamId: string;
  userId: string;
  actionType: string;
  description: string;
  screenshotPath: string;
  logType: number;
}

//!IUserLogList
export interface IUserLogListRequest {
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
  FromDate?: Date;
  ToDate?: Date;
  ActionType?: string;
  LogStatus?: number;
}

export interface IUserLogList {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: UserLogList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface UserLogList {
  logId: string;
  fullName: string;
  userCode: string | null;
  actionType: string;
  description: string;
  createdAt: Date;
}

export interface Response<T> {
  data: T;
}

export type UserLogListResponse = Response<IUserLogList>;

