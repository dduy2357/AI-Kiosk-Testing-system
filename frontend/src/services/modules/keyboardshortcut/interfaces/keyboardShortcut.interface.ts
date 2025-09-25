export interface IKeyboardShorcutRequest {
  IsActive?: boolean;
  RiskLevel?: number;
  PageSize?: number;
  CurrentPage?: number;
  TextSearch?: string;
}

export interface IKeyboardShorcut {
  success: boolean;
  message: string;
  data: IKeyboardShorcutList;
}

export interface IKeyboardShorcutList {
  result: KeyboardShortcutList[];
  totalPage: number;
  pageSize: number;
  currentPage: number;
  total: number;
}

export interface KeyboardShortcutList {
  keyId: string;
  keyCode: string;
  keyCombination: string;
  description: Description;
  riskLevel: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  createdUser: string;
  updatedUser: null;
}

export enum Description {
  String = 'string',
}

export interface Response<T> {
  data: T;
}

export type KeyboardShortcutListResponse = Response<IKeyboardShorcut>;

export interface IKeyboardShortcutDetail {
  success: boolean;
  message: string;
  data: KeyboardShortcutDetail;
}

export interface KeyboardShortcutDetail {
  keyId: string;
  keyCode: string;
  keyCombination: string;
  description: string;
  riskLevel: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  createdUser: string;
  updatedUser: null;
}

export type KeyboardShortcutDetailResponse = Response<IKeyboardShortcutDetail>;
