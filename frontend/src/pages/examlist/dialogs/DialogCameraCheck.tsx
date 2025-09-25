import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogOverlay,
  DialogPortal,
  DialogTitle,
} from '@/components/ui/dialog';
import httpService from '@/services/httpService';
import faceService from '@/services/modules/faceAI/face.service';
import { AlertTriangle, CameraIcon, CheckCircle, XCircle } from 'lucide-react';
import { useCallback, useEffect, useRef, useState } from 'react';
import { Camera } from 'react-camera-pro';
import { useTranslation } from 'react-i18next';

interface DialogCameraCheckProps {
  isOpen: boolean;
  toggle: () => void;
  onCameraSuccess: () => void;
  examTitle?: string;
}

const DialogCameraCheck = ({
  isOpen,
  toggle,
  onCameraSuccess,
  examTitle,
}: DialogCameraCheckProps) => {
  const cameraRef = useRef<any>(null);
  const { t } = useTranslation('shared');
  const [status, setStatus] = useState<'checking' | 'verifying' | 'success' | 'error'>('checking');
  const [isVerified, setIsVerified] = useState<boolean | null>(null);
  const [hasVerified, setHasVerified] = useState(false);

  const handleClose = () => {
    if (status === 'checking' || status === 'verifying') {
      return;
    }
    toggle();
  };

  const handleProceed = () => {
    onCameraSuccess();
    toggle();
  };

  const captureFrame = async (): Promise<Blob> => {
    if (!cameraRef.current?.takePhoto) {
      throw new Error('Camera not ready');
    }

    const imageData = await cameraRef.current.takePhoto();
    if (!imageData?.startsWith('data:image')) {
      throw new Error('Invalid image or camera not accessible');
    }

    const blob = await fetch(imageData).then((res) => res.blob());
    return blob;
  };

  const verifyFace = useCallback(async () => {
    setStatus('verifying');
    try {
      const blob = await captureFrame();

      const formData = new FormData();
      formData.append('image_file', blob, 'frame.jpg');

      const avatarUrl = httpService.getUserStorage()?.avatarUrl ?? '';
      formData.append('image_url', avatarUrl);

      const { data } = await faceService.verifyFace(formData);
      const verified = data?.verified;

      setIsVerified(verified);
      setStatus(verified ? 'success' : 'error');
    } catch {
      setIsVerified(false);
      setStatus('error');
    } finally {
      setHasVerified(true);
    }
  }, []);

  // Poll camera readiness
  useEffect(() => {
    if (!isOpen || hasVerified) return;

    let attempts = 0;
    const maxAttempts = 10;
    const interval = setInterval(async () => {
      try {
        const photo = await cameraRef.current?.takePhoto?.();
        if (photo?.startsWith('data:image')) {
          clearInterval(interval);
          await verifyFace();
        } else {
          throw new Error();
        }
      } catch {
        if (++attempts >= maxAttempts) {
          clearInterval(interval);
          setStatus('error');
        }
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [isOpen, hasVerified, verifyFace]);

  // Reset when dialog closes
  useEffect(() => {
    if (!isOpen) setHasVerified(false);
  }, [isOpen]);

  // Check if camera disconnected after verification
  useEffect(() => {
    if (!isOpen || status !== 'success') return;

    const interval = setInterval(async () => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        if (!stream.active) setStatus('error');
        stream.getTracks().forEach((track) => track.stop());
      } catch {
        setStatus('error');
      }
    }, 2000);

    return () => clearInterval(interval);
  }, [isOpen, status]);

  const renderStatus = () => {
    const base = 'flex items-center justify-center space-x-3';
    switch (status) {
      case 'checking':
      case 'verifying':
        return (
          <div className="flex flex-col items-center justify-center space-y-6 py-8">
            <div className="relative">
              <div className="h-16 w-16 animate-spin rounded-full border-4 border-blue-200 border-t-blue-600"></div>
              <div className="absolute inset-0 flex items-center justify-center">
                <CameraIcon className="h-6 w-6 text-blue-600" />
              </div>
            </div>
            <div className="text-center">
              <p className="text-lg font-medium text-gray-800">
                {status === 'checking'
                  ? t('ExamList.CheckingCamera')
                  : t('ExamList.VerifyingIdentity')}
              </p>
              <p className="text-sm text-gray-500">
                {status === 'checking'
                  ? t('ExamList.CheckingCamera')
                  : t('ExamList.VerifyingIdentity')}
              </p>
            </div>
          </div>
        );
      case 'success':
        return (
          <div className="rounded-xl bg-gradient-to-r from-green-50 to-emerald-50 p-6">
            <div className={`${base} text-green-700`}>
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-green-100">
                <CheckCircle className="h-6 w-6" />
              </div>
              <div>
                <p className="text-lg font-semibold">{t('ExamList.VerificationSuccess')}</p>
              </div>
            </div>
          </div>
        );
      case 'error':
        return (
          <div className="rounded-xl bg-gradient-to-r from-red-50 to-pink-50 p-6">
            <div className={`${base} text-red-700`}>
              <div className="flex h-12 w-12 items-center justify-center rounded-full bg-red-100">
                <XCircle className="h-6 w-6" />
              </div>
              <div>
                <p className="text-lg font-semibold">
                  {isVerified === false
                    ? t('ExamList.VerificationFailed')
                    : t('ExamList.CameraAccessDenied')}
                </p>
                <p className="text-sm text-red-600">
                  {isVerified === false
                    ? t('ExamList.VerificationFailedDescription')
                    : t('ExamList.CameraAccessDeniedDescription')}
                </p>
              </div>
            </div>
          </div>
        );
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={handleClose}>
      <DialogOverlay className="fixed inset-0 bg-black/30 backdrop-blur-sm" />
      <DialogPortal>
        <DialogContent className="max-w-3xl border-0 bg-white p-0 shadow-2xl">
          <div className="relative overflow-hidden rounded-lg">
            <div className="bg-gradient-to-r from-blue-600 to-purple-600 px-8 py-6 text-white">
              <div className="flex items-center space-x-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-white/20 backdrop-blur-sm">
                  <CameraIcon className="h-6 w-6" />
                </div>
                <div>
                  <DialogTitle className="text-2xl font-bold">
                    {t('ExamList.CameraCheck')}
                  </DialogTitle>
                  {examTitle && (
                    <p className="mt-1 text-blue-100">
                      Bài thi: <span className="font-semibold text-white">{examTitle}</span>
                    </p>
                  )}
                </div>
              </div>
            </div>

            <div className="p-8">
              <div className="mb-6">{renderStatus()}</div>

              <div className="relative overflow-hidden rounded-2xl bg-gradient-to-br from-gray-100 to-gray-200 shadow-inner">
                <div className="aspect-video">
                  <Camera
                    ref={cameraRef}
                    facingMode="user"
                    aspectRatio={16 / 9}
                    errorMessages={{
                      noCameraAccessible: t('ExamList.NoCameraAccessible'),
                      permissionDenied: t('ExamList.PermissionDenied'),
                      switchCamera: t('ExamList.SwitchCamera'),
                      canvas: t('ExamList.Canvas'),
                    }}
                  />
                </div>

                {status === 'success' && (
                  <div className="absolute right-4 top-4">
                    <div className="flex items-center space-x-2 rounded-full bg-red-500 px-3 py-1.5 text-sm font-medium text-white shadow-lg">
                      <div className="h-2 w-2 animate-pulse rounded-full bg-white"></div>
                      <span>LIVE</span>
                    </div>
                  </div>
                )}

                {status !== 'success' && (
                  <div className="absolute inset-0 flex items-center justify-center bg-gray-900/20 backdrop-blur-sm">
                    <div className="rounded-xl bg-white/90 p-6 text-center shadow-lg backdrop-blur-sm">
                      <div className="flex items-center space-x-3">
                        {status === 'checking' || status === 'verifying' ? (
                          <>
                            <div className="h-5 w-5 animate-spin rounded-full border-2 border-blue-200 border-t-blue-600" />
                            <span className="text-gray-700">
                              {status === 'checking'
                                ? 'Đang kết nối camera...'
                                : 'Đang xác minh danh tính...'}
                            </span>
                          </>
                        ) : (
                          <>
                            <AlertTriangle className="h-5 w-5 text-red-600" />
                            <span className="text-red-600">{t('ExamList.CameraNotAvailable')}</span>
                          </>
                        )}
                      </div>
                    </div>
                  </div>
                )}
              </div>

              <div className="mt-8 flex justify-end space-x-3">
                <Button
                  variant="outline"
                  onClick={handleClose}
                  disabled={status === 'checking' || status === 'verifying'}
                  className="px-6 py-2.5 font-medium transition-all hover:bg-gray-50"
                >
                  Hủy bỏ
                </Button>

                {status === 'success' && (
                  <Button
                    onClick={handleProceed}
                    className="bg-gradient-to-r from-blue-600 to-purple-600 px-8 py-2.5 font-semibold text-white shadow-lg transition-all hover:from-blue-700 hover:to-purple-700 hover:shadow-xl"
                  >
                    {t('ExamList.StartYourExamination')}
                  </Button>
                )}

                {status === 'error' && (
                  <Button
                    onClick={() => {
                      setStatus('checking');
                      setHasVerified(false);
                    }}
                    variant="outline"
                    className="border-blue-200 px-6 py-2.5 font-medium text-blue-600 transition-all hover:bg-blue-50"
                  >
                    {t('ExamList.TryAgain')}
                  </Button>
                )}
              </div>
            </div>
          </div>
        </DialogContent>
      </DialogPortal>
    </Dialog>
  );
};

export default DialogCameraCheck;
