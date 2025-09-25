export interface IProhibitedRequest {
  IsActive?: boolean;
  RiskLevel?: number;
  Category?: number;
  TypeApp?: number;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IProhibited {
  success: boolean;
  message: string;
  data: IProhibitedList;
}

export interface IProhibitedList {
  result: ProhbitedList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface ProhbitedList {
  appId: string;
  appName: string;
  processName: string;
  description: null | string;
  appIconUrl: null;
  isActive: boolean;
  riskLevel: number;
  category: number;
  createdAt: Date;
  updatedAt: Date;
  typeApp: number;
}

export interface Response<T> {
  data: T;
}

export type ResponseProhibited = Response<IProhibited>;

export interface IprohibitedDetail {
  success: boolean;
  message: string;
  data: ProhibitedDetail;
}

export interface ProhibitedDetail {
  appId: string;
  appName: string;
  processName: string;
  description: null;
  appIconUrl: null;
  isActive: boolean;
  riskLevel: number;
  category: number;
  createdAt: Date;
  updatedAt: Date;
  typeApp: string;
}

export type ResponseProhibitedDetail = Response<IprohibitedDetail>;
