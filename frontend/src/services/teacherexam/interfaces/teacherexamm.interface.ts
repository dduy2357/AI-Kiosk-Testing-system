export interface ITeacherExamRequest {
  status?: number;
  IsMyQuestion?: boolean;
  pageSize: number;
  currentPage: number;
  textSearch?: string;
  IsExamResult?: boolean;
}

export interface ITeacherExam {
  success: boolean;
  message: string;
  data: TeacherExamData;
}

export interface TeacherExamData {
  result: TeacherExamList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface TeacherExamList {
  examId: string;
  title: string;
  description: string;
  roomName: string;
  totalQuestions: number;
  totalPoints: number;
  duration: number;
  startTime: Date;
  endTime: Date;
  createdBy: string;
  status: number;
  isShowResult: boolean;
  isShowCorrectAnswer: boolean;
  examType: number;
  guideLines: string;
  liveStatus: number;
}

export interface Response<T> {
  data: T;
}

export type ResponseGetListTeacherExam = Response<ITeacherExam>;
