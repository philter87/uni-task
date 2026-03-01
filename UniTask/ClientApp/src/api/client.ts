import type {
  CreateProjectBody,
  CreateTaskProviderAuthBody,
  GetTasksParams,
  OrganisationDto,
  PagedResult,
  ProjectDto,
  TaskItemDto,
  UserDto,
} from '@/api/types'

const getToken = () => localStorage.getItem('auth_token')

async function apiFetch<T>(url: string, init?: RequestInit): Promise<T> {
  const token = getToken()
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...(init?.headers ?? {}),
  }
  const res = await fetch(url, { ...init, headers })
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText)
    throw new Error(`${res.status}: ${text}`)
  }
  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

function buildQuery(params: Record<string, unknown>): string {
  const qs = new URLSearchParams()
  for (const [key, value] of Object.entries(params)) {
    if (value === undefined || value === null) continue
    if (Array.isArray(value)) {
      value.forEach((v) => qs.append(key, String(v)))
    } else {
      qs.append(key, String(value))
    }
  }
  const str = qs.toString()
  return str ? `?${str}` : ''
}

export const authApi = {
  me: () => apiFetch<UserDto>('/api/auth/me'),
}

export const organisationsApi = {
  my: () => apiFetch<OrganisationDto[]>('/api/organisations/my'),
  projects: (orgId: string) => apiFetch<ProjectDto[]>(`/api/organisations/${orgId}/projects`),
}

export const projectsApi = {
  list: (organisationId?: string) =>
    apiFetch<ProjectDto[]>(`/api/projects${buildQuery({ organisationId })}`),
  get: (id: string) => apiFetch<ProjectDto>(`/api/projects/${id}`),
  create: (body: CreateProjectBody) =>
    apiFetch<string>('/api/projects', { method: 'POST', body: JSON.stringify(body) }),
}

export const tasksApi = {
  list: (params: GetTasksParams) =>
    apiFetch<PagedResult<TaskItemDto>>(`/api/tasks${buildQuery(params as Record<string, unknown>)}`),
  get: (id: string) => apiFetch<TaskItemDto>(`/api/tasks/${id}`),
  sync: (projectId: string) =>
    apiFetch<TaskItemDto[]>('/api/tasks/sync', { method: 'POST', body: JSON.stringify({ projectId }) }),
}

export const taskProviderAuthApi = {
  create: (body: CreateTaskProviderAuthBody) =>
    apiFetch<string>('/api/task-provider-auths', { method: 'POST', body: JSON.stringify(body) }),
}
