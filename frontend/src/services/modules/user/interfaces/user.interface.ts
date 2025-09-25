export interface IUserRequest {
  pageSize: number;
  currentPage: number;
  textSearch: string;
  roleId?: number | null;
  status?: number;
  campusId?: string;
  sortType?: string;
}

export interface InitialFilterUser {
  pageSize: number;
  currentPage: number;
  textSearch: string;
}

export interface IUser {
  success: boolean;
  message: string;
  data: IListUser;
}

export interface IListUser {
  result: UserList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface UserList {
  userId: string;
  fullName: string;
  userCode: any;
  phone: string;
  email: string;
  avatarUrl: string;
  sex: any;
  isActive: boolean;
  createAt: string;
  updateAt: any;
  createUser: string;
  updateUser: any;
  status: number;
  lastLogin: string;
  lastLoginIp: string;
  dob: string;
  address: any;
  campusId?: string;
  departmentId: any;
  positionId: any;
  majorId: any;
  specializationId: any;
  roleId: any[];
  position: string;
  major: string;
  campus: string;
  department: string;
  specialization: string;
}

export interface Response<T> {
  data: T;
}

export type ResponseGetListUser = Response<IUser>;
export interface IUserResetPass {
  userId: string;
  password: string;
  rePassword: string;
}
