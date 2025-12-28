import { useState } from "react";
import {
  Card,
  CardContent,
  CardActions,
  Typography,
  Chip,
  Box,
  IconButton,
  Checkbox,
  CircularProgress,
  Tooltip,
  Divider,
} from "@mui/material";
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Person as PersonIcon,
  CheckCircle as CheckCircleIcon,
  CalendarToday as CalendarIcon,
} from "@mui/icons-material";
import type { Task } from "../../types";
import { formatIsraelDateTime } from "../../utils/timezone";
import "./TaskCard.scss";

interface TaskCardProps {
  task: Task;
  onEdit: (task: Task) => void;
  onDelete: (id: number) => void;
  onMarkComplete: (id: number) => Promise<void>;
}

const getPriorityColor = (priority: string) => {
  switch (priority) {
    case "High":
      return "error";
    case "Medium":
      return "warning";
    case "Low":
      return "success";
    default:
      return "default";
  }
};

export const TaskCard = ({
  task,
  onEdit,
  onDelete,
  onMarkComplete,
}: TaskCardProps) => {
  const [completing, setCompleting] = useState(false);

  const handleMarkComplete = async () => {
    if (task.isComplete) return;

    setCompleting(true);
    try {
      await onMarkComplete(task.id);
    } finally {
      setCompleting(false);
    }
  };

  // Parse UTC date and compare with current local time
  const dueDate = new Date(task.dueDate + 'Z'); // Add 'Z' to parse as UTC
  const isOverdue = dueDate < new Date() && !task.isComplete;

  return (
    <Card
      elevation={0}
      className={`task-card ${task.isComplete ? "completed" : ""} ${
        isOverdue ? "overdue" : ""
      }`}
    >
      <CardContent className="task-card__content">
        {/* Status Badges */}
        <Box className="task-card__badges">
          {task.isComplete && (
            <Chip
              label="Completed"
              size="small"
              color="success"
              variant="filled"
              className="task-card__badge"
            />
          )}
          {isOverdue && (
            <Chip
              label="Overdue"
              size="small"
              color="error"
              variant="filled"
              className="task-card__badge"
            />
          )}
          <Chip
            label={task.priority}
            size="small"
            color={getPriorityColor(task.priority)}
            className="task-card__badge"
          />
        </Box>

        {/* Tags */}
        <Box className="task-card__tags">
          {task.tags.slice(0, 3).map((tag) => (
            <Chip
              key={tag.id}
              label={tag.name}
              size="small"
              variant="outlined"
              className="task-card__tag"
            />
          ))}
          {task.tags.length > 3 && (
            <Chip
              label={`+${task.tags.length - 3}`}
              size="small"
              variant="outlined"
              className="task-card__tag"
            />
          )}
        </Box>

        {/* Title with Checkbox */}
        <Box className="task-card__header">
          {completing ? (
            <CircularProgress size={24} sx={{ flexShrink: 0 }} />
          ) : (
            <Tooltip title={task.isComplete ? "Completed" : "Mark as complete"}>
              <Checkbox
                checked={task.isComplete}
                onChange={handleMarkComplete}
                disabled={task.isComplete}
                icon={<CheckCircleIcon />}
                checkedIcon={<CheckCircleIcon />}
                color="success"
                className="task-card__checkbox"
              />
            </Tooltip>
          )}
          <Box
            className={`task-card__title ${task.isComplete ? "completed" : ""}`}
          >
            <Tooltip title={task.title} placement="top" arrow>
              <Typography variant="h6" component="h2" noWrap={false}>
                {task.title}
              </Typography>
            </Tooltip>
          </Box>
        </Box>

        <Divider className="task-card__divider" />

        {/* Description */}
        <Box
          className={`task-card__description ${
            task.isComplete ? "completed" : ""
          }`}
        >
          <Tooltip title={task.description} placement="bottom" arrow>
            <Typography variant="body2" color="text.secondary">
              {task.description}
            </Typography>
          </Tooltip>
        </Box>

        {/* User Info */}
        <Box className="task-card__info-box">
          <PersonIcon fontSize="small" sx={{ flexShrink: 0 }} />
          <Box className="task-card__info-text">
            <Tooltip title={task.userDetails.fullName} arrow>
              <Typography variant="body2" noWrap>
                {task.userDetails.fullName}
              </Typography>
            </Tooltip>
          </Box>
        </Box>

        {/* Due Date */}
        <Box className={`task-card__info-box ${isOverdue ? "overdue" : ""}`}>
          <CalendarIcon fontSize="small" sx={{ flexShrink: 0 }} />
          <Box className={`task-card__date ${isOverdue ? "overdue" : ""}`}>
            <Typography variant="caption" noWrap>
              {formatIsraelDateTime(task.dueDate)}
            </Typography>
          </Box>
        </Box>
      </CardContent>

      <Divider />

      <CardActions className="task-card__actions">
        <Tooltip
          title={task.isComplete ? "Cannot edit completed task" : "Edit task"}
          arrow
        >
          <span>
            <IconButton
              size="small"
              color="primary"
              onClick={() => onEdit(task)}
              disabled={task.isComplete}
            >
              <EditIcon fontSize="small" />
            </IconButton>
          </span>
        </Tooltip>
        <Tooltip title="Delete task" arrow>
          <IconButton
            size="small"
            color="error"
            onClick={() => onDelete(task.id)}
          >
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </CardActions>
    </Card>
  );
};
