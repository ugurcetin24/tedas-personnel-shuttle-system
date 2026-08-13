import { apiClient } from './httpClient'

export type HealthResponse = {
  application: string
  status: string
  database: {
    provider: string
    canConnect: boolean
    path: string
  }
}

export async function getHealth() {
  const response = await apiClient.get<HealthResponse>('/health')

  return response.data
}
