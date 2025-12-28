import { useEffect, useState, useMemo } from 'react';
import {
  Box,
  Typography,
  Button,
  CircularProgress,
  Alert,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  TextField,
  InputAdornment,
} from '@mui/material';
import { Add as AddIcon, Search as SearchIcon } from '@mui/icons-material';
import { taskService, tagService } from '../../services/api';
import { TaskCard } from '../../components/TaskCard/TaskCard';
import { TaskForm } from '../../components/TaskForm/TaskForm';
import { useNotificationContext } from '../../contexts/NotificationContext';
import type { Task, Tag, CreateTaskDto } from '../../types';
import './Tasks.scss';

export const Tasks = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [tags, setTags] = useState<Tag[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [formOpen, setFormOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [taskToDelete, setTaskToDelete] = useState<number | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  
  const { removeNotificationByTaskId } = useNotificationContext();

  const loadData = async () => {
    try {
      setLoading(true);
      const [tasksData, tagsData] = await Promise.all([
        taskService.getTasks(),
        tagService.getTags(),
      ]);
      setTasks(tasksData);
      setTags(tagsData);
      setError(null);
    } catch (err) {
      setError('Failed to load data');
      console.error('Error loading data:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleCreateTask = async (taskData: CreateTaskDto) => {
    try {
      await taskService.createTask(taskData);
      await loadData();
      setFormOpen(false);
    } catch (err) {
      console.error('Error creating task:', err);
      throw err;
    }
  };

  const handleUpdateTask = async (taskData: CreateTaskDto) => {
    if (!selectedTask) return;

    try {
      await taskService.updateTask(selectedTask.id, {
        ...taskData,
        isComplete: selectedTask.isComplete,
      });
      await loadData();
      setFormOpen(false);
      setSelectedTask(null);
    } catch (err) {
      console.error('Error updating task:', err);
      throw err;
    }
  };

  const handleMarkComplete = async (taskId: number) => {
    try {
      await taskService.markTaskComplete(taskId);
      removeNotificationByTaskId(taskId);
      await loadData();
    } catch (err) {
      console.error('Error marking task complete:', err);
      setError('Failed to mark task complete');
    }
  };

  const handleDeleteTask = async () => {
    if (taskToDelete === null) return;

    try {
      await taskService.deleteTask(taskToDelete);
      removeNotificationByTaskId(taskToDelete);
      await loadData();
      setDeleteDialogOpen(false);
      setTaskToDelete(null);
    } catch (err) {
      console.error('Error deleting task:', err);
      setError('Failed to delete task');
    }
  };

  const openEditForm = (task: Task) => {
    setSelectedTask(task);
    setFormOpen(true);
  };

  const openDeleteDialog = (id: number) => {
    setTaskToDelete(id);
    setDeleteDialogOpen(true);
  };

  const closeForm = () => {
    setFormOpen(false);
    setSelectedTask(null);
  };

  const filteredTasks = useMemo(() => {
    const query = searchQuery.trim().toLowerCase();
    
    if (query.length < 3) {
      return tasks;
    }

    return tasks.filter((task) => {
      const isOverdue = new Date(task.dueDate + 'Z') < new Date() && !task.isComplete;
      
      const searchableFields = [
        task.title,
        task.description,
        task.priority,
        task.userDetails.fullName,
        task.userDetails.email,
        task.userDetails.telephone,
        ...task.tags.map(tag => tag.name),
        task.dueDate,
        task.isComplete ? 'completed' : '',
        isOverdue ? 'overdue' : '',
      ];

      return searchableFields.some(field => 
        field.toLowerCase().includes(query)
      );
    });
  }, [tasks, searchQuery]);

  if (loading) {
    return (
      <Box className="tasks-page__loading">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box className="tasks-page">
      <Box className="tasks-page__header">
        <Typography variant="h4" className="tasks-page__title">Tasks</Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setFormOpen(true)}
        >
          Add Task
        </Button>
      </Box>

      <TextField
        fullWidth
        placeholder="Search tasks (min 3 characters)..."
        value={searchQuery}
        onChange={(e) => setSearchQuery(e.target.value)}
        className="tasks-page__search"
        InputProps={{
          startAdornment: (
            <InputAdornment position="start">
              <SearchIcon />
            </InputAdornment>
          ),
        }}
      />

      <Box className="tasks-page__content">
        {error && (
          <Alert severity="error" className="tasks-page__error">
            {error}
          </Alert>
        )}

        {tasks.length === 0 ? (
          <Box className="tasks-page__empty">
            <Typography variant="h6" color="text.secondary">
              No tasks yet. Create your first task!
            </Typography>
          </Box>
        ) : filteredTasks.length === 0 ? (
          <Box className="tasks-page__empty">
            <Typography variant="h6" color="text.secondary">
              No tasks match your search.
            </Typography>
          </Box>
        ) : (
          <Box className="tasks-page__grid">
            {filteredTasks.map((task) => (
              <TaskCard
                key={task.id}
                task={task}
                onEdit={openEditForm}
                onDelete={openDeleteDialog}
                onMarkComplete={handleMarkComplete}
              />
            ))}
          </Box>
        )}
      </Box>

      <TaskForm
        open={formOpen}
        onClose={closeForm}
        onSubmit={selectedTask ? handleUpdateTask : handleCreateTask}
        task={selectedTask}
        tags={tags}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Task</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete this task? This action cannot be undone.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteTask} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};
