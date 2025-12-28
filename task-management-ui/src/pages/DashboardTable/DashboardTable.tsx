import { useEffect, useState } from 'react';
import {
  Grid,
  Box,
  Typography,
  CircularProgress,
  Alert,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Checkbox,
  Tooltip,
  TableSortLabel,
  TextField,
  InputAdornment,
  Stack,
  Card,
  CardContent,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
  Search as SearchIcon,
  Assignment as AssignmentIcon,
  PriorityHigh as PriorityHighIcon,
} from '@mui/icons-material';
import { taskService } from '../../services/api';
import type { Task } from '../../types';
import { formatIsraelDateTime } from '../../utils/timezone';
import './DashboardTable.scss';

type OrderDirection = 'asc' | 'desc';
type SortableField = 'title' | 'dueDate' | 'priority' | 'userDetails.fullName' | 'isComplete';

export const DashboardTable = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [orderBy, setOrderBy] = useState<SortableField>('dueDate');
  const [order, setOrder] = useState<OrderDirection>('asc');

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

  useEffect(() => {
    loadTasks();
  }, []);

  const handleMarkComplete = async (taskId: number) => {
    try {
      await taskService.markTaskComplete(taskId);
      await loadTasks();
    } catch (err) {
      console.error('Error marking task complete:', err);
      setError('Failed to mark task complete');
    }
  };

  const handleSort = (field: SortableField) => {
    const isAsc = orderBy === field && order === 'asc';
    setOrder(isAsc ? 'desc' : 'asc');
    setOrderBy(field);
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

  const getPriorityWeight = (priority: string): number => {
    switch (priority) {
      case 'High':
        return 3;
      case 'Medium':
        return 2;
      case 'Low':
        return 1;
      default:
        return 0;
    }
  };

  const getNestedValue = (obj: Task, path: string): any => {
    return path.split('.').reduce((acc, part) => acc?.[part], obj as any);
  };

  const filteredAndSortedTasks = tasks
    .filter((task) => {
      const searchLower = searchQuery.toLowerCase();
      return (
        task.title.toLowerCase().includes(searchLower) ||
        task.description.toLowerCase().includes(searchLower) ||
        task.userDetails.fullName.toLowerCase().includes(searchLower) ||
        task.tags.some((tag) => tag.name.toLowerCase().includes(searchLower))
      );
    })
    .sort((a, b) => {
      let aValue: any;
      let bValue: any;

      if (orderBy === 'priority') {
        aValue = getPriorityWeight(a.priority);
        bValue = getPriorityWeight(b.priority);
      } else if (orderBy === 'isComplete') {
        aValue = a.isComplete ? 1 : 0;
        bValue = b.isComplete ? 1 : 0;
      } else {
        aValue = getNestedValue(a, orderBy);
        bValue = getNestedValue(b, orderBy);
      }

      if (aValue < bValue) {
        return order === 'asc' ? -1 : 1;
      }
      if (aValue > bValue) {
        return order === 'asc' ? 1 : -1;
      }
      return 0;
    });

  const stats = {
    total: tasks.length,
    incomplete: tasks.filter((t) => !t.isComplete).length,
    complete: tasks.filter((t) => t.isComplete).length,
    overdue: tasks.filter((t) => new Date(t.dueDate + 'Z') < new Date() && !t.isComplete).length,
  };

  if (loading) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="80vh">
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box className="dashboard-table">
      <Typography variant="h4" gutterBottom className="dashboard-table__title">
        Task Dashboard
      </Typography>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {/* Stats Cards */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card className="dashboard-table__stat-card">
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" variant="body2">
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
          <Card className="dashboard-table__stat-card">
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" variant="body2">
                    Incomplete
                  </Typography>
                  <Typography variant="h4" color="primary.main">{stats.incomplete}</Typography>
                </Box>
                <AssignmentIcon sx={{ fontSize: 40, opacity: 0.3, color: 'primary.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card className="dashboard-table__stat-card">
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" variant="body2">
                    Complete
                  </Typography>
                  <Typography variant="h4" color="success.main">{stats.complete}</Typography>
                </Box>
                <AssignmentIcon sx={{ fontSize: 40, opacity: 0.3, color: 'success.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>

        <Grid size={{ xs: 12, sm: 6, md: 3 }}>
          <Card className="dashboard-table__stat-card">
            <CardContent>
              <Box display="flex" alignItems="center" justifyContent="space-between">
                <Box>
                  <Typography color="text.secondary" variant="body2">
                    Overdue
                  </Typography>
                  <Typography variant="h4" color="error.main">{stats.overdue}</Typography>
                </Box>
                <PriorityHighIcon sx={{ fontSize: 40, opacity: 0.3, color: 'error.main' }} />
              </Box>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Search Bar */}
      <Box sx={{ mb: 3 }}>
        <TextField
          fullWidth
          variant="outlined"
          placeholder="Search tasks by title, description, assignee, or tags..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
          className="dashboard-table__search"
        />
      </Box>

      {/* Table */}
      <TableContainer component={Paper} className="dashboard-table__container" elevation={0}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell padding="checkbox" align="center">
                <Tooltip title="Status">
                  <TableSortLabel
                    active={orderBy === 'isComplete'}
                    direction={orderBy === 'isComplete' ? order : 'asc'}
                    onClick={() => handleSort('isComplete')}
                  >
                    Status
                  </TableSortLabel>
                </Tooltip>
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={orderBy === 'title'}
                  direction={orderBy === 'title' ? order : 'asc'}
                  onClick={() => handleSort('title')}
                >
                  Task
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={orderBy === 'userDetails.fullName'}
                  direction={orderBy === 'userDetails.fullName' ? order : 'asc'}
                  onClick={() => handleSort('userDetails.fullName')}
                >
                  Assignee
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={orderBy === 'priority'}
                  direction={orderBy === 'priority' ? order : 'asc'}
                  onClick={() => handleSort('priority')}
                >
                  Priority
                </TableSortLabel>
              </TableCell>
              <TableCell>
                <TableSortLabel
                  active={orderBy === 'dueDate'}
                  direction={orderBy === 'dueDate' ? order : 'asc'}
                  onClick={() => handleSort('dueDate')}
                >
                  Due Date
                </TableSortLabel>
              </TableCell>
              <TableCell>Tags</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredAndSortedTasks.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">
                    {searchQuery ? 'No tasks found matching your search' : 'No tasks available'}
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              filteredAndSortedTasks.map((task) => {
                const dueDate = new Date(task.dueDate + 'Z'); // Parse as UTC
                const isOverdue = dueDate < new Date() && !task.isComplete;
                return (
                  <TableRow
                    key={task.id}
                    className={`dashboard-table__row ${task.isComplete ? 'completed' : ''} ${
                      isOverdue ? 'overdue' : ''
                    }`}
                  >
                    <TableCell padding="checkbox" align="center">
                      <Tooltip title={task.isComplete ? 'Completed' : 'Mark as complete'}>
                        <Checkbox
                          checked={task.isComplete}
                          onChange={() => handleMarkComplete(task.id)}
                          disabled={task.isComplete}
                          icon={<CheckCircleIcon />}
                          checkedIcon={<CheckCircleIcon />}
                          color="success"
                        />
                      </Tooltip>
                    </TableCell>
                    <TableCell>
                      <Box>
                        <Typography
                          variant="body1"
                          className={`dashboard-table__task-title ${
                            task.isComplete ? 'completed' : ''
                          }`}
                        >
                          {task.title}
                        </Typography>
                        <Typography
                          variant="body2"
                          color="text.secondary"
                          className={`dashboard-table__task-description ${
                            task.isComplete ? 'completed' : ''
                          }`}
                        >
                          {task.description}
                        </Typography>
                      </Box>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">{task.userDetails.fullName}</Typography>
                      <Typography variant="caption" color="text.secondary">
                        {task.userDetails.email}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={task.priority}
                        size="small"
                        color={getPriorityColor(task.priority)}
                        className="dashboard-table__priority-chip"
                      />
                    </TableCell>
                    <TableCell>
                      <Typography
                        variant="body2"
                        className={`dashboard-table__due-date ${isOverdue ? 'overdue' : ''}`}
                      >
                        {formatIsraelDateTime(task.dueDate).split(' • ')[0]}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {formatIsraelDateTime(task.dueDate).split(' • ')[1]}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Stack direction="row" spacing={0.5} flexWrap="wrap" useFlexGap>
                        {task.tags.slice(0, 2).map((tag) => (
                          <Chip
                            key={tag.id}
                            label={tag.name}
                            size="small"
                            variant="outlined"
                            className="dashboard-table__tag-chip"
                          />
                        ))}
                        {task.tags.length > 2 && (
                          <Tooltip
                            title={task.tags
                              .slice(2)
                              .map((t) => t.name)
                              .join(', ')}
                          >
                            <Chip
                              label={`+${task.tags.length - 2}`}
                              size="small"
                              variant="outlined"
                              className="dashboard-table__tag-chip"
                            />
                          </Tooltip>
                        )}
                      </Stack>
                    </TableCell>
                    <TableCell align="right">
                      <Box className="dashboard-table__actions">
                        <Tooltip
                          title={task.isComplete ? 'Cannot edit completed task' : 'Edit task'}
                        >
                          <span>
                            <IconButton
                              size="small"
                              color="primary"
                              disabled={task.isComplete}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </span>
                        </Tooltip>
                        <Tooltip title="Delete task">
                          <IconButton size="small" color="error">
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </TableCell>
                  </TableRow>
                );
              })
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {filteredAndSortedTasks.length > 0 && (
        <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <Typography variant="body2" color="text.secondary">
            Showing {filteredAndSortedTasks.length} of {tasks.length} tasks
          </Typography>
        </Box>
      )}
    </Box>
  );
};
