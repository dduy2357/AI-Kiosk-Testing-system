import { ImageSource } from '@/assets';
import { isString } from 'lodash';
import { toast, ToastOptions } from 'react-toastify';

export const toastContainer = {
  success: 'bg-toast-success border-toast-borderSuccess',
  error: 'bg-toast-error border-toast-borderError',
  info: 'bg-gray-600',
  warning: 'bg-orange-400',
  default: 'bg-indigo-600',
  dark: 'bg-white-600 font-gray-300',
};

export const toastBody = {
  success: ' text-toast-textSuccess',
  error: ' text-toast-textError',
  info: ' text-toast-textSuccess',
  warning: ' text-toast-textSuccess',
  default: ' text-toast-textSuccess',
  dark: ' text-toast-textSuccess ',
};

export const toastIcons = {
  success: ImageSource.SuccessIcon,
  error: ImageSource.ErrorIcon,
  info: ImageSource.SuccessIcon,
  warning: ImageSource.SuccessIcon,
  default: ImageSource.SuccessIcon,
  dark: ImageSource.SuccessIcon,
};

export const toastCloseIcons = {
  success: ImageSource.CloseSuccess,
  error: ImageSource.CloseErrorIcon,
  info: ImageSource.SuccessIcon,
  warning: ImageSource.SuccessIcon,
  default: ImageSource.SuccessIcon,
  dark: ImageSource.SuccessIcon,
};

export const showSuccess = (
  msg: any,
  //  options?: ToastOptions
) => {
  if (isString(msg)) {
    toast.success(msg, {
      style: {
        color: 'green', // Đặt màu chữ thành đen
        backgroundColor: 'white', // Đặt màu nền thành trắng
      },
    });
    return;
  }

  toast.success('Error default');
};

export const showError = (error: any, options?: ToastOptions) => {
  const errorStyle = {
    style: {
      color: 'red',
      backgroundColor: 'white',
    },
    ...options,
  };

  if (error?.response) {
    if (error?.response?.data?.Error) {
      toast.error(JSON.stringify(error?.response?.data?.errors), errorStyle);
      return;
    }

    if (error?.response?.data?.title) {
      toast.error(JSON.stringify(error?.response?.data?.title), errorStyle);
      return;
    }

    if (error?.response?.data?.message) {
      toast.error(error?.response?.data?.message, errorStyle);
      return;
    }
  }

  if (isString(error) || isString(error?.toString())) {
    toast.error(error, errorStyle);
    return;
  }

  toast.error('Error default', errorStyle);
};
