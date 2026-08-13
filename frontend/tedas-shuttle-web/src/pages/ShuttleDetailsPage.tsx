import Add from '@mui/icons-material/Add'
import ArrowBack from '@mui/icons-material/ArrowBack'
import Edit from '@mui/icons-material/Edit'
import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  LinearProgress,
  Paper,
  Stack,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material'
import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { PageHeader } from '../components/PageHeader'
import { ShiftFormDialog } from '../features/shifts/ShiftFormDialog'
import { shiftTypes, type ShiftFormValues, type ShiftListItem } from '../features/shifts/shiftTypes'
import {
  useCreateShift,
  useShifts,
  useUpdateShift,
  useUpdateShiftStatus,
} from '../features/shifts/useShifts'
import { useShuttle } from '../features/shuttles/useShuttles'
import { getApiErrorMessage } from '../utils/apiErrors'

export function ShuttleDetailsPage() {
  const { id } = useParams<{ id: string }>()
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selectedShift, setSelectedShift] = useState<ShiftListItem | null>(null)

  const shuttleQuery = useShuttle(id)
  const shiftsQuery = useShifts(id)
  const createShiftMutation = useCreateShift(id)
  const updateShiftMutation = useUpdateShift(id)
  const updateStatusMutation = useUpdateShiftStatus(id)

  const mutationError =
    createShiftMutation.error ?? updateShiftMutation.error ?? updateStatusMutation.error

  function openCreateDialog() {
    setSelectedShift(null)
    setDialogOpen(true)
  }

  function openEditDialog(shift: ShiftListItem) {
    setSelectedShift(shift)
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
    createShiftMutation.reset()
    updateShiftMutation.reset()
  }

  function handleSubmit(values: ShiftFormValues) {
    if (selectedShift) {
      updateShiftMutation.mutate(
        {
          id: selectedShift.id,
          values,
        },
        { onSuccess: closeDialog },
      )
      return
    }

    createShiftMutation.mutate(values, { onSuccess: closeDialog })
  }

  const isLoading = shuttleQuery.isLoading || shiftsQuery.isLoading

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <Stack spacing={1}>
          <Button
            component={Link}
            to="/shuttles"
            startIcon={<ArrowBack />}
            size="small"
            sx={{ alignSelf: 'flex-start' }}
          >
            Servislere Don
          </Button>
          <PageHeader title={shuttleQuery.data?.code ?? 'Servis Detayi'} />
        </Stack>
        <Button startIcon={<Add />} variant="contained" onClick={openCreateDialog} disabled={!id}>
          Vardiya Ekle
        </Button>
      </Stack>

      {isLoading ? <LinearProgress /> : null}
      {shuttleQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(shuttleQuery.error)}</Alert>
      ) : null}
      {shiftsQuery.isError ? <Alert severity="error">{getApiErrorMessage(shiftsQuery.error)}</Alert> : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      {shuttleQuery.data ? (
        <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
          <Stack direction={{ xs: 'column', md: 'row' }} spacing={3}>
            <InfoItem label="Servis Kodu" value={shuttleQuery.data.code} />
            <InfoItem label="Plaka" value={shuttleQuery.data.plateNumber} />
            <InfoItem label="Aciklama" value={shuttleQuery.data.description ?? '-'} />
            <Box>
              <Typography variant="caption" color="text.secondary">
                Durum
              </Typography>
              <Box sx={{ mt: 0.5 }}>
                <Chip
                  label={shuttleQuery.data.isActive ? 'Aktif' : 'Pasif'}
                  color={shuttleQuery.data.isActive ? 'success' : 'default'}
                  size="small"
                  variant="outlined"
                />
              </Box>
            </Box>
          </Stack>
        </Paper>
      ) : null}

      <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Vardiya</TableCell>
              <TableCell>Tur</TableCell>
              <TableCell>Saat</TableCell>
              <TableCell align="right">Kapasite</TableCell>
              <TableCell align="right">Doluluk</TableCell>
              <TableCell align="right">Bos Koltuk</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell align="right">Islemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {shiftsQuery.data?.map((shift) => (
              <TableRow key={shift.id} hover>
                <TableCell>{shift.name}</TableCell>
                <TableCell>{getShiftTypeLabel(shift.shiftType)}</TableCell>
                <TableCell>
                  {formatTime(shift.startTime)} - {formatTime(shift.endTime)}
                </TableCell>
                <TableCell align="right">{shift.capacity}</TableCell>
                <TableCell align="right">{shift.occupancy}</TableCell>
                <TableCell align="right">{shift.availableSeats}</TableCell>
                <TableCell>
                  <Chip
                    label={shift.isActive ? 'Aktif' : 'Pasif'}
                    color={shift.isActive ? 'success' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Duzenle">
                    <IconButton size="small" onClick={() => openEditDialog(shift)}>
                      <Edit fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={shift.isActive ? 'Pasif yap' : 'Aktif yap'}>
                    <Switch
                      size="small"
                      checked={shift.isActive}
                      disabled={updateStatusMutation.isPending}
                      onChange={(event) =>
                        updateStatusMutation.mutate({
                          id: shift.id,
                          isActive: event.target.checked,
                        })
                      }
                    />
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
            {shiftsQuery.data?.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                  Vardiya kaydi bulunamadi.
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
      </TableContainer>

      <ShiftFormDialog
        open={dialogOpen}
        mode={selectedShift ? 'edit' : 'create'}
        shift={selectedShift}
        isSubmitting={createShiftMutation.isPending || updateShiftMutation.isPending}
        errorMessage={
          createShiftMutation.error
            ? getApiErrorMessage(createShiftMutation.error)
            : updateShiftMutation.error
              ? getApiErrorMessage(updateShiftMutation.error)
              : null
        }
        onClose={closeDialog}
        onSubmit={handleSubmit}
      />
    </Stack>
  )
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return (
    <Box>
      <Typography variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography variant="body2" sx={{ mt: 0.5 }}>
        {value}
      </Typography>
    </Box>
  )
}

function getShiftTypeLabel(value: number) {
  return shiftTypes.find((type) => type.value === value)?.label ?? '-'
}

function formatTime(value: string) {
  return value.slice(0, 5)
}

