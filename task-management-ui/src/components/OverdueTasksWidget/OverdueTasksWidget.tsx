import { useState } from 'react';
import {
  Card,
  CardContent,
  Typography,
  List,
  ListItem,
  ListItemText,
  ListItemButton,
  Chip,
  Box,
  IconButton,
  Alert,
  CircularProgress,
} from '@mui/material';
import {
  CheckCircle as CheckCircleIcon,
  Warning as WarningIcon,
} from '@mui/icons-material';
import { formatDistanceToNow } from 'date-fns';
import type { Task } from '../../types';
import { formatIsraelDateTime } from '../../utils/timezone';

interface OverdueTasksWidgetProps {
  overdueTasks: Task[];
  loading: boolean;
  error: string | null;
  onMarkComplete: (taskId: number) => Promise<void>;
}

export const OverdueTasksWidget = ({
  overdueTasks,
  loading,
  error,
  onMarkComplete,
}: OverdueTasksWidgetProps) => {
  const [completingTaskId, setCompletingTaskId] = useState<number | null>(null);

  const handleMarkComplete = async (taskId: number) => {
    setCompletingTaskId(taskId);
    try {
      await onMarkComplete(taskId);
    } finally {
      setCompletingTaskId(null);
    }
  };

  const getPriorityColor = (priority: string) => {
    switch (priority) {
      case 'High':
        return 'error';
      case 'Medium':
        return 'warning';
      case 'Low':
        return 'success';
      default:
        return 'default';
    }
  };

  if (loading) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <WarningIcon color="error" />
            Overdue Tasks
          </Typography>
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        </CardContent>
      </Card>
    );
  }

  if (error) {
    return (
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <WarningIcon color="error" />
            Overdue Tasks
          </Typography>
          <Alert severity="error">{error}</Alert>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardContent>
        <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <WarningIcon color="error" />
          Overdue Tasks
          {overdueTasks.length > 0 && (
            <Chip
              label={overdueTasks.length}
              color="error"
              size="small"
            />
          )}
        </Typography>

        {overdueTasks.length === 0 ? (
          <Typography color="text.secondary" sx={{ py: 2 }}>
            No overdue tasks! 🎉
          </Typography>
        ) : (
          <List>
            {overdueTasks.map((task) => (
              <ListItem
                key={task.id}
                disablePadding
                secondaryAction={
                  <IconButton
                    edge="end"
                    color="success"
                    onClick={() => handleMarkComplete(task.id)}
                    disabled={completingTaskId === task.id}
                    aria-label="mark complete"
                  >
                    {completingTaskId === task.id ? (
                      <CircularProgress size={24} />
                    ) : (
                      <CheckCircleIcon />
                    )}
                  </IconButton>
                }
              >
                <ListItemButton>
                  <ListItemText
                    primary={
                      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                        <Typography variant="body1">{task.title}</Typography>
                        <Chip
                          label={task.priority}
                          size="small"
                          color={getPriorityColor(task.priority)}
                        />
                      </Box>
                    }
                    secondary={
                      <Box>
                        <Typography variant="body2" color="text.secondary">
                          {task.userDetails.fullName}
                        </Typography>
                        <Typography variant="caption" color="error">
                          Due {formatDistanceToNow(new Date(task.dueDate + 'Z'), { addSuffix: true })}
                          {' • '}
                          {formatIsraelDateTime(task.dueDate)}
                        </Typography>
                      </Box>
                    }
                  />
                </ListItemButton>
              </ListItem>
            ))}
          </List>
        )}
      </CardContent>
    </Card>
  );
};
