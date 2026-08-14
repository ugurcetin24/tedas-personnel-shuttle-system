import { apiClient } from '../../api/httpClient'
import type { CapacityImportCommitResult, ExcelImportPreview, PersonnelImportCommitResult } from './importTypes'

export async function previewPersonnelImport(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<ExcelImportPreview>('/api/imports/personnel/preview', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })

  return response.data
}

export async function commitPersonnelImport(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<PersonnelImportCommitResult>(
    '/api/imports/personnel/commit',
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
    },
  )

  return response.data
}

export async function previewCapacityImport(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<ExcelImportPreview>('/api/imports/capacity/preview', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })

  return response.data
}

export async function commitCapacityImport(file: File) {
  const formData = new FormData()
  formData.append('file', file)

  const response = await apiClient.post<CapacityImportCommitResult>(
    '/api/imports/capacity/commit',
    formData,
    {
      headers: { 'Content-Type': 'multipart/form-data' },
    },
  )

  return response.data
}
