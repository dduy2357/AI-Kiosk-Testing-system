export interface IQuestionRequest {
  Status?: number;
  IsMyQuestion?: boolean;
  DifficultyLevel?: number;
  pageSize: number;
  currentPage: number;
  textSearch: string;
}

export interface IQuestionForm {
  questionBankId: string;
  subjectId: string;
  content: string;
  type: number;
  difficultLevel: number;
  point: number;
  options: string[];
  correctAnswer: string;
  explanation: string;
  objectFile: string;
  tags: string;
  description: string;
}
export interface IQuestionFormEdit {
  questionBankId: string;
  content: string;
  type: number;
  difficultLevel: number;
  point: number;
  options: string[];
  correctAnswer: string;
  explanation: string;
  objectFile: string;
  tags: string;
  description: string;
  questionId: string;
  createUserId: string;
}
export interface IQuestion {
  success: boolean;
  message: string;
  data: QuestionData;
}

export interface QuestionData {
  result: QuestionList[];
  totalQuestions: number;
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface QuestionList {
  questionId: string;
  subjectId: string;
  subjectName: string;
  questionBankId: string;
  questionBankName: string;
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

export interface Response<T> {
  data: T;
}

export type ResponseGetListQuestion = Response<IQuestion>;
