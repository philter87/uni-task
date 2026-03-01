import { createContext, useContext, type ReactNode } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { authApi } from '@/api/client'
import { queryKeys } from '@/lib/query-keys'
import type { UserDto } from '@/api/types'

interface AuthContextValue {
  user: UserDto | null
  isLoading: boolean
  logout: () => void
}

const AuthContext = createContext<AuthContextValue>({
  user: null,
  isLoading: true,
  logout: () => {},
})

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const token = localStorage.getItem('auth_token')

  const { data: user, isLoading } = useQuery({
    queryKey: queryKeys.auth.me,
    queryFn: authApi.me,
    enabled: !!token,
    retry: false,
    staleTime: 5 * 60 * 1000,
  })

  const logout = () => {
    localStorage.removeItem('auth_token')
    queryClient.clear()
    window.location.href = '/login'
  }

  return (
    <AuthContext.Provider value={{ user: user ?? null, isLoading: !!token && isLoading, logout }}>
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = () => useContext(AuthContext)
