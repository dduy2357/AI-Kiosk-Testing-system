export interface IRoomRequest {
  ClassId?: string;
  SubjectId?: string;
  IsActive?: boolean | null;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IRoom {
  success: boolean;
  message: string;
  data: IListRoom;
}

export interface IListRoom {
  result: RoomList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface RoomList {
  roomId: string;
  isRoomActive: boolean;
  roomDescription: string;
  roomCode: string;
  capacity: number;
  classId: string;
  classCode: string;
  classDescription: string;
  classCreatedBy: string;
  isClassActive: boolean;
  classStartDate: Date;
  classEndDate: Date;
  subjectId: string;
  subjectName: string;
  subjectDescription: string;
  subjectCode: string;
  subjectContent: null;
  subjectStatus: boolean;
  totalUsers: number;
  roomTeachers: string[];
  roomCreatedAt: Date;
  roomUpdatedAt: Date;
}
export interface Response<T> {
  data: T;
}

export type ResponseRoomList = Response<IRoom>;

export interface IRoomDetail {
  success: boolean;
  message: string;
  data: RoomDetail;
}

export interface RoomDetail {
  roomId: string;
  isRoomActive: boolean;
  roomDescription: string;
  roomCode: string;
  capacity: number;
  classId: string;
  classCode: string;
  classDescription: null;
  classCreatedBy: string;
  isClassActive: boolean;
  classStartDate: Date;
  classEndDate: Date;
  subjectId: string;
  subjectName: string;
  subjectDescription: string;
  subjectCode: string;
  subjectContent: null;
  subjectStatus: boolean;
  totalUsers: number;
  roomTeachers: any[];
  roomCreatedAt: Date;
  roomUpdatedAt: Date;
}

export type ResponseRoomDetail = Response<IRoomDetail>;
