import { useQuery } from '@tanstack/react-query'
import { getHealth } from '../api/health'

export function useApiHealth() {
  return useQuery({
    queryKey: ['api-health'],
    queryFn: getHealth,
    staleTime: 30_000,
  })
}
