export interface IRole {
  success: boolean;
  message: string;
  data: Datum[];
}

export interface Datum {
  id: number;
  name: string;
}

//!
export interface IPermissionRequest {
  pageSize: number;
  currentPage: number;
  textSearch: string;
}

export interface IPermission {
  success: boolean;
  message: string;
  data: IListPermissions;
}

export interface IListPermissions {
  result: PermissionList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
}

export interface PermissionList {
  roleId: number;
  roleName: string;
  categories: Category[];
  description?: string;
  isActive?: boolean;
}

export interface Category {
  categoryId: string;
  description: string;
  permissions: Permission[];
}

export interface Permission {
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

export type ResponseGetListPermission = Response<IPermission>;
