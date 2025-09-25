export interface IResultReport {
  subjectName: string;
  examDate: string;
  durationMinutes: number;
  questionType: string;
  totalPoints: number;
  createdBy: string;
  totalStudents: number;
  averageScore: number;
  averageDuration: string;
  liveStatus: string;
  studentResults: StudentResult[];
}

export interface StudentResult {
  fullName: string;
  className: string;
  score: number;
  submitTime: string;
  status: string;
  gradingStatus: string;
  workingTime: string;
  questionType: string;
  studentExamId: string;
}

export interface Responst<T> {
  data: T;
}

export type ResultReportResponse = Responst<IResultReport>;

/////////////////////////////////////////////////////////
export interface IResultDetail {
  success: boolean;
  message: string;
  data: ResultDetail;
}

export interface ResultDetail {
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
}

export interface Question {
  questionId: string;
  content: string;
  difficulty: number;
  type: number;
  questionBankId: string;
  questionBankName: string;
}

export interface Responst<T> {
  data: T;
}

export type ResultDetailResponse = Responst<IResultDetail>;

///////////////////////////////////////////////
export interface StudentResultDetail {
  fullname: string;
  className: string;
  gradingStatus: string;
  score: number;
  submitTime: string;
  status: string;
  workingTime: string;
  questionType: string;
  studentExamId: string;
}

export interface Answer {
  questionId: string;
  questionContent: string;
  userAnswer: string;
  correctAnswer: string;
  isCorrect: boolean | null;
  pointsEarned: number;
  timeSpent: number;
  options?: string;
}

export interface ExamInfo {
  examTitle: string;
  score: number;
  totalQuestions: number;
  startTime: string;
  submitTime: string;
  durationSpent: number;
  totalCorrectAnswers: number;
  totalWrongAnswers: number;
  answers: Answer[];
  studentName: string;
  studentCode: string;
}
