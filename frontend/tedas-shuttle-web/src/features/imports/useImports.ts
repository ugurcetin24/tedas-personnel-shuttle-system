import { useMutation } from '@tanstack/react-query'
import { commitPersonnelImport, previewPersonnelImport } from './importApi'

export function usePreviewPersonnelImport() {
  return useMutation({
    mutationFn: previewPersonnelImport,
  })
}

export function useCommitPersonnelImport() {
  return useMutation({
    mutationFn: commitPersonnelImport,
  })
}
