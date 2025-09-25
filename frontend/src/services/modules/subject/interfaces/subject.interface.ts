export interface ISubjectForm {
  subjectId?: string;
  subjectName: string;
  subjectDescription: string;
  subjectCode: string;
  subjectContent?: string;
  status: boolean;
}

export interface ISubjectRequest {
  pageSize: number;
  currentPage: number;
  textSearch: string;
  status: boolean | string | undefined;
}

export interface ISubject {
  success: boolean;
  message: string;
  data: SubjectData;
}

export interface SubjectData {
  result: SubjectList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface SubjectList {
  subjectId: string;
  subjectCode: string;
  subjectName: string;
  subjectDescription: string;
  status: boolean;
  createdAt: Date;
  updatedAt: Date;
}

export interface Response<T> {
  data: T;
}

export type ResponseGetListSubject = Response<ISubject>;

export interface ISubjectDetail {
  success: boolean;
  message: string;
  data: SubjectDetail;
}

export interface SubjectDetail {
  subjectId: string;
  subjectCode: string;
  subjectName: string;
  subjectDescription: string;
  status: boolean;
  credits: number;
  createdAt: Date;
  updatedAt: Date;
}

export type ResponseGetDetailSubject = Response<ISubjectDetail>;
