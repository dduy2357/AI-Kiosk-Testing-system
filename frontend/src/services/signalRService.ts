import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import httpService from '@/services/httpService';

type EventCallback = (...args: any[]) => void;

class SignalRService {
  public connection: HubConnection;

  constructor(signalRUrl: string) {
    this.connection = new HubConnectionBuilder()
      .withUrl(signalRUrl, {
        accessTokenFactory: () => httpService.getTokenStorage() || '',
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();
  }

  public async start(): Promise<HubConnection> {
    if (this.connection.state === 'Disconnected') {
      try {
        await this.connection.start();
        console.log('SignalR connection established');
      } catch (error) {
        console.error('SignalR Connection Error:', error);
        throw error;
      }
    }
    return this.connection;
  }

  public async stop(): Promise<void> {
    if (this.connection && this.connection.state !== 'Disconnected') {
      await this.connection.stop();
    }
  }

  public on(event: string, callback: EventCallback): void {
    this.connection.on(event, callback);
  }

  public off(event: string, callback?: EventCallback): void {
    if (callback) {
      this.connection.off(event, callback);
    } else {
      this.connection.off(event);
    }
  }

  public async invoke(method: string, ...args: any[]): Promise<any> {
    if (this.connection.state !== 'Connected') {
      await this.start();
    }
    return this.connection.invoke(method, ...args).catch((error) => {
      console.error(`SignalR invoke error for method ${method}:`, error);
      throw error;
    });
  }
}

export default function createSignalRService(signalRUrl: string): SignalRService {
  return new SignalRService(signalRUrl);
}
