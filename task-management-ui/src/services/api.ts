import axios from "axios";
import type {
  Task,
  Tag,
  CreateTaskDto,
  UpdateTaskDto,
  CreateTagDto,
} from "../types";

const API_BASE_URL =
  import.meta.env.VITE_API_URL || "https://localhost:7071/api";

const priorityMap: Record<string, number> = {
  Low: 1,
  Medium: 3,
  High: 5,
};

const priorityReverseMap: Record<number, 'Low' | 'Medium' | 'High'> = {
  1: 'Low',
  2: 'Low',
  3: 'Medium',
  4: 'Medium',
  5: 'High',
};

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

export const taskService = {
  getTasks: async (): Promise<Task[]> => {
    const response = await api.get<any[]>("/tasks");
    return response.data.map((task) => ({
      ...task,
      priority: priorityReverseMap[task.priority] || 'Medium',
    }));
  },

  getTask: async (id: number): Promise<Task> => {
    const response = await api.get<any>(`/tasks/${id}`);
    return {
      ...response.data,
      priority: priorityReverseMap[response.data.priority] || 'Medium',
    };
  },

  getOverdueTasks: async (): Promise<Task[]> => {
    const response = await api.get<any[]>("/tasks/overdue");
    return response.data.map((task) => ({
      ...task,
      priority: priorityReverseMap[task.priority] || 'Medium',
    }));
  },

  markTaskComplete: async (id: number): Promise<Task> => {
    const response = await api.patch<any>(`/tasks/${id}/complete`);
    return {
      ...response.data,
      priority: priorityReverseMap[response.data.priority] || 'Medium',
    };
  },

  createTask: async (task: CreateTaskDto): Promise<Task> => {
    // Convert local datetime to ISO string with timezone offset
    // The datetime-local gives us YYYY-MM-DDTHH:mm format
    const localDate = new Date(task.dueDate);
    const dueDate = new Date(localDate.getTime() - (localDate.getTimezoneOffset() * 60000)).toISOString();
    
    const taskPayload = {
      ...task,
      dueDate,
      priority: priorityMap[task.priority] || 3,
    };
    
    const response = await api.post<any>("/tasks", taskPayload);
    return {
      ...response.data,
      priority: priorityReverseMap[response.data.priority] || 'Medium',
    };
  },

  updateTask: async (id: number, task: UpdateTaskDto): Promise<Task> => {
    // Convert local datetime to ISO string with timezone offset
    // The datetime-local gives us YYYY-MM-DDTHH:mm format
    const localDate = new Date(task.dueDate);
    const dueDate = new Date(localDate.getTime() - (localDate.getTimezoneOffset() * 60000)).toISOString();
    
    const taskPayload = {
      ...task,
      dueDate,
      priority: priorityMap[task.priority] || 3,
    };
    
    const response = await api.put<any>(`/tasks/${id}`, taskPayload);
    return {
      ...response.data,
      priority: priorityReverseMap[response.data.priority] || 'Medium',
    };
  },

  deleteTask: async (id: number): Promise<void> => {
    await api.delete(`/tasks/${id}`);
  },
};

export const tagService = {
  getTags: async (): Promise<Tag[]> => {
    const response = await api.get<Tag[]>("/tags");
    return response.data;
  },

  getTag: async (id: number): Promise<Tag> => {
    const response = await api.get<Tag>(`/tags/${id}`);
    return response.data;
  },

  createTag: async (tag: CreateTagDto): Promise<Tag> => {
    const response = await api.post<Tag>("/tags", tag);
    return response.data;
  },

  updateTag: async (id: number, tag: CreateTagDto): Promise<Tag> => {
    const response = await api.put<Tag>(`/tags/${id}`, tag);
    return response.data;
  },

  deleteTag: async (id: number): Promise<void> => {
    await api.delete(`/tags/${id}`);
  },
};

export default api;
