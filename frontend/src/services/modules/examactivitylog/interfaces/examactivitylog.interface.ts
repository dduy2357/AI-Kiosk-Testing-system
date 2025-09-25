export interface IExamActivityLogRequest {
    studentExamId?: string;
    pageSize: number;
    currentPage: number;
    textSearch?: string;
}

export interface IExamActivityLog {
    success: boolean;
    message: string;
    data: ListExamActivityLog;
}

export interface ListExamActivityLog {
    result: IListExamActivityLog;
    totalPage: number;
    pageSize: number;
    currentPage: number;
    total: number;
}

export interface IListExamActivityLog {
    examLogId: string;
    userCode: null;
    fullName: string;
    actionType: string;
    description: string;
    metadata: string;
    logType: number;
    createdAt: Date;
}

export interface Response<T> {
    data: T;
}

export type ResponseExamActivityLog = Response<IExamActivityLog>;

export interface IExamActivityLogDetail {
    success: boolean;
    message: string;
    data: ExamActivityLogDetail;
}

export interface ExamActivityLogDetail {
    examLogId: string;
    studentExamId: string;
    userId: string;
    userCode: null;
    fullName: string;
    actionType: string;
    description: string;
    ipAddress: string;
    browserInfo: string;
    screenshotPath: string;
    deviceId: null;
    deviceUsername: null;
    metadata?: string | string[];
    logType: number;
    createdAt: Date;
    updatedAt: Date;
}

export type ResponseExamActivityLogDetail = Response<IExamActivityLogDetail>;