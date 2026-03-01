import { useState } from 'react'
import { useQuery, keepPreviousData } from '@tanstack/react-query'
import type { OnChangeFn, SortingState, PaginationState } from '@tanstack/react-table'
import { tasksApi } from '@/api/client'
import { queryKeys } from '@/lib/query-keys'
import { TasksTable } from '@/components/tasks/TasksTable'
import { TasksFilters } from '@/components/tasks/TasksFilters'

export default function TasksPage() {
  const [sorting, setSorting] = useState<SortingState>([])
  const [pagination, setPagination] = useState<PaginationState>({ pageIndex: 0, pageSize: 50 })
  const [search, setSearch] = useState('')
  const [projectId, setProjectId] = useState<string | undefined>()

  const params = {
    page: pagination.pageIndex + 1,
    pageSize: pagination.pageSize,
    ...(search ? { search } : {}),
    ...(projectId ? { projectId } : {}),
    ...(sorting[0] ? { sortBy: sorting[0].id, sortDescending: sorting[0].desc } : {}),
  }

  const { data, isLoading, isFetching } = useQuery({
    queryKey: queryKeys.tasks.list(params),
    queryFn: () => tasksApi.list(params),
    placeholderData: keepPreviousData,
  })

  const handleSortingChange: OnChangeFn<SortingState> = (updater) => {
    setSorting(updater)
    setPagination((p) => ({ ...p, pageIndex: 0 }))
  }

  const handleSearchChange = (v: string) => {
    setSearch(v)
    setPagination((p) => ({ ...p, pageIndex: 0 }))
  }

  const handleProjectChange = (v: string | undefined) => {
    setProjectId(v)
    setPagination((p) => ({ ...p, pageIndex: 0 }))
  }

  const handleClear = () => {
    setSearch('')
    setProjectId(undefined)
    setSorting([])
    setPagination({ pageIndex: 0, pageSize: 50 })
  }

  return (
    <div className="flex flex-col h-full min-h-0">
      <div className="flex items-center justify-between px-6 py-4 border-b border-border shrink-0">
        <h1 className="text-lg font-semibold">Tasks</h1>
        {isFetching && !isLoading && (
          <div className="w-4 h-4 border-2 border-primary border-t-transparent rounded-full animate-spin" />
        )}
      </div>

      <div className="px-6 py-3 border-b border-border shrink-0">
        <TasksFilters
          search={search}
          onSearchChange={handleSearchChange}
          projectId={projectId}
          onProjectChange={handleProjectChange}
          hasFilters={!!(search || projectId)}
          onClear={handleClear}
        />
      </div>

      <div className="flex-1 overflow-auto px-6 py-4 min-h-0">
        <TasksTable
          data={data}
          isLoading={isLoading}
          sorting={sorting}
          onSortingChange={handleSortingChange}
          pagination={pagination}
          onPaginationChange={setPagination}
        />
      </div>
    </div>
  )
}
