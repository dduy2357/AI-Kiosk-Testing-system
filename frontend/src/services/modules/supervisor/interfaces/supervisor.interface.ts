export interface ISupervisorDetail {
  success: boolean;
  message: string;
  data: SupervisorDetail;
}

export interface SupervisorDetail {
  examId: string;
  examTitle: string;
  subjectName: string;
  classCode: string;
  roomCode: string;
  startTime: Date;
  endTime: Date;
  supervisorVMs: SupervisorVM[];
}

export interface SupervisorVM {
  userId: string;
  avatarUrl: null;
  department: string;
  major: string;
  specialization: string;
  fullName: string;
  email: string;
  userCode: string;
  phone: string;
  assignAt: Date;
}

export interface Response<T> {
  data: T;
}

export type ISupervisorDetailResponse = Response<ISupervisorDetail>;

export interface ISupervisorAssign {
  supervisorId: string[];
  examId: string;
  note?: string;
}

//List Exam Supervisor
export interface IListExamSupervisorRequest {
  PageSize: number;
  CurrentPage: number;
  TextSearch?: string;
}
export interface IListExamSupervisor {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: ListExamSupervisor[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface ListExamSupervisor {
  examId: string;
  title: string;
  subjectName: string;
  classCode: string;
  startTime: Date;
  endTime: Date;
  createdAt: Date;
  studentCount: number;
  hasSupervisor: boolean;
}

export type IListExamSupervisorResponse = Response<IListExamSupervisor>;

//Supervisor
export interface ISupervisorRequest {
  PageSize: number;
  CurrentPage: number;
  TextSearch?: string;
}

export interface IListSupervisor {
  success: boolean;
  message: string;
  data: ISupervisor;
}

export interface ISupervisor {
  result: ListSupervisor[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface ListSupervisor {
  userId: string;
  fullName: string;
  userCode: string;
  email: string;
  subjectNames: string[];
}

export type IListSupervisorResponse = Response<IListSupervisor>;
