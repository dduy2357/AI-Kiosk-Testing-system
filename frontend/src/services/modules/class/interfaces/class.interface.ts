export interface IClassRequest {
  FromDate?: Date;
  ToDate?: Date;
  IsActive?: boolean | undefined | null;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IClass {
  success: boolean;
  message: string;
  data: IClassList;
}

export interface IClassList {
  result: ClassList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface ClassList {
  classId?: string;
  classCode?: string;
  description?: string;
  maxStudent?: number;
  createdBy?: string;
  isActive?: boolean;
  startDate?: Date;
  endDate?: Date;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface Response<T> {
  data: T;
}

export type ResponseGetListClass = Response<IClass>;

export interface IDetailClass {
  success: boolean;
  message: string;
  data: DetailClass;
}

export interface DetailClass {
  classId: string;
  classCode: string;
  description: string;
  maxStudent: number;
  createdBy: string;
  isActive: boolean;
  startDate: Date;
  endDate: Date;
  createdAt: Date;
  updatedAt: Date;
}

export type ResponseGetDetailClass = Response<IDetailClass>;
