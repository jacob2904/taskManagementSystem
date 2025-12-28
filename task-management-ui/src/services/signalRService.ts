import * as signalR from "@microsoft/signalr";
import { authService } from "./authService";
import { config } from "../config/env";

export interface TaskNotification {
  taskId: number;
  taskTitle: string;
  dueDate: string;
  timestamp: string;
  message: string;
}

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private listeners: Array<(notification: TaskNotification) => void> = [];
  private startPromise: Promise<void> | null = null;

  async start(): Promise<void> {
    // If already connected or connecting, return existing promise
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }
    if (this.startPromise) {
      return this.startPromise;
    }

    // Get JWT token for authentication
    const token = authService.getToken();

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(config.signalRHubUrl, {
        skipNegotiation: false,
        transport:
          signalR.HttpTransportType.WebSockets |
          signalR.HttpTransportType.ServerSentEvents |
          signalR.HttpTransportType.LongPolling,
        accessTokenFactory: () => token || "",
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.on(
      "ReceiveTaskNotification",
      (notification: TaskNotification) => {
        console.log("Received notification:", notification);
        this.listeners.forEach((listener) => listener(notification));
      }
    );

    this.connection.onreconnecting((error) => {
      console.warn("SignalR reconnecting:", error);
    });

    this.connection.onreconnected((connectionId) => {
      console.log("SignalR reconnected:", connectionId);
    });

    this.connection.onclose((error) => {
      console.error("SignalR connection closed:", error);
    });

    this.startPromise = (async () => {
      try {
        await this.connection!.start();
        console.log("SignalR connected successfully");
      } catch (error) {
        console.error("Error connecting to SignalR:", error);
        this.connection = null;
        throw error;
      } finally {
        this.startPromise = null;
      }
    })();

    return this.startPromise;
  }

  async stop(): Promise<void> {
    // Only stop if no listeners remain
    if (this.connection && this.listeners.length === 0) {
      await this.connection.stop();
      this.connection = null;
      this.startPromise = null;
      console.log("SignalR disconnected");
    }
  }

  onNotification(
    callback: (notification: TaskNotification) => void
  ): () => void {
    this.listeners.push(callback);

    // Return unsubscribe function
    return () => {
      this.listeners = this.listeners.filter(
        (listener) => listener !== callback
      );
    };
  }

  getConnectionState(): signalR.HubConnectionState | null {
    return this.connection?.state ?? null;
  }
}

export const signalRService = new SignalRService();
