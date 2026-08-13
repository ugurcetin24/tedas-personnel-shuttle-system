import axios from 'axios'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5284',
  timeout: 10_000,
  headers: {
    Accept: 'application/json',
  },
})
