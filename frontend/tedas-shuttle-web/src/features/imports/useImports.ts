import { useMutation } from '@tanstack/react-query'
import { previewPersonnelImport } from './importApi'

export function usePreviewPersonnelImport() {
  return useMutation({
    mutationFn: previewPersonnelImport,
  })
}
