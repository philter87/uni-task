import { Search, X } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { projectsApi } from '@/api/client'
import { queryKeys } from '@/lib/query-keys'

interface TasksFiltersProps {
  search: string
  onSearchChange: (v: string) => void
  projectId: string | undefined
  onProjectChange: (v: string | undefined) => void
  hasFilters: boolean
  onClear: () => void
}

export function TasksFilters({ search, onSearchChange, projectId, onProjectChange, hasFilters, onClear }: TasksFiltersProps) {
  const { data: projects } = useQuery({
    queryKey: queryKeys.projects.list(),
    queryFn: () => projectsApi.list(),
    staleTime: 60_000,
  })

  return (
    <div className="flex items-center gap-2 flex-wrap">
      <div className="relative">
        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground pointer-events-none" />
        <Input
          className="pl-8 h-9 w-56 text-sm"
          placeholder="Search tasks..."
          value={search}
          onChange={(e) => onSearchChange(e.target.value)}
        />
      </div>

      <Select value={projectId ?? ''} onValueChange={(v) => onProjectChange(v || undefined)}>
        <SelectTrigger className="h-9 w-44 text-sm">
          <SelectValue placeholder="All projects" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="">All projects</SelectItem>
          {projects?.map((p) => (
            <SelectItem key={p.id} value={p.id}>
              {p.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      {hasFilters && (
        <Button variant="ghost" size="sm" onClick={onClear} className="h-9 text-muted-foreground hover:text-foreground">
          <X className="h-3.5 w-3.5 mr-1" />
          Clear
        </Button>
      )}
    </div>
  )
}
