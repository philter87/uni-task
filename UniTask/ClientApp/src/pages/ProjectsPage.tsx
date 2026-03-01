import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { FolderKanban, Plus } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { projectsApi } from '@/api/client'
import { queryKeys } from '@/lib/query-keys'
import type { TaskProvider } from '@/api/types'
import { cn } from '@/lib/utils'
import { CreateProjectDialog } from '@/components/projects/CreateProjectDialog'

const providerLabel: Record<TaskProvider, string> = {
  GitHub: 'GitHub Issues',
  AzureDevOps: 'Azure DevOps',
  Internal: 'Internal',
  Jira: 'Jira',
}

const providerColor: Record<TaskProvider, string> = {
  GitHub: 'bg-neutral-800 text-neutral-300',
  AzureDevOps: 'bg-blue-950 text-blue-300',
  Internal: 'bg-indigo-950 text-indigo-300',
  Jira: 'bg-blue-950 text-blue-200',
}

export default function ProjectsPage() {
  const [dialogOpen, setDialogOpen] = useState(false)
  const { data: projects, isLoading } = useQuery({
    queryKey: queryKeys.projects.list(),
    queryFn: () => projectsApi.list(),
  })

  return (
    <div className="flex flex-col h-full min-h-0">
      <div className="flex items-center justify-between px-6 py-4 border-b border-border shrink-0">
        <h1 className="text-lg font-semibold">Projects</h1>
        <Button size="sm" className="h-8 gap-1.5 text-xs" onClick={() => setDialogOpen(true)}>
          <Plus className="h-3.5 w-3.5" />
          Add project
        </Button>
      </div>
      <CreateProjectDialog open={dialogOpen} onOpenChange={setDialogOpen} />

      <div className="flex-1 overflow-auto px-6 py-4">
        {isLoading ? (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <div key={i} className="h-24 rounded-lg border border-border bg-card animate-pulse" />
            ))}
          </div>
        ) : !projects?.length ? (
          <div className="flex flex-col items-center justify-center h-48 gap-3 text-muted-foreground">
            <FolderKanban className="h-10 w-10 opacity-30" />
            <p className="text-sm">No projects yet. Add one to get started.</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-3">
            {projects.map((p) => (
              <div
                key={p.id}
                className="rounded-lg border border-border bg-card p-4 flex flex-col gap-2 hover:border-border/80 transition-colors"
              >
                <div className="flex items-start justify-between gap-2">
                  <span className="font-medium text-sm truncate">{p.name}</span>
                  {p.provider && p.provider !== 'Internal' && (
                    <span className={cn('text-[10px] px-1.5 py-0.5 rounded font-medium shrink-0', providerColor[p.provider])}>
                      {providerLabel[p.provider]}
                    </span>
                  )}
                </div>
                {p.externalId && (
                  <span className="text-xs text-muted-foreground truncate">{p.externalId}</span>
                )}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}
