import { useEffect, useRef, useState } from 'react';

const useCameraMonitor = () => {
  const [cameraStatus, setCameraStatus] = useState<'checking' | 'success' | 'error'>('checking');
  const [showCameraWarning, setShowCameraWarning] = useState(false);
  const [cameraKey, setCameraKey] = useState(0);
  const cameraRef = useRef<any>(null);
  const streamRef = useRef<MediaStream | null>(null);

  useEffect(() => {
    let cameraCheckInterval: NodeJS.Timeout | null = null;

    const stopCurrentStream = () => {
      if (streamRef.current) {
        streamRef.current.getTracks().forEach((track) => track.stop());
        streamRef.current = null;
      }
    };

    const checkAndMonitorCamera = async () => {
      try {
        stopCurrentStream();
        const stream = await navigator.mediaDevices.getUserMedia({ video: true });
        streamRef.current = stream;
        if (stream.active && cameraRef.current) {
          setCameraStatus('success');
          setShowCameraWarning(false);
          setCameraKey((prev) => prev + 1);
          const videoTrack = stream.getVideoTracks()[0];
          if (videoTrack) {
            videoTrack.addEventListener('ended', () => {
              setCameraStatus('error');
              setShowCameraWarning(true);
            });
          }
        } else {
          throw new Error('Stream not active or camera ref not available');
        }
      } catch (error) {
        console.error('Camera access error:', error);
        setCameraStatus('error');
        setShowCameraWarning(true);
      }
    };

    checkAndMonitorCamera();
    if (cameraStatus === 'error') {
      cameraCheckInterval = setInterval(checkAndMonitorCamera, 2000);
    }

    return () => {
      stopCurrentStream();
      if (cameraCheckInterval) clearInterval(cameraCheckInterval);
    };
  }, [cameraStatus]);

  return {
    cameraStatus,
    showCameraWarning,
    cameraRef,
    cameraKey,
    setCameraStatus,
    setShowCameraWarning,
  };
};

export default useCameraMonitor;
