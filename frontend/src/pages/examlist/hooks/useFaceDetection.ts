import { showError } from '@/helpers/toast';
import httpService from '@/services/httpService';
import faceService from '@/services/modules/faceAI/face.service';
import faceCaptureService from '@/services/modules/facecapture/facecapture.srvice';
import { useCallback, useEffect, useState } from 'react';

export interface AnalyzeFaceResponse {
  avg_arousal: number;
  avg_valence: number;
  dominant_emotion: string;
  emotions: {
    angry: number;
    disgust: number;
    fear: number;
    happy: number;
    neutral: number;
    sad: number;
    surprise: number;
  };
  inferred_state: string;
  region: {
    h: number;
    left_eye: null | { x: number; y: number };
    right_eye: null | { x: number; y: number };
    w: number;
    x: number;
    y: number;
  };
  result: string;
  status: string;
}

export interface MutipleFaceResponse {
  count: number;
  result: string;
  status: string;
}

const inferState = (valence: number, arousal: number): string => {
  if (valence > 0.5 && arousal >= 0.5 && arousal <= 0.8) return 'Confident';
  if (valence < -0.3 && arousal > 0.6) return 'Anxious';
  if (valence < 0 && arousal > 0.5) return 'Stressed';
  if (valence > 0.4 && arousal < 0.4) return 'Relaxed';
  if (valence >= 0 && valence <= 0.5 && arousal >= 0.4 && arousal <= 0.6) return 'Focused';
  if (valence >= -0.1 && valence <= 0.1 && arousal < 0.3) return 'Distracted';
  return 'Mixed';
};

const useFaceDetection = (cameraStatus: string, cameraRef: React.MutableRefObject<any>) => {
  const [emotionData, setEmotionData] = useState<AnalyzeFaceResponse | null>(null);
  const [multipleFaceDetected, setMultipleFaceDetected] = useState<MutipleFaceResponse | null>(
    null,
  );
  const [inferredState, setInferredState] = useState<string | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  // Reusable function to capture image and create FormData
  const captureImageAndCreateFormData = useCallback(async () => {
    if (!cameraRef.current) {
      showError('Camera is not available');
      return null;
    }

    try {
      await cameraRef.current.video?.play();
      const base64Image = cameraRef.current.takePhoto();
      if (!base64Image) {
        showError('Failed to capture image');
        return null;
      }

      const response = await fetch(base64Image);
      const blob = await response.blob();
      const file = new File(
        [blob],
        `camera-screenshot-${Date.now()}-${Math.random().toString(36).substring(2, 8)}.jpg`,
        { type: 'image/jpeg' },
      );

      const formData = new FormData();
      formData.append('StudentExamId', httpService.getStudentIdStorage() ?? '');
      formData.append('ImageCapture', file);
      formData.append('Description', 'Auto-captured during question navigation');
      formData.append(
        'CaptureId',
        `capture-${Date.now()}-${Math.random().toString(36).substring(2, 8)}`,
      );
      formData.append('AvgArousal', emotionData?.avg_arousal?.toString() ?? '0');
      formData.append('AvgValence', emotionData?.avg_valence?.toString() ?? '0');
      formData.append('DominantEmotion', emotionData?.dominant_emotion ?? '');
      formData.append(
        'Emotions',
        emotionData?.emotions ? JSON.stringify(emotionData.emotions) : '{}',
      );
      formData.append('InferredState', emotionData?.inferred_state ?? inferredState ?? '');
      formData.append('Region', emotionData?.region ? JSON.stringify(emotionData.region) : '{}');
      formData.append('Result', emotionData?.result ?? '');
      formData.append('Status', emotionData?.status ?? '');
      formData.append('IsDetected', emotionData?.region ? 'true' : 'false');
      formData.append('ErrorMessage', errorMsg ?? '');

      return formData;
    } catch (error) {
      showError(error);
      return null;
    }
  }, [cameraRef, emotionData, inferredState, errorMsg]);

  useEffect(() => {
    if (cameraStatus !== 'success' || !cameraRef.current) return;

    const captureScreenshot = async () => {
      try {
        const base64Image = cameraRef.current.takePhoto();
        if (!base64Image) {
          showError('Không thể chụp ảnh từ camera');
          setMultipleFaceDetected(null);
          setEmotionData(null);
          setInferredState(null);
          return;
        }

        const response = await fetch(base64Image);
        const blob = await response.blob();
        const file = new File([blob], 'camera-screenshot.jpg', { type: 'image/jpeg' });
        const formData = new FormData();
        formData.append('image', file);
        const analyzeResponse = await faceService.analyzeFace(formData);

        if (analyzeResponse.data.result === 'MultipleFacesDetected') {
          setMultipleFaceDetected(analyzeResponse.data as MutipleFaceResponse);
          setEmotionData(null);
          setInferredState(null);
          setErrorMsg(null);
        } else {
          const data = analyzeResponse.data as AnalyzeFaceResponse;
          setEmotionData(data);
          setMultipleFaceDetected(null);
          setErrorMsg(null);
          setInferredState(inferState(data.avg_valence, data.avg_arousal));
        }
      } catch (error) {
        setErrorMsg(
          (error &&
            typeof error === 'object' &&
            'response' in error &&
            (error as any).response?.data?.message) ??
            'An error occurred while capturing the screenshot',
        );
        setEmotionData(null);
        setMultipleFaceDetected(null);
        setInferredState(null);

        if (window.chrome && window.chrome.webview) {
          const message = {
            event: 'captureScreenshot',
          };
          window.chrome.webview.postMessage(message);
        } else {
          console.log('Webview API not available. Cannot send message.');
        }

        // Execute face capture on error
        const formData = await captureImageAndCreateFormData();
        if (formData) {
          try {
            await faceCaptureService.addFaceCapture(formData);
          } catch (captureError) {
            console.log(captureError);
          }
        }
      }
    };

    const screenshotInterval = setInterval(captureScreenshot, 2000);
    return () => clearInterval(screenshotInterval);
  }, [cameraStatus, cameraRef, captureImageAndCreateFormData]);

  return { emotionData, multipleFaceDetected, inferredState, errorMsg };
};

export default useFaceDetection;
