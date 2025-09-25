export interface IUserActivityLogRequest {
    logStatus?: number | null;
    ActionType?: string | null;
    FromDate: Date | undefined;
    ToDate: Date | undefined;
    RoleEnum: number | null;
    pageSize: number;
    currentPage: number;
    textSearch?: string;
}

export interface IUserActivityLog {
    success: boolean;
    message: string;
    data: ListUserActivityLog;
}

export interface ListUserActivityLog {
    result: IListUserActivityLog;
    totalPage: number;
    pageSize: number;
    currentPage: number;
    total: number;
}

export interface IListUserActivityLog {
    logId: string;
    fullName: string;
    userCode: string;
    actionType: string;
    description: string;
    createdAt: Date;
}

export interface Response<T> {
    data: T;
}

export type ResponseUserActivityLog = Response<IUserActivityLog>;
export interface IUserActivityLogDetail {
    success: boolean;
    message: string;
    data: UserActivityLogDetail;
}
export interface UserActivityLogDetail {
    logId: string;
    userId: string;
    email: string;
    fullName: string;
    userCode: string;
    actionType: string;
    objectId: null;
    description: string;
    status: number;
    metadata: string;
    ipAddress: string;
    browserInfo: string;
    createdAt: Date;
    lastLogin: Date;
}

export type ResponseUserActivityLogDetail = Response<IUserActivityLogDetail>;