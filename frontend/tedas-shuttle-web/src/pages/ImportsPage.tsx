import CloudUpload from '@mui/icons-material/CloudUpload'
import {
  Alert,
  Button,
  Chip,
  Divider,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import type { ChangeEvent } from 'react'
import { useMemo, useState } from 'react'
import { PageHeader } from '../components/PageHeader'
import type { ImportStatus } from '../features/imports/importTypes'
import { usePreviewPersonnelImport } from '../features/imports/useImports'
import { getApiErrorMessage } from '../utils/apiErrors'

export function ImportsPage() {
  const [selectedFileName, setSelectedFileName] = useState('')
  const previewMutation = usePreviewPersonnelImport()
  const preview = previewMutation.data
  const visibleRows = preview?.rows.slice(0, 50) ?? []
  const previewStats = useMemo(() => {
    const rows = preview?.rows ?? []

    return {
      valid: rows.filter((row) => row.status === 'Valid').length,
      warning: rows.filter((row) => row.status === 'Warning').length,
      error: rows.filter((row) => row.status === 'Error').length,
    }
  }, [preview?.rows])

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''

    if (!file) {
      return
    }

    setSelectedFileName(file.name)
    previewMutation.mutate(file)
  }

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', md: 'row' }} sx={{ justifyContent: 'space-between', gap: 2 }}>
        <PageHeader title="Excel Aktarim" />
        <Button component="label" variant="contained" startIcon={<CloudUpload />}>
          Dosya Sec
          <input hidden type="file" accept=".xlsx,.xlsm" onChange={handleFileChange} />
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack spacing={2}>
          <Stack direction={{ xs: 'column', md: 'row' }} sx={{ justifyContent: 'space-between', gap: 2 }}>
            <Stack spacing={0.5}>
              <Typography variant="subtitle1">Personel Excel onizleme</Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedFileName || 'Henuz dosya secilmedi.'}
              </Typography>
            </Stack>
            {preview ? (
              <Stack direction="row" spacing={1}>
                <Chip size="small" color="success" variant="outlined" label={`Gecerli ${previewStats.valid}`} />
                <Chip size="small" color="warning" variant="outlined" label={`Uyari ${previewStats.warning}`} />
                <Chip size="small" color="error" variant="outlined" label={`Hata ${previewStats.error}`} />
              </Stack>
            ) : null}
          </Stack>

          {previewMutation.isPending ? <Alert severity="info">Excel dosyasi okunuyor.</Alert> : null}
          {previewMutation.isError ? (
            <Alert severity="error">{getApiErrorMessage(previewMutation.error)}</Alert>
          ) : null}

          {preview ? (
            <>
              <Divider />
              <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
                <Typography variant="body2" color="text.secondary">
                  Sayfa: {preview.sheetName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Satir: {preview.rows.length}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Baslik: {preview.headers.length}
                </Typography>
              </Stack>

              <TableContainer variant="outlined" component={Paper} sx={{ borderRadius: 1 }}>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell width={96}>Satir</TableCell>
                      <TableCell width={120}>Durum</TableCell>
                      <TableCell>Sicil</TableCell>
                      <TableCell>Ad Soyad</TableCell>
                      <TableCell>Birim</TableCell>
                      <TableCell>Aciklama</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {visibleRows.map((row) => (
                      <TableRow key={row.rowNumber} hover>
                        <TableCell>{row.rowNumber}</TableCell>
                        <TableCell>
                          <Chip
                            size="small"
                            color={getStatusColor(row.status)}
                            variant="outlined"
                            label={getStatusLabel(row.status)}
                          />
                        </TableCell>
                        <TableCell>{row.normalizedData.RegistrationNumber ?? '-'}</TableCell>
                        <TableCell>
                          {[row.normalizedData.FirstName, row.normalizedData.LastName].filter(Boolean).join(' ') ||
                            '-'}
                        </TableCell>
                        <TableCell>{row.normalizedData.Department ?? '-'}</TableCell>
                        <TableCell>{[...row.errors, ...row.warnings].join(' ') || '-'}</TableCell>
                      </TableRow>
                    ))}
                    {visibleRows.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={6} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                          Onizlenecek satir bulunamadi.
                        </TableCell>
                      </TableRow>
                    ) : null}
                  </TableBody>
                </Table>
              </TableContainer>
            </>
          ) : null}
        </Stack>
      </Paper>
    </Stack>
  )
}

function getStatusLabel(status: ImportStatus) {
  if (status === 'Valid') {
    return 'Gecerli'
  }

  if (status === 'Warning') {
    return 'Uyari'
  }

  return 'Hata'
}

function getStatusColor(status: ImportStatus) {
  if (status === 'Valid') {
    return 'success'
  }

  if (status === 'Warning') {
    return 'warning'
  }

  return 'error'
}
