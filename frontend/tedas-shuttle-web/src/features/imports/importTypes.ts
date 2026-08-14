export type ImportStatus = 'Valid' | 'Warning' | 'Error'

export type ImportAction = 'Create' | 'Update' | 'NoChange' | 'Conflict' | 'Skip'

export type ColumnMappingSuggestion = {
  sourceHeader: string
  targetField: string
  confidence: number
}

export type ExcelPreviewRow = {
  rowNumber: number
  status: ImportStatus
  action: ImportAction
  normalizedData: Record<string, string | null>
  errors: string[]
  warnings: string[]
}

export type ExcelImportPreview = {
  fileName: string
  sheetName: string
  headers: string[]
  mappingSuggestions: ColumnMappingSuggestion[]
  rows: ExcelPreviewRow[]
}

export type PersonnelImportCommitResult = {
  createdCount: number
  updatedCount: number
  skippedCount: number
  rows: ExcelPreviewRow[]
}
