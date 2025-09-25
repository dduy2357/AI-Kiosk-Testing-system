export interface IUserInRoomRequest {
  RoomId: string;
  Role?: number | null;
  Status?: number | null;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IUserInRoom {
  success: boolean;
  message: string;
  data: ListUserInRoom;
}

export interface ListUserInRoom {
  result: IListUserInRoom;
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface IListUserInRoom {
  roomId: string;
  description: string;
  isRoomActive: boolean;
  users: UserElement[];
}

export interface UserElement {
  roomId: string;
  roomUserId: string;
  userId: string;
  role: number;
  userStatus: number;
  joinTime: Date;
  updatedAt: Date;
  user: UserUser;
}

export interface UserUser {
  userId: string;
  userCode: null;
  fullname: string;
  email: string;
  avatar: null | string;
  sex: number;
  createAt: null;
  updateAt: Date | null;
}

export interface Response<T> {
  data: T;
}

export type ResponseUserInRoom = Response<IUserInRoom>;

export interface AllRoomUser {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: AllRoomUser[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface AllRoomUser {
  userId: string;
  userCode: string;
  fullName: string;
  major: string;
}

export type ResponseAllRoomUser = Response<AllRoomUser>;
