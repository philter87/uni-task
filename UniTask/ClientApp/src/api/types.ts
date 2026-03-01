// Types matching the backend API models

export type TaskProvider = 'Internal' | 'GitHub' | 'AzureDevOps' | 'Jira'
export type AuthenticationType = 'GitHubApp' | 'AzureAppRegistration' | 'JiraApiToken'

export interface UserDto {
  id: string
  email?: string
  displayName?: string
  avatarUrl?: string
  personalOrganisationId?: string
}

export interface OrganisationDto {
  id: string
  name: string
  isPersonal: boolean
  provider?: TaskProvider
  role?: string
}

export interface ProjectDto {
  id: string
  externalId?: string
  name: string
  description?: string
  organisationId?: string
  provider?: TaskProvider
  taskProviderAuthId?: string
  createdAt: string
  updatedAt: string
}

export interface StatusDto {
  id: string
  name: string
  description?: string
  order?: number
}

export interface TaskTypeDto {
  id: string
  name: string
  description?: string
}

export interface BoardDto {
  id: string
  name: string
  startDate?: string
  endDate?: string
}

export interface TagDto {
  id: string
  name: string
}

export interface LabelDto {
  id: string
  name: string
  color?: string
}

export interface CommentDto {
  id: string
  content: string
  createdAt: string
}

export interface AttachmentDto {
  id: string
  name: string
  url: string
  fileType: string
}

export interface TaskItemDto {
  id: string
  externalId?: string
  title: string
  description?: string
  projectId?: string
  taskTypeId?: string
  statusId?: string
  boardId?: string
  parentId?: string
  priority: number
  createdAt: string
  updatedAt: string
  dueDate?: string
  assignedTo?: string
  assignedToUserId?: string
  provider?: TaskProvider
  durationHours?: number
  durationRemainingHours?: number
  project?: ProjectDto
  taskType?: TaskTypeDto
  status?: StatusDto
  board?: BoardDto
  comments: CommentDto[]
  labels: LabelDto[]
  tags: TagDto[]
  attachments: AttachmentDto[]
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
}

export interface GetTasksParams {
  projectId?: string
  organisationId?: string
  search?: string
  tagIds?: string[]
  statusIds?: string[]
  taskTypeIds?: string[]
  boardIds?: string[]
  assignedTo?: string
  sortBy?: string
  sortDescending?: boolean
  page?: number
  pageSize?: number
}

export interface CreateProjectBody {
  name: string
  description?: string
  organisationId?: string
  provider?: TaskProvider
  externalId?: string
  taskProviderAuthId?: string
  triggerSync?: boolean
}

export interface CreateTaskProviderAuthBody {
  organisationId: string
  authenticationType: AuthenticationType
  authTypeId: string
  secretValue: string
}
