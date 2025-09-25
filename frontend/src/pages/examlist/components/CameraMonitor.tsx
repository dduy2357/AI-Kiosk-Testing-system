import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { AlertTriangle, CameraIcon } from 'lucide-react';
import React, { useRef } from 'react';
import { Camera } from 'react-camera-pro';
import { AnalyzeFaceResponse } from '../hooks/useFaceDetection';
import { useTranslation } from 'react-i18next';

interface CameraMonitorProps {
  cameraStatus: 'checking' | 'success' | 'error';
  cameraRef: React.MutableRefObject<any>;
  cameraKey: number;
  emotionData?: AnalyzeFaceResponse | null;
}

const CameraMonitor: React.FC<CameraMonitorProps> = React.memo(
  ({ cameraStatus, cameraRef, cameraKey, emotionData }) => {
    const { t } = useTranslation('shared');
    const videoContainerRef = useRef<HTMLDivElement>(null);

    const getBoundingBoxStyles = () => {
      if (!emotionData || !videoContainerRef.current) return {};

      const videoContainer = videoContainerRef.current;
      const videoWidth = videoContainer.offsetWidth;
      const videoHeight = videoContainer.offsetHeight;

      const scaleX = videoWidth / 640;
      const scaleY = videoHeight / 360;

      return {
        position: 'absolute' as const,
        left: `${emotionData.region.x * scaleX}px`,
        top: `${emotionData.region.y * scaleY}px`,
        width: `${emotionData.region.w * scaleX}px`,
        height: `${emotionData.region.h * scaleY}px`,
        border: '2px solid red',
        boxSizing: 'border-box' as const,
        pointerEvents: 'none' as const,
      };
    };

    return (
      <Card>
        <CardHeader className="pb-3">
          <CardTitle className="flex items-center text-sm">
            <CameraIcon className="mr-2 h-4 w-4" />
            {t('ExamList.CameraMonitorTitle')}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="relative overflow-hidden rounded-lg bg-gray-100" ref={videoContainerRef}>
            <div className="aspect-video">
              <Camera
                key={cameraKey}
                ref={cameraRef}
                facingMode="user"
                aspectRatio={15 / 9}
                errorMessages={{
                  noCameraAccessible: t('ExamList.NoCameraAccessible'),
                  permissionDenied: t('ExamList.PermissionDenied'),
                  switchCamera: t('ExamList.SwitchCamera'),
                  canvas: t('ExamList.Canvas'),
                }}
              />
              {emotionData && cameraStatus === 'success' && <div style={getBoundingBoxStyles()} />}
            </div>
            <div className="absolute right-2 top-2">
              {cameraStatus === 'success' && (
                <div className="flex items-center space-x-1 rounded-full bg-green-500 px-2 py-1 text-xs font-medium text-white">
                  <div className="h-1.5 w-1.5 animate-pulse rounded-full bg-white"></div>
                  <span>REC</span>
                </div>
              )}
              {cameraStatus === 'error' && (
                <div className="flex items-center space-x-1 rounded-full bg-red-500 px-2 py-1 text-xs font-medium text-white">
                  <AlertTriangle className="h-3 w-3" />
                  <span>ERROR</span>
                </div>
              )}
              {cameraStatus === 'checking' && (
                <div className="flex items-center space-x-1 rounded-full bg-yellow-500 px-2 py-1 text-xs font-medium text-white">
                  <div className="h-1.5 w-1.5 animate-spin rounded-full border border-white border-t-transparent"></div>
                  <span>INIT</span>
                </div>
              )}
            </div>
            {cameraStatus !== 'success' && (
              <div className="absolute inset-0 flex items-center justify-center bg-gray-900/50 backdrop-blur-sm">
                <div className="rounded-lg bg-white/90 p-3 text-center">
                  {cameraStatus === 'checking' ? (
                    <div className="flex flex-col items-center space-y-2 text-sm">
                      <div className="h-4 w-4 animate-spin rounded-full border-2 border-blue-200 border-t-blue-600"></div>
                      <span>{t('ExamList.CameraConnecting')}</span>
                    </div>
                  ) : (
                    <div className="flex flex-col items-center space-y-2 text-sm text-red-600">
                      <AlertTriangle className="h-4 w-4" />
                      <span>{t('ExamList.CameraNotAvailable')}</span>
                      <p className="text-xs text-gray-500">
                        {t('ExamList.CameraNotAvailableDescription')}
                      </p>
                      <Button
                        onClick={() => window.location.reload()}
                        className="mt-2 rounded bg-red-600 px-2 py-1 text-xs text-white hover:bg-red-700"
                      >
                        Tải lại
                      </Button>
                    </div>
                  )}
                </div>
              </div>
            )}
          </div>
          <div className="mt-2 text-center">
            {cameraStatus === 'success' && (
              <p className="text-xs text-green-600">{t('ExamList.CameraIsActive')}</p>
            )}
            {cameraStatus === 'error' && (
              <p className="text-xs text-red-600">{t('ExamList.CheckCamera')}</p>
            )}
            {cameraStatus === 'checking' && (
              <p className="text-xs text-yellow-600">{t('ExamList.CheckingCamera')}</p>
            )}
          </div>
        </CardContent>
      </Card>
    );
  },
);

export default CameraMonitor;
