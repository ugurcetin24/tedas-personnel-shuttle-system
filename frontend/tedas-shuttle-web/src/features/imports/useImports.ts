import { useMutation } from '@tanstack/react-query'
import {
  commitCapacityImport,
  commitPersonnelImport,
  commitRouteImport,
  previewCapacityImport,
  previewPersonnelImport,
  previewRouteImport,
} from './importApi'

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

export function usePreviewCapacityImport() {
  return useMutation({
    mutationFn: previewCapacityImport,
  })
}

export function useCommitCapacityImport() {
  return useMutation({
    mutationFn: commitCapacityImport,
  })
}

export function usePreviewRouteImport() {
  return useMutation({
    mutationFn: previewRouteImport,
  })
}

export function useCommitRouteImport() {
  return useMutation({
    mutationFn: commitRouteImport,
  })
}
