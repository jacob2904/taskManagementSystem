import { useEffect, useState } from 'react';
import { signalRService } from '../services/signalRService';
import type { TaskNotification } from '../services/signalRService';
import { authService } from '../services/authService';

export const useNotifications = () => {
  const [notifications, setNotifications] = useState<TaskNotification[]>([]);
  const [isConnected, setIsConnected] = useState(false);

  useEffect(() => {
    let isMounted = true;

    const connectToHub = async () => {
      // Only connect if user is authenticated
      if (!authService.isAuthenticated()) {
        console.log('User not authenticated, skipping SignalR connection');
        return;
      }

      try {
        await signalRService.start();
        if (isMounted) {
          setIsConnected(true);
        }
      } catch (error) {
        if (isMounted) {
          console.error('Failed to connect to notification hub:', error);
          setIsConnected(false);
        }
      }
    };

    connectToHub();

    const unsubscribe = signalRService.onNotification((notification) => {
      setNotifications(prev => [notification, ...prev]);
      
      // Show browser notification if permission granted
      if (Notification.permission === 'granted') {
        new Notification('Task Reminder', {
          body: notification.message,
          icon: '/vite.svg',
          tag: `task-${notification.taskId}`,
        });
      }
    });

    return () => {
      isMounted = false;
      unsubscribe();
    };
  }, []);

  const clearNotifications = () => {
    setNotifications([]);
  };

  const removeNotification = (index: number) => {
    setNotifications(prev => prev.filter((_, i) => i !== index));
  };

  const removeNotificationByTaskId = (taskId: number) => {
    setNotifications(prev => prev.filter(n => n.taskId !== taskId));
  };

  const requestNotificationPermission = async () => {
    if ('Notification' in window && Notification.permission === 'default') {
      await Notification.requestPermission();
    }
  };

  return {
    notifications,
    isConnected,
    clearNotifications,
    removeNotification,
    removeNotificationByTaskId,
    requestNotificationPermission,
  };
};
