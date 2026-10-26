import { useQuery } from '@tanstack/react-query'
import { z } from 'zod'
import { getJson } from './client'

const statusSchema = z.object({
  databaseReachable: z.boolean(),
})

export type Status = z.infer<typeof statusSchema>

export function useStatus() {
  return useQuery({
    queryKey: ['status'],
    queryFn: () => getJson('/api/status', statusSchema),
  })
}
