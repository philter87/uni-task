import { Github, Chrome } from 'lucide-react'
import { useSearchParams } from 'react-router-dom'
import { Button } from '@/components/ui/button'

export default function LoginPage() {
  const [searchParams] = useSearchParams()
  const error = searchParams.get('error')

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="w-full max-w-sm space-y-8 px-4">
        {/* Logo */}
        <div className="text-center space-y-2">
          <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-primary/10 border border-primary/20">
            <svg viewBox="0 0 24 24" fill="none" className="w-6 h-6 text-primary" stroke="currentColor" strokeWidth={2}>
              <path d="M9 3H5a2 2 0 00-2 2v4m6-6h10a2 2 0 012 2v4M9 3v18m0 0h10a2 2 0 002-2V9M9 21H5a2 2 0 01-2-2V9m0 0h18" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </div>
          <h1 className="text-2xl font-bold tracking-tight">UniTask</h1>
          <p className="text-sm text-muted-foreground">Unified task management across all your tools</p>
        </div>

        {/* Error message */}
        {error && (
          <div className="rounded-md bg-destructive/10 border border-destructive/20 px-4 py-3 text-sm text-destructive">
            {decodeURIComponent(error)}
          </div>
        )}

        {/* Auth buttons */}
        <div className="space-y-3">
          <a href="/api/auth/login/github" className="block">
            <Button variant="outline" className="w-full gap-3 h-11 border-border hover:bg-accent">
              <Github className="w-4 h-4" />
              Continue with GitHub
            </Button>
          </a>
          <a href="/api/auth/login/google" className="block">
            <Button variant="outline" className="w-full gap-3 h-11 border-border hover:bg-accent">
              <Chrome className="w-4 h-4" />
              Continue with Google
            </Button>
          </a>
        </div>

        <p className="text-center text-xs text-muted-foreground">
          By continuing, you agree to our terms and privacy policy.
        </p>
      </div>
    </div>
  )
}
