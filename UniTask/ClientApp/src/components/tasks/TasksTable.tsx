import {
  useReactTable,
  getCoreRowModel,
  flexRender,
  type OnChangeFn,
  type SortingState,
  type PaginationState,
} from '@tanstack/react-table'
import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { taskColumns } from './columns'
import type { PagedResult, TaskItemDto } from '@/api/types'

interface TasksTableProps {
  data?: PagedResult<TaskItemDto>
  isLoading: boolean
  sorting: SortingState
  onSortingChange: OnChangeFn<SortingState>
  pagination: PaginationState
  onPaginationChange: OnChangeFn<PaginationState>
}

export function TasksTable({ data, isLoading, sorting, onSortingChange, pagination, onPaginationChange }: TasksTableProps) {
  const table = useReactTable({
    data: data?.items ?? [],
    columns: taskColumns,
    getCoreRowModel: getCoreRowModel(),
    manualSorting: true,
    manualPagination: true,
    manualFiltering: true,
    pageCount: data?.totalPages ?? -1,
    state: { sorting, pagination },
    onSortingChange,
    onPaginationChange,
  })

  return (
    <div className="flex flex-col gap-3">
      <div className="rounded-lg border border-border overflow-hidden">
        <Table>
          <TableHeader>
            {table.getHeaderGroups().map((hg) => (
              <TableRow key={hg.id} className="hover:bg-transparent border-border bg-muted/20">
                {hg.headers.map((header) => (
                  <TableHead key={header.id} className="text-xs text-muted-foreground font-medium h-9 px-3 whitespace-nowrap">
                    {header.isPlaceholder ? null : flexRender(header.column.columnDef.header, header.getContext())}
                  </TableHead>
                ))}
              </TableRow>
            ))}
          </TableHeader>
          <TableBody>
            {isLoading ? (
              Array.from({ length: 8 }).map((_, i) => (
                <TableRow key={i} className="border-border">
                  {taskColumns.map((_, j) => (
                    <TableCell key={j} className="px-3 py-2.5">
                      <Skeleton className="h-4 w-full max-w-[120px]" />
                    </TableCell>
                  ))}
                </TableRow>
              ))
            ) : table.getRowModel().rows.length === 0 ? (
              <TableRow>
                <TableCell colSpan={taskColumns.length} className="h-32 text-center text-muted-foreground text-sm">
                  No tasks found.
                </TableCell>
              </TableRow>
            ) : (
              table.getRowModel().rows.map((row) => (
                <TableRow key={row.id} className="border-border hover:bg-muted/30 cursor-default">
                  {row.getVisibleCells().map((cell) => (
                    <TableCell key={cell.id} className="px-3 py-2">
                      {flexRender(cell.column.columnDef.cell, cell.getContext())}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      <div className="flex items-center justify-between text-sm text-muted-foreground px-1">
        <span className="text-xs">
          {data ? `${data.totalCount.toLocaleString()} task${data.totalCount !== 1 ? 's' : ''}` : ''}
        </span>
        <div className="flex items-center gap-2">
          <span className="text-xs">
            Page {data?.page ?? 1} of {data?.totalPages ?? 1}
          </span>
          <Button
            variant="outline"
            size="icon"
            className="h-7 w-7"
            onClick={() => table.previousPage()}
            disabled={!table.getCanPreviousPage()}
          >
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <Button
            variant="outline"
            size="icon"
            className="h-7 w-7"
            onClick={() => table.nextPage()}
            disabled={!table.getCanNextPage()}
          >
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </div>
  )
}
