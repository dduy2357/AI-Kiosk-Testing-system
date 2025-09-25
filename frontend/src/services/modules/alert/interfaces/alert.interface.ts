export interface IAlertRequest {
  IsRead?: boolean;
  DateFrom?: Date;
  DateTo?: Date;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IAlertList {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: ListAlert[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface ListAlert {
  id: string;
  message: string;
  sendToId: string;
  createdName: string;
  createdAvatar: string;
  createdEmail: string;
  createdBy: string;
  isRead: boolean;
  type: string;
  createdAt: Date;
}

export interface Response<T> {
  data: T;
}

export type ResponseAlertList = Response<IAlertList>;

export interface IDetailAlert {
  success: boolean;
  message: string;
  data: AlertDetail;
}

export interface AlertDetail {
  id: string;
  type: string;
  message: string;
  isRead: boolean;
  studentName: string;
  studentAvatar: string;
  studentUserCode: string;
  createdName: string;
  createdAvatar: string;
  createdUserCode: string;
  createdAt: Date;
}

export type ResponseDetailAlert = Response<IDetailAlert>;
