import BaseUrl from '@/consts/baseUrl';
import { getFromStorage } from '@/helpers/common';
import { KEY_LANG } from '@/i18n/config';
import { UserInfo } from '@/interfaces/user';
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

export const TOKEN_KEY = 'token';
export const USER_KEY = 'user';
export const STUDENT_ID_KEY = 'studentId';
export const TIME_REMAINING_KEY = 'timeRemaining';
export const SELECTED_CAMPUS_KEY = 'selectedCampus';
export const EXTRA_START_TIME_KEY = 'extraStartTime';
export const EXAM_START_TIME_KEY = 'examStartTime';
export const OTP_EXPIRED_KEY = 'otpExpired';
export const OTP_DATA_KEY = 'otpData';

class Services {
  axios: AxiosInstance;

  constructor() {
    this.axios = axios;
    this.axios.defaults.withCredentials = false;

    //! Interceptor request
    this.axios.interceptors.request.use(
      function (config) {
        config.headers['x-timezone'] = Intl.DateTimeFormat().resolvedOptions().timeZone;
        const { data: token } = getFromStorage(TOKEN_KEY) || '';
        const { data: lang } = getFromStorage(KEY_LANG) || '';
        config.headers['Accept-Language'] = lang === 'en' ? 'en' : 'vi';
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      function (error) {
        return Promise.reject(error);
      },
    );

    //! Interceptor response
    this.axios.interceptors.response.use(
      (config) => {
        const token = getFromStorage(TOKEN_KEY) || '';
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        if (
          error.response?.data?.Message === 'Token is invalid or expired' &&
          error.response?.data?.isOk === false
        ) {
          const channel = new BroadcastChannel('auth');
          channel.postMessage({ type: 'logout' });
          channel.close();

          this.clearStorage();
          window.sessionStorage.clear();

          window.location.href = BaseUrl.Login;
        }
        return Promise.reject(error);
      },
    );
  }

  attachTokenToHeader(token: string) {
    this.axios.interceptors.request.use(
      function (config) {
        if (config.headers) {
          // Do something before request is sent
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      function (error) {
        return Promise.reject(error);
      },
    );
  }

  setupInterceptors() {
    this.axios.interceptors.response.use(
      (response) => {
        return response;
      },
      (error) => {
        const { status } = error?.response || {};
        if (status === 401) {
          window.localStorage.clear();
          window.location.reload();
        }

        return Promise.reject(error);
      },
    );
  }

  get(url: string, config?: AxiosRequestConfig) {
    return this.axios.get(url, config);
  }

  post(url: string, data: any, config?: AxiosRequestConfig) {
    return this.axios.post(url, data, config);
  }

  delete(url: string, config?: AxiosRequestConfig) {
    return this.axios.delete(url, config);
  }

  put(url: string, data: any, config?: AxiosRequestConfig) {
    return this.axios.put(url, data, config);
  }

  saveTokenStorage(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  getTokenStorage() {
    const token = localStorage.getItem(TOKEN_KEY);
    return token || '';
  }

  clearStorage() {
    // Xóa các key cố định
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    localStorage.removeItem(STUDENT_ID_KEY);
    localStorage.removeItem('activeTab');
    localStorage.removeItem(SELECTED_CAMPUS_KEY);

    // Xóa tất cả các key bắt đầu bằng TIME_REMAINING_KEY
    Object.keys(localStorage).forEach((key) => {
      if (key.startsWith(TIME_REMAINING_KEY)) {
        localStorage.removeItem(key);
      }
    });

    // Xóa tất cả các key bắt đầu bằng EXTRA_START_TIME_KEY
    Object.keys(localStorage).forEach((key) => {
      if (key.startsWith(EXTRA_START_TIME_KEY)) {
        localStorage.removeItem(key);
      }
    });

    // Xóa tất cả các key bắt đầu bằng EXAM_START_TIME_KEY
    Object.keys(localStorage).forEach((key) => {
      if (key.startsWith(EXAM_START_TIME_KEY)) {
        localStorage.removeItem(key);
      }
    });

    // Xóa tất cả các key bắt đầu bằng OTP_EXPIRED_KEY
    Object.keys(localStorage).forEach((key) => {
      if (key.startsWith(OTP_EXPIRED_KEY)) {
        localStorage.removeItem(key);
      }
    });

    // Xóa tất cả các key bắt đầu bằng OTP_DATA_KEY
    Object.keys(localStorage).forEach((key) => {
      if (key.startsWith(OTP_DATA_KEY)) {
        localStorage.removeItem(key);
      }
    });
  }

  saveUserStorage(user: UserInfo) {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  }

  getUserStorage() {
    if (localStorage.getItem(USER_KEY)) {
      return JSON.parse(localStorage?.getItem(USER_KEY) || '') as UserInfo;
    }
    return null;
  }

  saveStudentIdStorage(studentId: string) {
    localStorage.setItem(STUDENT_ID_KEY, studentId);
  }

  getStudentIdStorage() {
    return localStorage.getItem(STUDENT_ID_KEY) || '';
  }

  saveTimeRemainingStorage = (examId: string, timeRemaining: number) => {
    localStorage.setItem(`${TIME_REMAINING_KEY}_${examId}`, timeRemaining.toString());
  };

  getTimeRemainingStorage = (examId: string) => {
    const timeRemaining = localStorage.getItem(`${TIME_REMAINING_KEY}_${examId}`);
    return timeRemaining ? parseInt(timeRemaining, 10) : 0;
  };

  saveExtraStartTime = (examId: string, extraStartTime: number) => {
    localStorage.setItem(`${EXTRA_START_TIME_KEY}_${examId}`, extraStartTime.toString());
  };

  getExtraStartTime = (examId: string) => {
    const extraStartTime = localStorage.getItem(`${EXTRA_START_TIME_KEY}_${examId}`);
    return extraStartTime ? parseInt(extraStartTime, 10) : 0;
  };

  saveSelectedCampus = (campus: string) => {
    localStorage.setItem(SELECTED_CAMPUS_KEY, campus);
  };

  getSelectedCampus = () => {
    return localStorage.getItem(SELECTED_CAMPUS_KEY) || '';
  };

  saveOtpExpiredStorage = (examId: string, expiredAt: string) => {
    localStorage.setItem(`${OTP_EXPIRED_KEY}_${examId}`, expiredAt);
  };

  getOtpExpiredStorage = (examId: string) => {
    return localStorage.getItem(`${OTP_EXPIRED_KEY}_${examId}`) || '';
  };

  saveOtpDataStorage = (examId: string, otpData: string) => {
    localStorage.setItem(`${OTP_DATA_KEY}_${examId}`, otpData);
  };

  getOtpDataStorage = (examId: string) => {
    return localStorage.getItem(`${OTP_DATA_KEY}_${examId}`) || '';
  };
}

export default new Services();
