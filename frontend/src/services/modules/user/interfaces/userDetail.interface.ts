export interface IUserDetail {
  success: boolean;
  message: string;
  data: UserDetail;
}

export interface UserDetail {
  campus: string;
  department: null;
  position: null;
  major: string;
  specialization: null;
  userId: string;
  fullName: string;
  userCode: string;
  phone: string;
  email: string;
  avatarUrl: string;
  sex: number;
  createAt: null;
  updateAt: Date;
  createUser: null;
  updateUser: string;
  status: number;
  lastLogin: Date;
  lastLoginIp: string;
  dob: Date;
  address: string;
  roleId: number[] | number | string;
}

export interface Response<T> {
  data: T;
}

export type ResponseGetDetailUser = Response<IUserDetail>;
