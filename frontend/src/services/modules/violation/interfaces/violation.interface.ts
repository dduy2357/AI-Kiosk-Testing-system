export interface IViolationRequest {
    ExamId?: string;
    StudentExamId?: string;
    PageSize: number;
    CurrentPage: number;
    TextSearch?: string;
}

export interface IViolation {
    success: boolean;
    message: string;
    data: ListViolation;
}

export interface ListViolation {
    result: Violation[];
    totalPage: number;
    pageSize: number;
    currentPage: number;
    total: number;
}

export interface Violation {
    violationId: string;
    creatorName: string;
    creatorEmail: string;
    creatorCode: string;
    studentName: string;
    studentCode: string;
    studentEmail: string;
    studentExamId: string;
    violationName: string;
    message: string;
    screenshotPath: string;
    createdAt: Date;
}

export interface Response<T> {
    data: T;
}

export type ResponseViolation = Response<IViolation>;

export interface IViolationDetail {
    success: boolean;
    message: string;
    data: ViolationDetail;
}

export interface ViolationDetail {
    violationId: string;
    creatorName: string;
    creatorEmail: string;
    creatorCode: string;
    studentName: string;
    studentCode: string;
    studentEmail: string;
    studentExamId: string;
    violationName: string;
    message: string;
    screenshotPath: string;
    createdAt: Date;
}

export type ResponseViolationDetail = Response<IViolationDetail>;

export interface IViolationForm {
    studentExamId: string;
    violateName: string;
    message: string;
    screenshotPath?: string;
    isSendMail: boolean
}