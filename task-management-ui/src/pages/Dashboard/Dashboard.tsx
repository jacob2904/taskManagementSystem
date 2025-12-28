import { useEffect, useState } from 'react';
import {
  Grid,
  Card,
  CardContent,
  Typography,
  Box,
  CircularProgress,
  Alert,
  Paper,
  List,
  ListItem,
  ListItemText,
  Chip,
} from '@mui/material';
import {
  Assignment as AssignmentIcon,
  PriorityHigh as PriorityHighIcon,
} from '@mui/icons-material';
import { taskService } from '../../services/api';
import { OverdueTasksWidget } from '../../components/OverdueTasksWidget/OverdueTasksWidget';
import type { Task } from '../../types';

export const Dashboard = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [overdueTasks, setOverdueTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [overdueLoading, setOverdueLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [overdueError, setOverdueError] = useState<string | null>(null);

  const loadTasks = async () => {
    try {
      setLoading(true);
      const data = await taskService.getTasks();
      setTasks(data);
      setError(null);
    } catch (err) {
      setError('Failed to load tasks');
      console.error('Error loading tasks:', err);
    } finally {
      setLoading(false);
    }
  };

  const loadOverdueTasks = async () => {
    try {
      setOverdueLoading(true);
      const data = await taskService.getOverdueTasks();
      setOverdueTasks(data);
      setOverdueError(null);
    } catch (err) {
      setOverdueError('Failed to load overdue tasks');
      console.error('Error loading overdue tasks:', err);
    } finally {
      setOverdueLoading(false);
    }
  };

  useEffect(() => {
    loadTasks();
    loadOverdueTasks();
  }, []);

  const handleMarkComplete = async (taskId: number) => {
    try {
      await taskService.markTaskComplete(taskId);
      await Promise.all([loadTasks(), loadOverdueTasks()]);
    } catch (err) {
      console.error('Error marking task complete:', err);
      setOverdueError('Failed to mark task complete');
    }
  };

  const stats = {
    total: tasks.length,
    incomplete: tasks.filter((t) => !t.isComplete).length,
    complete: tasks.filter((t) => t.isComplete).length,
    overdue: overdueTasks.length,
    highPriority: tasks.filter((t) => !t.isComplete && t.priority === 'High').length,
    mediumPriority: tasks.filter((t) => !t.isComplete && t.priority === 'Medium').length,
    lowPriority: tasks.filter((t) => !t.isComplete && t.priority === 'Low').length,
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

  if (loading && overdueLoading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="80vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Typography variant="h4" gutterBottom>
        Dashboard
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Grid container spacing={3}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Total Tasks
                  </Typography>
                  <Typography variant="h4">{stats.total}</Typography>
                </Box>
                <AssignmentIcon sx={{ fontSize: 40, opacity: 0.3 }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Incomplete
                  </Typography>
                  <Typography variant="h4">{stats.incomplete}</Typography>
                </Box>
                <AssignmentIcon sx={{ fontSize: 40, opacity: 0.3, color: 'primary.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Complete
                  </Typography>
                  <Typography variant="h4">{stats.complete}</Typography>
                </Box>
                <AssignmentIcon sx={{ fontSize: 40, opacity: 0.3, color: 'success.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card>
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" gutterBottom>
                    Overdue
                  </Typography>
                  <Typography variant="h4" color="error">{stats.overdue}</Typography>
                </Box>
                <PriorityHighIcon sx={{ fontSize: 40, opacity: 0.3, color: 'error.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12 }}>
          <OverdueTasksWidget
            overdueTasks={overdueTasks}
            loading={overdueLoading}
            error={overdueError}
            onMarkComplete={handleMarkComplete}
          />
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                High Priority
              </Typography>
              <Typography variant="h3" color="error.main">
                {stats.highPriority}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Medium Priority
              </Typography>
              <Typography variant="h3" color="warning.main">
                {stats.mediumPriority}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, md: 4 }}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Low Priority
              </Typography>
              <Typography variant="h3" color="success.main">
                {stats.lowPriority}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12 }}>
          <Paper sx={{ p: 2 }}>
            <Typography variant="h6" gutterBottom>
              Recent Incomplete Tasks
            </Typography>
            <List>
              {tasks
                .filter((t) => !t.isComplete)
                .slice(0, 5)
                .map((task) => (
                  <ListItem key={task.id}>
                    <ListItemText
                      primary={
                        <Box display="flex" alignItems="center" gap={1}>
                          <Typography>{task.title}</Typography>
                          <Chip
                            label={task.priority}
                            size="small"
                            color={getPriorityColor(task.priority)}
                          />
                        </Box>
                      }
                      secondary={task.userDetails.fullName}
                    />
                  </ListItem>
                ))}
            </List>
          </Paper>
        </Grid>
      </Grid>
    </Box>
  );
};
