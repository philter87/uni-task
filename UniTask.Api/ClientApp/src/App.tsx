import { useState, useEffect } from 'react'
import './App.css'
import { tasksApi } from './api/tasks-api'
import type { TaskItem } from './api/tasks-api'
import { TaskStatus, TaskPriority, TaskStatusNames, TaskPriorityNames } from './api/tasks-api'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Trash2 } from 'lucide-react'

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

  const getPriorityVariant = (priority: number) => {
    switch (priority) {
      case TaskPriority.Critical:
      case TaskPriority.High:
        return 'destructive'
      case TaskPriority.Medium:
        return 'default'
      case TaskPriority.Low:
        return 'secondary'
      default:
        return 'outline'
    }
  }

  return (
    <div className="container mx-auto p-6 max-w-6xl">
      <div className="mb-8">
        <h1 className="text-4xl font-bold mb-2">UniTask - Unified Task Manager</h1>
        <p className="text-muted-foreground">
          A unified task manager for Azure DevOps boards and GitHub issues
        </p>
      </div>

      {error && (
        <div className="bg-destructive/15 text-destructive px-4 py-3 rounded-lg mb-6">
          {error}
        </div>
      )}

      <Card className="mb-8">
        <CardHeader>
          <CardTitle>Create New Task</CardTitle>
          <CardDescription>Add a new task to your unified task manager</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleCreateTask} className="space-y-4">
            <div className="flex gap-3">
              <Input
                type="text"
                placeholder="Task title"
                value={newTaskTitle}
                onChange={(e) => setNewTaskTitle(e.target.value)}
                className="flex-1"
              />
              <Button type="submit">Add Task</Button>
            </div>
            <Textarea
              placeholder="Description (optional)"
              value={newTaskDescription}
              onChange={(e) => setNewTaskDescription(e.target.value)}
              className="min-h-[80px]"
            />
          </form>
        </CardContent>
      </Card>

      <div className="mb-4">
        <h2 className="text-2xl font-semibold">Tasks</h2>
      </div>
      
      {loading ? (
        <p className="text-muted-foreground">Loading tasks...</p>
      ) : tasks.length === 0 ? (
        <p className="text-muted-foreground">No tasks yet. Create one above!</p>
      ) : (
        <div className="grid gap-4">
          {tasks.map((task) => (
            <Card key={task.id}>
              <CardHeader>
                <div className="flex justify-between items-start">
                  <div className="flex-1">
                    <CardTitle className="text-xl">{task.title}</CardTitle>
                    {task.description && (
                      <CardDescription className="mt-2">{task.description}</CardDescription>
                    )}
                  </div>
                  <Button
                    variant="destructive"
                    size="icon"
                    onClick={() => handleDelete(task.id)}
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                <div className="flex flex-wrap gap-2 mb-4">
                  <Badge variant={getPriorityVariant(task.priority)}>
                    {TaskPriorityNames[task.priority]}
                  </Badge>
                  <Badge variant="outline">
                    {new Date(task.createdAt).toLocaleDateString()}
                  </Badge>
                  {task.source && (
                    <Badge variant="outline">
                      {task.source}
                    </Badge>
                  )}
                </div>
              </CardContent>
              <CardFooter>
                <Select
                  value={task.status.toString()}
                  onValueChange={(value) => handleStatusChange(task, parseInt(value))}
                >
                  <SelectTrigger className="w-[200px]">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {Object.values(TaskStatus).filter(v => typeof v === 'number').map((status) => (
                      <SelectItem key={status} value={status.toString()}>
                        {TaskStatusNames[status as number]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </CardFooter>
            </Card>
          ))}
        </div>
      )}
    </div>
  )
}

export default App

