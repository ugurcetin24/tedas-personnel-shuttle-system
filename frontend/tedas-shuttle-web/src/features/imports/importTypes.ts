export type ImportStatus = 'Valid' | 'Warning' | 'Error'

export type ColumnMappingSuggestion = {
  sourceHeader: string
  targetField: string
  confidence: number
}

export type ExcelPreviewRow = {
  rowNumber: number
  status: ImportStatus
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
