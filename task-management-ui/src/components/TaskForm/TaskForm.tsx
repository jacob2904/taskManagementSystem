import { useState, useEffect } from "react";
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  MenuItem,
  Box,
  Chip,
  FormControl,
  InputLabel,
  Select,
  OutlinedInput,
} from "@mui/material";
import type { SelectChangeEvent } from "@mui/material";
import type { Task, CreateTaskDto, Tag } from "../../types";
import { localToUtc, utcToIsraelLocal } from "../../utils/timezone";

interface TaskFormProps {
  open: boolean;
  task: Task | null;
  tags: Tag[];
  onClose: () => void;
  onSubmit: (task: CreateTaskDto) => Promise<void>;
}

export const TaskForm = ({
  open,
  task,
  tags,
  onClose,
  onSubmit,
}: TaskFormProps) => {
  const getCurrentLocalTime = () => {
    const now = new Date();
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  };

  const [formData, setFormData] = useState<CreateTaskDto>({
    title: "",
    description: "",
    dueDate: getCurrentLocalTime(),
    priority: "Medium",
    tagIds: [],
  });

  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (task) {
      // Convert UTC datetime from backend to local time for datetime-local input
      const dueDateValue = utcToIsraelLocal(task.dueDate);
      
      setFormData({
        title: task.title,
        description: task.description,
        dueDate: dueDateValue,
        priority: task.priority,
        tagIds: task.tags.map((tag) => tag.id),
      });
    } else {
      setFormData({
        title: "",
        description: "",
        dueDate: getCurrentLocalTime(),
        priority: "Medium",
        tagIds: [],
      });
    }
  }, [task, open]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Client-side validation
    if (formData.description.length < 10) {
      return; // Don't submit if description is too short
    }

    setLoading(true);
    try {
      // Convert local datetime to UTC before sending to backend
      const taskDataToSubmit = {
        ...formData,
        dueDate: localToUtc(formData.dueDate),
      };
      await onSubmit(taskDataToSubmit);
      onClose();
    } catch (error) {
      console.error("Error submitting task:", error);
    } finally {
      setLoading(false);
    }
  };

  const handleTagChange = (event: SelectChangeEvent<number[]>) => {
    const value = event.target.value;
    setFormData({
      ...formData,
      tagIds: typeof value === "string" ? [] : value,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>{task ? "Edit Task" : "Create New Task"}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: "flex", flexDirection: "column", gap: 2, mt: 1 }}>
            <TextField
              label="Title"
              value={formData.title}
              onChange={(e) =>
                setFormData({ ...formData, title: e.target.value })
              }
              required
              fullWidth
            />

            <TextField
              label="Description"
              value={formData.description}
              onChange={(e) =>
                setFormData({ ...formData, description: e.target.value })
              }
              required
              fullWidth
              multiline
              rows={3}
              error={
                formData.description.length > 0 &&
                formData.description.length < 10
              }
              helperText={
                formData.description.length > 0 &&
                formData.description.length < 10
                  ? `Description must be at least 10 characters (${formData.description.length}/10)`
                  : `${formData.description.length}/2000 characters (minimum 10)`
              }
            />

            <TextField
              label="Due Date & Time"
              type="datetime-local"
              value={formData.dueDate}
              onChange={(e) =>
                setFormData({ ...formData, dueDate: e.target.value })
              }
              required
              fullWidth
              InputLabelProps={{ shrink: true }}
            />

            <TextField
              label="Priority"
              select
              value={formData.priority}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  priority: e.target.value as "Low" | "Medium" | "High",
                })
              }
              required
              fullWidth
            >
              <MenuItem value="Low">Low</MenuItem>
              <MenuItem value="Medium">Medium</MenuItem>
              <MenuItem value="High">High</MenuItem>
            </TextField>

            <FormControl fullWidth>
              <InputLabel>Tags</InputLabel>
              <Select
                multiple
                value={formData.tagIds}
                onChange={handleTagChange}
                input={<OutlinedInput label="Tags" />}
                renderValue={(selected) => (
                  <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                    {selected.map((tagId) => {
                      const tag = tags.find((t) => t.id === tagId);
                      return tag ? (
                        <Chip key={tagId} label={tag.name} size="small" />
                      ) : null;
                    })}
                  </Box>
                )}
              >
                {tags.map((tag) => (
                  <MenuItem key={tag.id} value={tag.id}>
                    {tag.name}
                  </MenuItem>
                ))}
              </Select>
            </FormControl>
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={onClose} disabled={loading}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={loading}>
            {task ? "Update" : "Create"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
};
