const API_BASE_URL = '/api';

export interface TaskItem {
  id: number;
  title: string;
  description?: string | null;
  status: number;
  priority: number;
  createdAt: string;
  dueDate?: string | null;
  assignedTo?: string | null;
  source?: string | null;
  externalId?: string | null;
}

export const TaskStatus = {
  Todo: 0,
  InProgress: 1,
  Done: 2
} as const;

export const TaskPriority = {
  Low: 0,
  Medium: 1,
  High: 2,
  Critical: 3
} as const;

export const TaskStatusNames = ['Todo', 'InProgress', 'Done'];
export const TaskPriorityNames = ['Low', 'Medium', 'High', 'Critical'];

class TasksApi {
  async getTasks(): Promise<TaskItem[]> {
    const response = await fetch(`${API_BASE_URL}/tasks`);
    if (!response.ok) {
      throw new Error('Failed to fetch tasks');
    }
    return response.json();
  }

  async getTask(id: number): Promise<TaskItem> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`);
    if (!response.ok) {
      throw new Error('Failed to fetch task');
    }
    return response.json();
  }

  async createTask(task: Partial<TaskItem>): Promise<TaskItem> {
    const response = await fetch(`${API_BASE_URL}/tasks`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(task),
    });
    if (!response.ok) {
      throw new Error('Failed to create task');
    }
    return response.json();
  }

  async updateTask(id: number, task: TaskItem): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(task),
    });
    if (!response.ok) {
      throw new Error('Failed to update task');
    }
  }

  async deleteTask(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/tasks/${id}`, {
      method: 'DELETE',
    });
    if (!response.ok) {
      throw new Error('Failed to delete task');
    }
  }
}

export const tasksApi = new TasksApi();
