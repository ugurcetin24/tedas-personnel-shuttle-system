import CloudUpload from '@mui/icons-material/CloudUpload'
import {
  Alert,
  Button,
  Chip,
  Divider,
  Paper,
  Stack,
  Tab,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Typography,
} from '@mui/material'
import type { ChangeEvent, SyntheticEvent } from 'react'
import { useMemo, useState } from 'react'
import { PageHeader } from '../components/PageHeader'
import type { ImportAction, ImportStatus } from '../features/imports/importTypes'
import {
  useCommitCapacityImport,
  useCommitPersonnelImport,
  useCommitRouteImport,
  usePreviewCapacityImport,
  usePreviewPersonnelImport,
  usePreviewRouteImport,
} from '../features/imports/useImports'
import { getApiErrorMessage } from '../utils/apiErrors'

type ImportMode = 'personnel' | 'capacity' | 'route'

export function ImportsPage() {
  const [mode, setMode] = useState<ImportMode>('personnel')
  const [selectedFileName, setSelectedFileName] = useState('')
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const personnelPreviewMutation = usePreviewPersonnelImport()
  const personnelCommitMutation = useCommitPersonnelImport()
  const capacityPreviewMutation = usePreviewCapacityImport()
  const capacityCommitMutation = useCommitCapacityImport()
  const routePreviewMutation = usePreviewRouteImport()
  const routeCommitMutation = useCommitRouteImport()
  const previewMutation =
    mode === 'personnel' ? personnelPreviewMutation : mode === 'capacity' ? capacityPreviewMutation : routePreviewMutation
  const commitMutation =
    mode === 'personnel' ? personnelCommitMutation : mode === 'capacity' ? capacityCommitMutation : routeCommitMutation
  const preview = previewMutation.data
  const visibleRows = preview?.rows.slice(0, 50) ?? []
  const previewStats = useMemo(() => {
    const rows = preview?.rows ?? []

    return {
      valid: rows.filter((row) => row.status === 'Valid').length,
      warning: rows.filter((row) => row.status === 'Warning').length,
      error: rows.filter((row) => row.status === 'Error').length,
      create: rows.filter((row) => row.action === 'Create').length,
      update: rows.filter((row) => row.action === 'Update').length,
      noChange: rows.filter((row) => row.action === 'NoChange').length,
    }
  }, [preview?.rows])
  const canCommit = !!selectedFile && !!preview && previewStats.error === 0 && preview.rows.length > 0

  function handleModeChange(_: SyntheticEvent, value: ImportMode) {
    setMode(value)
    setSelectedFile(null)
    setSelectedFileName('')
    personnelPreviewMutation.reset()
    personnelCommitMutation.reset()
    capacityPreviewMutation.reset()
    capacityCommitMutation.reset()
    routePreviewMutation.reset()
    routeCommitMutation.reset()
  }

  function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0]
    event.target.value = ''

    if (!file) {
      return
    }

    setSelectedFileName(file.name)
    setSelectedFile(file)
    commitMutation.reset()
    previewMutation.mutate(file)
  }

  function handleCommit() {
    if (selectedFile) {
      commitMutation.mutate(selectedFile)
    }
  }

  return (
    <Stack spacing={3}>
      <Stack direction={{ xs: 'column', md: 'row' }} sx={{ justifyContent: 'space-between', gap: 2 }}>
        <PageHeader title="Excel Aktarim" />
        <Stack direction="row" spacing={1}>
          <Button component="label" variant="outlined" startIcon={<CloudUpload />}>
            Dosya Sec
            <input hidden type="file" accept=".xlsx,.xlsm" onChange={handleFileChange} />
          </Button>
          <Button
            variant="contained"
            disabled={!canCommit || commitMutation.isPending}
            onClick={handleCommit}
          >
            Iceri Aktar
          </Button>
        </Stack>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack spacing={2}>
          <Tabs value={mode} onChange={handleModeChange}>
            <Tab value="personnel" label="Personel" />
            <Tab value="capacity" label="Kapasite" />
            <Tab value="route" label="Guzergah" />
          </Tabs>

          <Stack direction={{ xs: 'column', md: 'row' }} sx={{ justifyContent: 'space-between', gap: 2 }}>
            <Stack spacing={0.5}>
              <Typography variant="subtitle1">{getModeTitle(mode)}</Typography>
              <Typography variant="body2" color="text.secondary">
                {selectedFileName || 'Henuz dosya secilmedi.'}
              </Typography>
            </Stack>
            {preview ? (
              <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', rowGap: 1 }}>
                <Chip size="small" color="success" variant="outlined" label={`Gecerli ${previewStats.valid}`} />
                <Chip size="small" color="warning" variant="outlined" label={`Uyari ${previewStats.warning}`} />
                <Chip size="small" color="error" variant="outlined" label={`Hata ${previewStats.error}`} />
                <Chip size="small" variant="outlined" label={`Yeni ${previewStats.create}`} />
                <Chip size="small" variant="outlined" label={`Guncelleme ${previewStats.update}`} />
                <Chip size="small" variant="outlined" label={`Degisiklik yok ${previewStats.noChange}`} />
              </Stack>
            ) : null}
          </Stack>

          {previewMutation.isPending ? <Alert severity="info">Excel dosyasi okunuyor.</Alert> : null}
          {previewMutation.isError ? (
            <Alert severity="error">{getApiErrorMessage(previewMutation.error)}</Alert>
          ) : null}
          {commitMutation.isError ? (
            <Alert severity="error">{getApiErrorMessage(commitMutation.error)}</Alert>
          ) : null}
          {commitMutation.data ? (
            <Alert severity="success">
              {commitMutation.data.createdCount} yeni, {commitMutation.data.updatedCount} guncelleme,{' '}
              {commitMutation.data.skippedCount} atlanan satir islendi.
            </Alert>
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
                      <TableCell width={128}>Aksiyon</TableCell>
                      {mode === 'personnel' ? (
                        <>
                          <TableCell>Sicil</TableCell>
                          <TableCell>Ad Soyad</TableCell>
                          <TableCell>Birim</TableCell>
                        </>
                      ) : mode === 'capacity' ? (
                        <>
                          <TableCell>Servis</TableCell>
                          <TableCell>Vardiya</TableCell>
                          <TableCell>Kapasite</TableCell>
                          <TableCell>Mevcut</TableCell>
                          <TableCell>Dolu</TableCell>
                        </>
                      ) : (
                        <>
                          <TableCell>Servis</TableCell>
                          <TableCell>Vardiya</TableCell>
                          <TableCell>Sira</TableCell>
                          <TableCell>Durak</TableCell>
                          <TableCell>Koordinat</TableCell>
                        </>
                      )}
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
                        <TableCell>{getActionLabel(row.action)}</TableCell>
                        {mode === 'personnel' ? (
                          <>
                            <TableCell>{row.normalizedData.RegistrationNumber ?? '-'}</TableCell>
                            <TableCell>
                              {[row.normalizedData.FirstName, row.normalizedData.LastName]
                                .filter(Boolean)
                                .join(' ') || '-'}
                            </TableCell>
                            <TableCell>{row.normalizedData.Department ?? '-'}</TableCell>
                          </>
                        ) : mode === 'capacity' ? (
                          <>
                            <TableCell>{row.normalizedData.PhysicalShuttleCode ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.ShiftName ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.Capacity ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.CurrentCapacity ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.Occupancy ?? '-'}</TableCell>
                          </>
                        ) : (
                          <>
                            <TableCell>{row.normalizedData.PhysicalShuttleCode ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.ShiftName ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.Order ?? '-'}</TableCell>
                            <TableCell>{row.normalizedData.Name ?? '-'}</TableCell>
                            <TableCell>
                              {row.normalizedData.Latitude && row.normalizedData.Longitude
                                ? `${row.normalizedData.Latitude}, ${row.normalizedData.Longitude}`
                                : '-'}
                            </TableCell>
                          </>
                        )}
                        <TableCell>{[...row.errors, ...row.warnings].join(' ') || '-'}</TableCell>
                      </TableRow>
                    ))}
                    {visibleRows.length === 0 ? (
                      <TableRow>
                        <TableCell colSpan={mode === 'personnel' ? 7 : 9} align="center" sx={{ py: 6, color: 'text.secondary' }}>
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

function getModeTitle(mode: ImportMode) {
  if (mode === 'personnel') {
    return 'Personel Excel onizleme'
  }

  if (mode === 'capacity') {
    return 'Servis kapasitesi Excel onizleme'
  }

  return 'Guzergah Excel onizleme'
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

function getActionLabel(action: ImportAction) {
  if (action === 'Create') {
    return 'Yeni'
  }

  if (action === 'Update') {
    return 'Guncelle'
  }

  if (action === 'NoChange') {
    return 'Degisiklik yok'
  }

  if (action === 'Conflict') {
    return 'Cakisma'
  }

  return 'Atla'
}
