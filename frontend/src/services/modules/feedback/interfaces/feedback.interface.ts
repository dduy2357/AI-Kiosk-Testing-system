export interface IFeedbackForm {
    title: string;
    content: string;
}

export interface IFeedbackRequest {
    dateFrom: Date | undefined,
    dateTo: Date | undefined,
    pageSize: number,
    currentPage: number,
    textSearch: string
}

export interface IFeedback {
    success: boolean;
    message: string;
    data: IFeedbackList;
}

export interface IFeedbackList {
    result: Feedback[];
    totalPage: number;
    pageSize: number;
    currentPage: number;
    total: number;
}

export interface Feedback {
    feedbackId: string;
    title: string;
    content: string;
    studentId: string;
    studentName: string;
    studentEmail: string;
    studentCode: string;
    createdAt: Date;
}

export interface Response<T> {
    data: T;
}

export type FeedbackResponse = Response<IFeedback>;

export interface IFeedbackDetail {
    success: boolean;
    message: string;
    data: FeedbackDetail;
}

export interface FeedbackDetail {
    feedbackId: string;
    title: string;
    content: string;
    studentId: string;
    studentName: string;
    studentEmail: string;
    studentCode: string;
    createdAt: Date;
}

export type ResponseGetDetailFeedback = Response<IFeedbackDetail>;