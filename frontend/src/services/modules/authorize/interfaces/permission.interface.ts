export interface IPermissionsRequest {
  PageSize: number;
  CurrentPage: number;
  TextSearch: string;
  SortType: number | null;
}

export interface IPermissions {
  success: boolean;
  message: string;
  data: IPermissionsList;
}

export interface IPermissionsList {
  result: PermissionsList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface PermissionsList {
  categoryId: string;
  description: string;
  permissions: Permissions[];
}

export interface Permissions {
  id: string;
  name: string;
  action: Action;
  resource: string;
  isActive: boolean;
}

export enum Action {
  Add = 'add',
  Createupdate = 'createupdate',
  Delete = 'delete',
  Update = 'update',
  View = 'view',
}

export interface Response<T> {
  data: T;
}

export type ResponseGetListPermissions = Response<IPermissions>;
