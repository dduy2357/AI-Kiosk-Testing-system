export interface IFaceCaptureRequest {
  StudentExamId: string;
  ExamId: string;
  LogType?: string;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IFaceCapture {
  success: boolean;
  message: string;
  data: Data;
}

export interface Data {
  result: FaceCaptureList;
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface FaceCaptureList {
  userId: string;
  fullName: string;
  userCode: string;
  studentExamId: string;
  captures: Capture[];
}

export interface Capture {
  captureId: string;
  imageUrl: string;
  description: string;
  logType: number;
  emotions: string;
  dominantEmotion: null | string;
  avgArousal: number;
  avgValence: number;
  inferredState: null | string;
  region: string;
  result: null | string;
  status: null | string;
  isDetected: boolean;
  createdAt: Date;
  updatedAt: Date;
  errorMessage?: string;
}

export interface Response<T> {
  data: T;
}

export type ResponseFaceCapture = Response<IFaceCapture>;
