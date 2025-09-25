import React, { createContext, useContext, useEffect, useState } from 'react';
import createSignalRService from '@/services/signalRService';
import { SIGNALR_URL } from '@/consts/apiUrl';

const SignalRContext = createContext<{
  signalRService: ReturnType<typeof createSignalRService> | null;
}>({ signalRService: null });

export const SignalRProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [signalRService, setSignalRService] = useState<ReturnType<
    typeof createSignalRService
  > | null>(null);

  useEffect(() => {
    const service = createSignalRService(SIGNALR_URL);
    setSignalRService(service);

    service.start().catch((error) => {
      console.error('Failed to start SignalR:', error);
    });

    return () => {
      service.stop().catch((error) => {
        console.error('Failed to stop SignalR:', error);
      });
    };
  }, []);

  return <SignalRContext.Provider value={{ signalRService }}>{children}</SignalRContext.Provider>;
};

export const useSignalR = () => useContext(SignalRContext);
