import { useMutation } from '@tanstack/react-query'
import { searchGeocoding } from './geocodingApi'

export function useGeocodingSearch() {
  return useMutation({
    mutationFn: ({ query, limit = 5 }: { query: string; limit?: number }) =>
      searchGeocoding(query, limit),
  })
}

