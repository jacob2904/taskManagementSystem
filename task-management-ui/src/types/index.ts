export interface Tag {
  id: number;
  name: string;
}

export interface UserDetails {
  id: number;
  fullName: string;
  telephone: string;
  email: string;
}

export interface Task {
  id: number;
  title: string;
  description: string;
  dueDate: string;
  priority: 'Low' | 'Medium' | 'High';
  isComplete: boolean;
  userDetails: UserDetails;
  tags: Tag[];
  createdAt: string;
  updatedAt?: string;
}

export interface CreateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: 'Low' | 'Medium' | 'High';
  tagIds: number[];
}

export interface UpdateTaskDto {
  title: string;
  description: string;
  dueDate: string;
  priority: 'Low' | 'Medium' | 'High';
  isComplete: boolean;
  tagIds: number[];
}

export interface CreateTagDto {
  name: string;
}
