import { apiClient } from '../../api/httpClient'
import type { GeocodingResult } from './geocodingTypes'

export async function searchGeocoding(query: string, limit = 5) {
  const response = await apiClient.get<GeocodingResult[]>('/api/geocoding/search', {
    params: { query, limit },
  })

  return response.data
}

