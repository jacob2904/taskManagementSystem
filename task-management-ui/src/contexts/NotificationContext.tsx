import { createContext, useContext } from 'react';
import type { ReactNode } from 'react';
import { useNotifications } from '../hooks/useNotifications';
import type { TaskNotification } from '../services/signalRService';

interface NotificationContextType {
  notifications: TaskNotification[];
  isConnected: boolean;
  clearNotifications: () => void;
  removeNotification: (index: number) => void;
  removeNotificationByTaskId: (taskId: number) => void;
  requestNotificationPermission: () => Promise<void>;
}

const NotificationContext = createContext<NotificationContextType | undefined>(undefined);

export const NotificationProvider = ({ children }: { children: ReactNode }) => {
  const notificationHook = useNotifications();

  return (
    <NotificationContext.Provider value={notificationHook}>
      {children}
    </NotificationContext.Provider>
  );
};

export const useNotificationContext = () => {
  const context = useContext(NotificationContext);
  if (context === undefined) {
    throw new Error('useNotificationContext must be used within a NotificationProvider');
  }
  return context;
};
