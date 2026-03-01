import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { useQueryClient } from '@tanstack/react-query'
import { Loader2 } from 'lucide-react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group'
import { projectsApi, taskProviderAuthApi } from '@/api/client'
import { queryKeys } from '@/lib/query-keys'
import { useAuth } from '@/lib/auth-context'

const schema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  description: z.string().optional(),
  provider: z.enum(['Internal', 'GitHub', 'AzureDevOps']),
  githubRepo: z.string().optional(),
  githubPat: z.string().optional(),
  adoOrgUrl: z.string().optional(),
  adoProject: z.string().optional(),
  adoPat: z.string().optional(),
})

type FormValues = z.infer<typeof schema>

interface Props {
  open: boolean
  onOpenChange: (open: boolean) => void
}

const STEP_FIELDS: Record<number, (keyof FormValues)[]> = {
  1: ['name'],
  2: ['provider'],
  3: [],
}

const providerOptions = [
  { value: 'Internal', label: 'None', description: 'Internal tasks only, no sync' },
  { value: 'GitHub', label: 'GitHub Issues', description: 'Sync issues from a GitHub repository' },
  { value: 'AzureDevOps', label: 'Azure DevOps Boards', description: 'Sync work items from an Azure DevOps project' },
] as const

export function CreateProjectDialog({ open, onOpenChange }: Props) {
  const queryClient = useQueryClient()
  const { user } = useAuth()
  const [step, setStep] = useState<1 | 2 | 3>(1)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [submitError, setSubmitError] = useState<string | null>(null)

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { provider: 'Internal' },
  })

  const provider = form.watch('provider')
  const needsCredentials = provider !== 'Internal'
  const totalSteps = needsCredentials ? 3 : 2

  const handleClose = () => {
    form.reset()
    setStep(1)
    setSubmitError(null)
    onOpenChange(false)
  }

  const handleNext = async () => {
    const valid = await form.trigger(STEP_FIELDS[step])
    if (!valid) return
    if (step === 2 && !needsCredentials) {
      await handleSubmit()
    } else {
      setStep((s) => (s < 3 ? ((s + 1) as 1 | 2 | 3) : s))
    }
  }

  const handleBack = () => setStep((s) => (s > 1 ? ((s - 1) as 1 | 2 | 3) : s))

  const handleSubmit = async () => {
    const values = form.getValues()
    setIsSubmitting(true)
    setSubmitError(null)
    try {
      const orgId = user?.personalOrganisationId ?? ''
      let taskProviderAuthId: string | undefined

      if (values.provider !== 'Internal') {
        taskProviderAuthId = await taskProviderAuthApi.create(
          values.provider === 'GitHub'
            ? {
                organisationId: orgId,
                authenticationType: 'GitHubApp',
                authTypeId: values.githubRepo ?? '',
                secretValue: values.githubPat ?? '',
              }
            : {
                organisationId: orgId,
                authenticationType: 'AzureAppRegistration',
                authTypeId: values.adoOrgUrl ?? '',
                secretValue: values.adoPat ?? '',
              },
        )
      }

      await projectsApi.create({
        name: values.name,
        description: values.description || undefined,
        organisationId: orgId || undefined,
        provider: values.provider,
        externalId:
          values.provider === 'GitHub'
            ? values.githubRepo
            : values.provider === 'AzureDevOps'
              ? values.adoProject
              : undefined,
        taskProviderAuthId,
        triggerSync: values.provider !== 'Internal',
      })

      await queryClient.invalidateQueries({ queryKey: queryKeys.projects.all })
      await queryClient.invalidateQueries({ queryKey: queryKeys.tasks.all })
      handleClose()
    } catch (err) {
      setSubmitError(err instanceof Error ? err.message : 'Failed to create project')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !isSubmitting && (o ? onOpenChange(o) : handleClose())}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>
            {step === 1 ? 'New project' : step === 2 ? 'Connect a provider' : 'Credentials'}
          </DialogTitle>
          <p className="text-xs text-muted-foreground">
            Step {step} of {totalSteps}
          </p>
        </DialogHeader>

        <div className="py-2 space-y-4">
          {/* Step 1 — Details */}
          {step === 1 && (
            <>
              <div className="space-y-1.5">
                <Label htmlFor="name">Project name</Label>
                <Input id="name" placeholder="My project" {...form.register('name')} />
                {form.formState.errors.name && (
                  <p className="text-xs text-destructive">{form.formState.errors.name.message}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="description">Description <span className="text-muted-foreground font-normal">(optional)</span></Label>
                <Textarea id="description" placeholder="What is this project about?" className="resize-none" rows={3} {...form.register('description')} />
              </div>
            </>
          )}

          {/* Step 2 — Provider */}
          {step === 2 && (
            <RadioGroup
              value={provider}
              onValueChange={(v) => form.setValue('provider', v as FormValues['provider'])}
              className="gap-3"
            >
              {providerOptions.map((opt) => (
                <label
                  key={opt.value}
                  className="flex items-start gap-3 p-3 rounded-lg border border-border cursor-pointer hover:bg-muted/30 transition-colors has-[[data-state=checked]]:border-primary has-[[data-state=checked]]:bg-primary/5"
                >
                  <RadioGroupItem value={opt.value} className="mt-0.5" />
                  <div>
                    <p className="text-sm font-medium">{opt.label}</p>
                    <p className="text-xs text-muted-foreground">{opt.description}</p>
                  </div>
                </label>
              ))}
            </RadioGroup>
          )}

          {/* Step 3 — Credentials */}
          {step === 3 && provider === 'GitHub' && (
            <>
              <div className="space-y-1.5">
                <Label htmlFor="githubRepo">Repository <span className="text-muted-foreground font-normal text-xs">(owner/repo)</span></Label>
                <Input id="githubRepo" placeholder="octocat/hello-world" {...form.register('githubRepo')} />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="githubPat">Personal access token</Label>
                <Input id="githubPat" type="password" placeholder="ghp_••••••••••••••••" {...form.register('githubPat')} />
                <p className="text-xs text-muted-foreground">Needs <code className="text-xs bg-muted px-1 rounded">repo</code> scope to read issues.</p>
              </div>
            </>
          )}

          {step === 3 && provider === 'AzureDevOps' && (
            <>
              <div className="space-y-1.5">
                <Label htmlFor="adoOrgUrl">Organisation URL</Label>
                <Input id="adoOrgUrl" placeholder="https://dev.azure.com/myorg" {...form.register('adoOrgUrl')} />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="adoProject">Project name</Label>
                <Input id="adoProject" placeholder="MyProject" {...form.register('adoProject')} />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="adoPat">Personal access token</Label>
                <Input id="adoPat" type="password" placeholder="••••••••••••••••" {...form.register('adoPat')} />
                <p className="text-xs text-muted-foreground">Needs <code className="text-xs bg-muted px-1 rounded">Work Items (Read)</code> permission.</p>
              </div>
            </>
          )}

          {submitError && (
            <p className="text-xs text-destructive bg-destructive/10 px-3 py-2 rounded-md">{submitError}</p>
          )}
        </div>

        <DialogFooter className="gap-2">
          {step > 1 && (
            <Button variant="outline" size="sm" onClick={handleBack} disabled={isSubmitting}>
              Back
            </Button>
          )}
          <Button size="sm" onClick={handleNext} disabled={isSubmitting} className="min-w-20">
            {isSubmitting ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : step === totalSteps ? (
              'Create project'
            ) : (
              'Next'
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
