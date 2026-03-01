export const queryKeys = {
  auth: {
    me: ['auth', 'me'] as const,
  },
  tasks: {
    all: ['tasks'] as const,
    list: (filters: Record<string, unknown>) => ['tasks', 'list', filters] as const,
    detail: (id: string) => ['tasks', 'detail', id] as const,
  },
  projects: {
    all: ['projects'] as const,
    list: (orgId?: string) => ['projects', 'list', orgId] as const,
  },
  organisations: {
    my: ['organisations', 'my'] as const,
  },
}
