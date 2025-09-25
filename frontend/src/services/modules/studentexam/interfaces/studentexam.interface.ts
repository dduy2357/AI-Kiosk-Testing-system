export interface IStudentExam {
  success: boolean;
  message: string;
  data: StudentExamList[];
}

export interface StudentExamList {
  examId: string;
  title: string;
  startTime: Date;
  duration: number;
  endTime: Date;
  status: string;
  examType: number;
  verifyCamera: boolean;
}

export interface Response<T> {
  data: T;
}

export type StudentExamResponse = Response<IStudentExam>;
export interface ExamSubmitForm {
  studentExamId: string;
  examId: string;
  answers: Answer[];
}

export interface Answer {
  questionId: string;
  userAnswer: string;
}

//!Exam Detail Interfaces
export interface IExamDetail {
  success: boolean;
  message: string;
  data: ExamDetail;
}

export interface ExamDetail {
  examId: string;
  title: string;
  duration: number;
  startTime: Date;
  endTime: Date;
  totalQuestions: number;
  questions: Question[];
  examType: number;
}

export interface Question {
  questionId: string;
  content: string;
  questionType: string;
  point: number;
  difficultLevel: number;
  options: string[];
}

export type ExamDetailResponse = Response<IExamDetail>;

//!Essay Result Interfaces
export interface IEssayResult {
  success: boolean;
  message: string;
  data: EssayList;
}

export interface EssayList {
  studentExamId: string;
  examId: string;
  examTitle: string;
  subjectName: string;
  roomCode: string;
  examDate: Date;
  submitTime: Date;
  durationSpent: number;
  studentName: string;
  studentCode: string;
  studentAvatar: string;
  isMarked: boolean;
  totalQuestions: number;
  answers: Answers[];
}

export interface Answers {
  questionId: string;
  questionContent: string;
  correctAnswer: string;
  userAnswer: string;
  maxPoints: number;
  pointsEarned: number;
}

export interface FormEssayValue {
  studentExamId: string;
  examId: string;
  scores: Score[];
}

export interface Score {
  questionId: string;
  pointsEarned: number;
}

export type ResponseEssayResult = Response<IEssayResult>;

//!History Exam Interfaces
export interface IHistoryExamRequest {
  StartDate?: Date | string;
  EndDate?: Date | string;
  pageSize: number;
  currentPage: number;
  textSearch?: string;
}

export interface IHistoryExam {
  success: boolean;
  message: string;
  data: HistoryExamData;
}

export interface HistoryExamData {
  result: HistoryExamList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}
export interface HistoryExamList {
  studentExamId: string;
  examId: string;
  examTitle: string;
  score: number;
  examDate: Date | string;
  submitTime: Date | string;
  durationSpent: number;
}

export interface Responst<T> {
  data: T;
}

export type ResponseGetListHistoryExam = Responst<IHistoryExam>;
