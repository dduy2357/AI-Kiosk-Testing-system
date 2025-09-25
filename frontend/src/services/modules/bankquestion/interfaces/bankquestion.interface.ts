export interface IQuestionBankForm {
  title: string,
  subjectId: string,
  description: string
}

export interface IShareBankQuestionForm {
  questionBankId: string,
  targetUserEmail: string,
  accessMode: string
}

export interface IBankQuestionRequest {
  pageSize: number;
  currentPage: number;
  textSearch?: string;
  status?: number;
  IsMyQuestion?: boolean;
  filterSubject?: string
}

export interface IBankQuestion {
  success: boolean;
  message: string;
  data: BankQuestionData;
}

export interface BankQuestionData {
  result: BankQuestionList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
  totalQuestionBanks: number;
  totalQuestionsQB: number;
  totalSubjects: number;
  totalSharedQB: number;
}

export interface BankQuestionList {
  questionBankId: string;
  title: string;
  createBy: string;
  subjectName: string;
  totalQuestions: number;
  multipleChoiceCount: number;
  trueFalseCount: number;
  fillInTheBlank: number;
  essayCount: number;
  status: number;
  sharedByName: string;
  sharedWithUsers: string[];
}

export interface Response<T> {
  data: T;
}

export type ResponseGetListBankQuestion = Response<IBankQuestion>;

export interface IQuestionBankDetail {
  success: boolean;
  message: string;
  data: QuestionBankDetail;
}

export interface QuestionBankDetail {
  questionBankId: string;
  questionBankName: string;
  createBy: string;
  description: string;
  subjectId: string;
  subjectName: string;
  totalQuestions: number;
  multipleChoiceCount: number;
  essayCount: number;
  status: number;
  averageDifficulty: number;
  questions: Question[];
}

export interface Question {
  questionId: string;
  subjectId: string;
  content: string;
  type: number;
  difficultLevel: number;
  point: number;
  options: string[];
  correctAnswer: string;
  explanation: string;
  objectFile: string | null;
  status: number;
  creatorId: string;
}

export type ResponseGetDetailQuestionBank = Response<IQuestionBankDetail>;