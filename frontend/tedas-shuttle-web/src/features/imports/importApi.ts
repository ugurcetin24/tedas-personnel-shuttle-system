import { apiClient } from '../../api/httpClient'
import type { ExcelImportPreview } from './importTypes'

export async function previewPersonnelImport(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<ExcelImportPreview>('/api/imports/personnel/preview', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })

  return response.data
}
