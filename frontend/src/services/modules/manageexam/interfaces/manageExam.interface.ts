export interface IManageExamFormValue {
  questionBankId: string;
  roomId: string;
  questionIds: string[];
  title: string;
  description: string;
  duration: number;
  startTime: Date | null;
  endTime: Date | null;
  isShowResult: boolean;
  isShowCorrectAnswer: boolean;
  status: number;
  examType: number;
  guideLines: string;
  verifyCamera: boolean;
}

export interface IAssignOtpToExam {
  examId: string;
  timeValid: number;
  examOtpId?: string;
  expiredAt?: string;
  otpCode?: string;
}

export interface IManageExamRequest {
  status?: number;
  isMyQuestion?: boolean;
  pageSize?: number;
  currentPage?: number;
  textSearch?: string;
}

export interface IManageExam {
  message: string;
  data: Data;
}

export interface Data {
  result: ManageExamList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface ManageExamList {
  examId: string;
  title: string;
  roomName: string;
  totalQuestions: number;
  totalPoints: number;
  duration: number;
  startTime: string;
  endTime: string;
  createdBy: string;
  status: number;
  isShowResult: boolean;
  isShowCorrectAnswer: boolean;
  examType: number;
  liveStatus: number;
  createdById: string;
}

export interface Response<T> {
  data: T;
}

export type ResponseManageExam = Response<IManageExam>;

export interface IExamDetail {
  success: boolean;
  message: string;
  data: ExamDetail;
}

export interface ExamDetail {
  examId: string;
  title: string;
  description: string;
  roomId: string;
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
  guideLines: null;
  liveStatus: string;
  questions: Question[];
  verifyCamera: boolean;
}
export interface Question {
  questionId: string;
  content: string;
  difficulty: number;
  type: number;
  questionBankId: string;
  questionBankName: string;
}

export type ResponseGetDetailExam = Response<IExamDetail>;
