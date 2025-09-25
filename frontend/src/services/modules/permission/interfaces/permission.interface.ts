export interface IPermissionRequest {
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IPermission {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: PermissionList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface PermissionList {
  categoryId: string;
  description: string;
  permissions: any[];
}

export interface Response<T> {
  data: T;
}

export type ResponsePermission = Response<IPermission>;

export interface IPermissionDetail {
  success: boolean;
  message: string;
  data: PermissionDetail;
}

export interface PermissionDetail {
  categoryId: string;
  description: string;
  permissions: any[];
}

export type ResponsePermissionDetail = Response<IPermissionDetail>;
