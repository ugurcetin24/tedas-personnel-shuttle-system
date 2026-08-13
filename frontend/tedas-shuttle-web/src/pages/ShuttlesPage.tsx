import Add from '@mui/icons-material/Add'
import Edit from '@mui/icons-material/Edit'
import Visibility from '@mui/icons-material/Visibility'
import {
  Alert,
  Button,
  Chip,
  FormControl,
  IconButton,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TablePagination,
  TableRow,
  TextField,
  Tooltip,
} from '@mui/material'
import { useMemo, useState } from 'react'
import { Link } from 'react-router'
import { PageHeader } from '../components/PageHeader'
import { ShuttleFormDialog } from '../features/shuttles/ShuttleFormDialog'
import type { ShuttleFormValues, ShuttleListItem } from '../features/shuttles/shuttleTypes'
import {
  useCreateShuttle,
  useShuttles,
  useUpdateShuttle,
  useUpdateShuttleStatus,
} from '../features/shuttles/useShuttles'
import { getApiErrorMessage } from '../utils/apiErrors'

const pageSizeOptions = [10, 25, 50]

export function ShuttlesPage() {
  const [page, setPage] = useState(0)
  const [pageSize, setPageSize] = useState(10)
  const [code, setCode] = useState('')
  const [plateNumber, setPlateNumber] = useState('')
  const [isActiveFilter, setIsActiveFilter] = useState<'all' | 'active' | 'inactive'>('all')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selectedShuttle, setSelectedShuttle] = useState<ShuttleListItem | null>(null)

  const query = useMemo(
    () => ({
      page: page + 1,
      pageSize,
      code: code.trim() || undefined,
      plateNumber: plateNumber.trim() || undefined,
      isActive:
        isActiveFilter === 'all'
          ? undefined
          : isActiveFilter === 'active',
    }),
    [code, isActiveFilter, page, pageSize, plateNumber],
  )

  const shuttlesQuery = useShuttles(query)
  const createShuttleMutation = useCreateShuttle()
  const updateShuttleMutation = useUpdateShuttle()
  const updateStatusMutation = useUpdateShuttleStatus()

  const mutationError =
    createShuttleMutation.error ??
    updateShuttleMutation.error ??
    updateStatusMutation.error

  function openCreateDialog() {
    setSelectedShuttle(null)
    setDialogOpen(true)
  }

  function openEditDialog(shuttle: ShuttleListItem) {
    setSelectedShuttle(shuttle)
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
    createShuttleMutation.reset()
    updateShuttleMutation.reset()
  }

  function handleSubmit(values: ShuttleFormValues) {
    if (selectedShuttle) {
      updateShuttleMutation.mutate(
        {
          id: selectedShuttle.id,
          values: {
            plateNumber: values.plateNumber,
            description: values.description,
          },
        },
        { onSuccess: closeDialog },
      )
      return
    }

    createShuttleMutation.mutate(values, { onSuccess: closeDialog })
  }

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <PageHeader title="Servisler" />
        <Button startIcon={<Add />} variant="contained" onClick={openCreateDialog}>
          Servis Ekle
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack direction={{ xs: 'column', md: 'row' }} spacing={2}>
          <TextField
            label="Servis kodu ara"
            size="small"
            value={code}
            onChange={(event) => {
              setCode(event.target.value)
              setPage(0)
            }}
            sx={{ width: { xs: '100%', md: 260 } }}
          />
          <TextField
            label="Plaka ara"
            size="small"
            value={plateNumber}
            onChange={(event) => {
              setPlateNumber(event.target.value)
              setPage(0)
            }}
            sx={{ width: { xs: '100%', md: 260 } }}
          />
          <FormControl size="small" sx={{ width: { xs: '100%', md: 220 } }}>
            <InputLabel id="shuttle-status-filter-label">Durum</InputLabel>
            <Select
              labelId="shuttle-status-filter-label"
              label="Durum"
              value={isActiveFilter}
              onChange={(event) => {
                setIsActiveFilter(event.target.value as typeof isActiveFilter)
                setPage(0)
              }}
            >
              <MenuItem value="all">Tümü</MenuItem>
              <MenuItem value="active">Aktif</MenuItem>
              <MenuItem value="inactive">Pasif</MenuItem>
            </Select>
          </FormControl>
        </Stack>
      </Paper>

      {shuttlesQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(shuttlesQuery.error)}</Alert>
      ) : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Kod</TableCell>
              <TableCell>Plaka</TableCell>
              <TableCell>Açıklama</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {shuttlesQuery.data?.items.map((shuttle) => (
              <TableRow key={shuttle.id} hover>
                <TableCell>{shuttle.code}</TableCell>
                <TableCell>{shuttle.plateNumber}</TableCell>
                <TableCell>{shuttle.description ?? '-'}</TableCell>
                <TableCell>
                  <Chip
                    label={shuttle.isActive ? 'Aktif' : 'Pasif'}
                    color={shuttle.isActive ? 'success' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Vardiyalar">
                    <IconButton component={Link} to={`/shuttles/${shuttle.id}`} size="small">
                      <Visibility fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title="Düzenle">
                    <IconButton size="small" onClick={() => openEditDialog(shuttle)}>
                      <Edit fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={shuttle.isActive ? 'Pasif yap' : 'Aktif yap'}>
                    <Switch
                      size="small"
                      checked={shuttle.isActive}
                      disabled={updateStatusMutation.isPending}
                      onChange={(event) =>
                        updateStatusMutation.mutate({
                          id: shuttle.id,
                          isActive: event.target.checked,
                        })
                      }
                    />
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
            {shuttlesQuery.data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={5} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                  Servis kaydı bulunamadı.
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
        <TablePagination
          component="div"
          count={shuttlesQuery.data?.totalCount ?? 0}
          page={page}
          rowsPerPage={pageSize}
          rowsPerPageOptions={pageSizeOptions}
          labelRowsPerPage="Sayfa boyutu"
          onPageChange={(_, nextPage) => setPage(nextPage)}
          onRowsPerPageChange={(event) => {
            setPageSize(Number(event.target.value))
            setPage(0)
          }}
        />
      </TableContainer>

      <ShuttleFormDialog
        open={dialogOpen}
        mode={selectedShuttle ? 'edit' : 'create'}
        shuttle={selectedShuttle}
        isSubmitting={createShuttleMutation.isPending || updateShuttleMutation.isPending}
        errorMessage={
          createShuttleMutation.error
            ? getApiErrorMessage(createShuttleMutation.error)
            : updateShuttleMutation.error
              ? getApiErrorMessage(updateShuttleMutation.error)
              : null
        }
        onClose={closeDialog}
        onSubmit={handleSubmit}
      />
    </Stack>
  )
}
