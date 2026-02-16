import { useState, useEffect } from 'react'
import './App.css'
import { tasksApi } from './api/tasks-api'
import type { TaskItem } from './api/tasks-api'
import { TaskStatus, TaskPriority, TaskStatusNames, TaskPriorityNames } from './api/tasks-api'

function App() {
  const [tasks, setTasks] = useState<TaskItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [newTaskTitle, setNewTaskTitle] = useState('')
  const [newTaskDescription, setNewTaskDescription] = useState('')

  useEffect(() => {
    loadTasks()
  }, [])

  const loadTasks = async () => {
    try {
      setLoading(true)
      setError(null)
      const data = await tasksApi.getTasks()
      setTasks(data)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load tasks')
    } finally {
      setLoading(false)
    }
  }

  const handleCreateTask = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!newTaskTitle.trim()) return

    try {
      const newTask = {
        title: newTaskTitle,
        description: newTaskDescription || undefined,
        status: TaskStatus.Todo,
        priority: TaskPriority.Medium
      }
      await tasksApi.createTask(newTask)
      setNewTaskTitle('')
      setNewTaskDescription('')
      await loadTasks()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create task')
    }
  }

  const handleStatusChange = async (task: TaskItem, newStatus: number) => {
    try {
      const updatedTask = { ...task, status: newStatus }
      await tasksApi.updateTask(task.id, updatedTask)
      await loadTasks()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to update task')
    }
  }

  const handleDelete = async (id: number) => {
    try {
      await tasksApi.deleteTask(id)
      await loadTasks()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete task')
    }
  }

  const getStatusColor = (status: number) => {
    switch (status) {
      case TaskStatus.Todo:
        return '#ffa500'
      case TaskStatus.InProgress:
        return '#1e90ff'
      case TaskStatus.Done:
        return '#32cd32'
      default:
        return '#ccc'
    }
  }

  const getPriorityColor = (priority: number) => {
    switch (priority) {
      case TaskPriority.Critical:
        return '#ff0000'
      case TaskPriority.High:
        return '#ff8c00'
      case TaskPriority.Medium:
        return '#ffd700'
      case TaskPriority.Low:
        return '#90ee90'
      default:
        return '#ccc'
    }
  }

  return (
    <div style={{ padding: '20px', maxWidth: '1200px', margin: '0 auto' }}>
      <h1>UniTask - Unified Task Manager</h1>
      <p style={{ color: '#888', marginBottom: '30px' }}>
        A unified task manager for Azure DevOps boards and GitHub issues
      </p>

      {error && (
        <div style={{ padding: '10px', marginBottom: '20px', backgroundColor: '#ffebee', color: '#c62828', borderRadius: '4px' }}>
          {error}
        </div>
      )}

      <form onSubmit={handleCreateTask} style={{ marginBottom: '30px' }}>
        <h2>Create New Task</h2>
        <div style={{ display: 'flex', gap: '10px', marginBottom: '10px' }}>
          <input
            type="text"
            placeholder="Task title"
            value={newTaskTitle}
            onChange={(e) => setNewTaskTitle(e.target.value)}
            style={{ flex: 1, padding: '8px', fontSize: '16px' }}
          />
          <button type="submit" style={{ padding: '8px 20px', fontSize: '16px' }}>
            Add Task
          </button>
        </div>
        <textarea
          placeholder="Description (optional)"
          value={newTaskDescription}
          onChange={(e) => setNewTaskDescription(e.target.value)}
          style={{ width: '100%', padding: '8px', fontSize: '14px', minHeight: '60px' }}
        />
      </form>

      <h2>Tasks</h2>
      {loading ? (
        <p>Loading tasks...</p>
      ) : tasks.length === 0 ? (
        <p style={{ color: '#888' }}>No tasks yet. Create one above!</p>
      ) : (
        <div style={{ display: 'grid', gap: '15px' }}>
          {tasks.map((task) => (
            <div
              key={task.id}
              style={{
                border: '1px solid #ddd',
                borderRadius: '8px',
                padding: '15px',
                backgroundColor: '#f9f9f9'
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', marginBottom: '10px' }}>
                <div style={{ flex: 1 }}>
                  <h3 style={{ margin: '0 0 8px 0' }}>{task.title}</h3>
                  {task.description && (
                    <p style={{ margin: '0 0 8px 0', color: '#666' }}>{task.description}</p>
                  )}
                  <div style={{ display: 'flex', gap: '10px', fontSize: '12px', color: '#888' }}>
                    <span>
                      Priority:{' '}
                      <span style={{ color: getPriorityColor(task.priority), fontWeight: 'bold' }}>
                        {TaskPriorityNames[task.priority]}
                      </span>
                    </span>
                    <span>Created: {new Date(task.createdAt).toLocaleDateString()}</span>
                    {task.source && <span>Source: {task.source}</span>}
                  </div>
                </div>
                <button
                  onClick={() => handleDelete(task.id)}
                  style={{ padding: '4px 8px', fontSize: '12px', backgroundColor: '#ff4444', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
                >
                  Delete
                </button>
              </div>
              <div style={{ display: 'flex', gap: '10px' }}>
                {Object.values(TaskStatus).filter(v => typeof v === 'number').map((status) => (
                  <button
                    key={status}
                    onClick={() => handleStatusChange(task, status as number)}
                    style={{
                      padding: '6px 12px',
                      fontSize: '14px',
                      backgroundColor: task.status === status ? getStatusColor(status as number) : '#eee',
                      color: task.status === status ? 'white' : '#333',
                      border: 'none',
                      borderRadius: '4px',
                      cursor: 'pointer',
                      fontWeight: task.status === status ? 'bold' : 'normal'
                    }}
                  >
                    {TaskStatusNames[status as number]}
                  </button>
                ))}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  )
}

export default App

