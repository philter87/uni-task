import { type ColumnDef } from '@tanstack/react-table'
import { ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react'
import { Badge } from '@/components/ui/badge'
import type { TaskItemDto } from '@/api/types'
import { cn } from '@/lib/utils'

function SortableHeader({ column, label }: { column: { getIsSorted: () => false | 'asc' | 'desc'; toggleSorting: (d: boolean) => void }; label: string }) {
  const sorted = column.getIsSorted()
  return (
    <button
      className="flex items-center gap-1 hover:text-foreground transition-colors"
      onClick={() => column.toggleSorting(sorted === 'asc')}
    >
      {label}
      {sorted === 'asc' ? <ArrowUp className="h-3 w-3" /> : sorted === 'desc' ? <ArrowDown className="h-3 w-3" /> : <ArrowUpDown className="h-3 w-3 opacity-40" />}
    </button>
  )
}

const priorityConfig: Record<number, { label: string; color: string }> = {
  0: { label: 'No', color: 'text-muted-foreground' },
  1: { label: 'Low', color: 'text-blue-400' },
  2: { label: 'Low', color: 'text-blue-400' },
  3: { label: 'Med', color: 'text-yellow-400' },
  4: { label: 'Med', color: 'text-yellow-400' },
  5: { label: 'Med', color: 'text-yellow-400' },
  6: { label: 'High', color: 'text-orange-400' },
  7: { label: 'High', color: 'text-orange-400' },
  8: { label: 'Crit', color: 'text-red-400' },
  9: { label: 'Crit', color: 'text-red-400' },
  10: { label: 'Crit', color: 'text-red-500' },
}

const providerBadge: Record<string, string> = {
  GitHub: 'bg-neutral-800 text-neutral-300',
  AzureDevOps: 'bg-blue-950 text-blue-300',
  Internal: 'bg-indigo-950 text-indigo-300',
}

export const taskColumns: ColumnDef<TaskItemDto>[] = [
  {
    accessorKey: 'title',
    header: ({ column }) => <SortableHeader column={column} label="Name" />,
    cell: ({ row }) => (
      <div className="flex items-center gap-2 min-w-0">
        <span className="font-medium truncate max-w-xs" title={row.original.title}>
          {row.original.title}
        </span>
        {row.original.provider && row.original.provider !== 'Internal' && (
          <span className={cn('text-[10px] px-1.5 py-0.5 rounded font-medium shrink-0', providerBadge[row.original.provider] ?? 'bg-muted text-muted-foreground')}>
            {row.original.provider}
          </span>
        )}
      </div>
    ),
    enableSorting: true,
  },
  {
    accessorKey: 'project',
    header: 'Project',
    cell: ({ row }) => (
      <span className="text-sm text-muted-foreground truncate max-w-[120px] block" title={row.original.project?.name}>
        {row.original.project?.name ?? '—'}
      </span>
    ),
    enableSorting: false,
  },
  {
    accessorKey: 'status',
    header: ({ column }) => <SortableHeader column={column} label="Status" />,
    cell: ({ row }) => {
      const status = row.original.status
      if (!status) return <span className="text-muted-foreground text-xs">—</span>
      return (
        <span className="inline-flex items-center gap-1.5 text-xs">
          <span className="w-1.5 h-1.5 rounded-full bg-primary shrink-0" />
          {status.name}
        </span>
      )
    },
    enableSorting: true,
  },
  {
    accessorKey: 'taskType',
    header: 'Type',
    cell: ({ row }) => (
      <span className="text-xs text-muted-foreground">{row.original.taskType?.name ?? '—'}</span>
    ),
    enableSorting: false,
  },
  {
    accessorKey: 'board',
    header: 'Board/Sprint',
    cell: ({ row }) => (
      <span className="text-xs text-muted-foreground truncate max-w-[100px] block">{row.original.board?.name ?? '—'}</span>
    ),
    enableSorting: false,
  },
  {
    accessorKey: 'assignedTo',
    header: ({ column }) => <SortableHeader column={column} label="Assigned" />,
    cell: ({ row }) => (
      <span className="text-xs text-muted-foreground">{row.original.assignedTo ?? '—'}</span>
    ),
    enableSorting: true,
  },
  {
    accessorKey: 'tags',
    header: 'Tags',
    cell: ({ row }) => {
      const tags = row.original.tags
      if (!tags?.length) return <span className="text-muted-foreground text-xs">—</span>
      return (
        <div className="flex flex-wrap gap-1">
          {tags.slice(0, 3).map((tag) => (
            <Badge key={tag.id} variant="secondary" className="text-[10px] px-1.5 py-0 h-4">
              {tag.name}
            </Badge>
          ))}
          {tags.length > 3 && <span className="text-[10px] text-muted-foreground">+{tags.length - 3}</span>}
        </div>
      )
    },
    enableSorting: false,
  },
  {
    accessorKey: 'priority',
    header: ({ column }) => <SortableHeader column={column} label="Priority" />,
    cell: ({ row }) => {
      const p = Math.round(row.original.priority)
      const cfg = priorityConfig[Math.min(p, 10)] ?? priorityConfig[5]
      return <span className={cn('text-xs font-medium', cfg.color)}>{cfg.label}</span>
    },
    enableSorting: true,
  },
]
