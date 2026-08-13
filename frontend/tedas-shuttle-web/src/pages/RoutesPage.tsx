import Add from '@mui/icons-material/Add'
import ArrowDownward from '@mui/icons-material/ArrowDownward'
import ArrowUpward from '@mui/icons-material/ArrowUpward'
import Edit from '@mui/icons-material/Edit'
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
  TableRow,
  Tooltip,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { PageHeader } from '../components/PageHeader'
import { RoutePointFormDialog } from '../features/routePoints/RoutePointFormDialog'
import type { RoutePointFormValues, RoutePointListItem } from '../features/routePoints/routePointTypes'
import {
  useCreateRoutePoint,
  useReorderRoutePoints,
  useRoutePoints,
  useUpdateRoutePoint,
  useUpdateRoutePointStatus,
} from '../features/routePoints/useRoutePoints'
import { shiftTypes } from '../features/shifts/shiftTypes'
import { useAllShifts } from '../features/shifts/useShifts'
import { getApiErrorMessage } from '../utils/apiErrors'

export function RoutesPage() {
  const shiftsQuery = useAllShifts(true)
  const [selectedShiftId, setSelectedShiftId] = useState('')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selectedRoutePoint, setSelectedRoutePoint] = useState<RoutePointListItem | null>(null)

  useEffect(() => {
    if (!selectedShiftId && shiftsQuery.data?.length) {
      setSelectedShiftId(shiftsQuery.data[0].id)
    }
  }, [selectedShiftId, shiftsQuery.data])

  const routePointsQuery = useRoutePoints(selectedShiftId)
  const createRoutePointMutation = useCreateRoutePoint(selectedShiftId)
  const updateRoutePointMutation = useUpdateRoutePoint(selectedShiftId)
  const updateStatusMutation = useUpdateRoutePointStatus(selectedShiftId)
  const reorderMutation = useReorderRoutePoints(selectedShiftId)

  const mutationError =
    createRoutePointMutation.error ??
    updateRoutePointMutation.error ??
    updateStatusMutation.error ??
    reorderMutation.error

  function openCreateDialog() {
    setSelectedRoutePoint(null)
    setDialogOpen(true)
  }

  function openEditDialog(routePoint: RoutePointListItem) {
    setSelectedRoutePoint(routePoint)
    setDialogOpen(true)
  }

  function closeDialog() {
    setDialogOpen(false)
    createRoutePointMutation.reset()
    updateRoutePointMutation.reset()
  }

  function handleSubmit(values: RoutePointFormValues) {
    if (selectedRoutePoint) {
      updateRoutePointMutation.mutate(
        { id: selectedRoutePoint.id, values },
        { onSuccess: closeDialog },
      )
      return
    }

    createRoutePointMutation.mutate(values, { onSuccess: closeDialog })
  }

  function moveRoutePoint(index: number, direction: -1 | 1) {
    const routePoints = routePointsQuery.data ?? []
    const nextIndex = index + direction
    if (nextIndex < 0 || nextIndex >= routePoints.length) {
      return
    }

    const ids = routePoints.map((routePoint) => routePoint.id)
    const current = ids[index]
    ids[index] = ids[nextIndex]
    ids[nextIndex] = current
    reorderMutation.mutate(ids)
  }

  return (
    <Stack spacing={3}>
      <Stack direction="row" sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 2 }}>
        <PageHeader title="Guzergahlar" />
        <Button startIcon={<Add />} variant="contained" onClick={openCreateDialog} disabled={!selectedShiftId}>
          Nokta Ekle
        </Button>
      </Stack>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <FormControl size="small" sx={{ width: { xs: '100%', md: 420 } }}>
          <InputLabel id="route-shift-label">Vardiya</InputLabel>
          <Select
            labelId="route-shift-label"
            label="Vardiya"
            value={selectedShiftId}
            onChange={(event) => setSelectedShiftId(event.target.value)}
          >
            {shiftsQuery.data?.map((shift) => (
              <MenuItem key={shift.id} value={shift.id}>
                {shift.physicalShuttleCode} / {shift.name} ({getShiftTypeLabel(shift.shiftType)})
              </MenuItem>
            ))}
          </Select>
        </FormControl>
      </Paper>

      {shiftsQuery.isError ? <Alert severity="error">{getApiErrorMessage(shiftsQuery.error)}</Alert> : null}
      {routePointsQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(routePointsQuery.error)}</Alert>
      ) : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      <TableContainer component={Paper} variant="outlined" sx={{ borderRadius: 1 }}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell width={88}>Sira</TableCell>
              <TableCell>Nokta</TableCell>
              <TableCell>Adres</TableCell>
              <TableCell>Koordinat</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell align="right">Islemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {routePointsQuery.data?.map((routePoint, index) => (
              <TableRow key={routePoint.id} hover>
                <TableCell>{routePoint.order}</TableCell>
                <TableCell>{routePoint.name}</TableCell>
                <TableCell>{routePoint.address ?? '-'}</TableCell>
                <TableCell>
                  {routePoint.latitude}, {routePoint.longitude}
                </TableCell>
                <TableCell>
                  <Chip
                    label={routePoint.isActive ? 'Aktif' : 'Pasif'}
                    color={routePoint.isActive ? 'success' : 'default'}
                    size="small"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell align="right">
                  <Tooltip title="Yukari tasi">
                    <span>
                      <IconButton size="small" disabled={index === 0 || reorderMutation.isPending} onClick={() => moveRoutePoint(index, -1)}>
                        <ArrowUpward fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                  <Tooltip title="Asagi tasi">
                    <span>
                      <IconButton
                        size="small"
                        disabled={index === (routePointsQuery.data?.length ?? 0) - 1 || reorderMutation.isPending}
                        onClick={() => moveRoutePoint(index, 1)}
                      >
                        <ArrowDownward fontSize="small" />
                      </IconButton>
                    </span>
                  </Tooltip>
                  <Tooltip title="Duzenle">
                    <IconButton size="small" onClick={() => openEditDialog(routePoint)}>
                      <Edit fontSize="small" />
                    </IconButton>
                  </Tooltip>
                  <Tooltip title={routePoint.isActive ? 'Pasif yap' : 'Aktif yap'}>
                    <Switch
                      size="small"
                      checked={routePoint.isActive}
                      disabled={updateStatusMutation.isPending}
                      onChange={(event) =>
                        updateStatusMutation.mutate({
                          id: routePoint.id,
                          isActive: event.target.checked,
                        })
                      }
                    />
                  </Tooltip>
                </TableCell>
              </TableRow>
            ))}
            {routePointsQuery.data?.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 6, color: 'text.secondary' }}>
                  Guzergah noktasi bulunamadi.
                </TableCell>
              </TableRow>
            ) : null}
          </TableBody>
        </Table>
      </TableContainer>

      <RoutePointFormDialog
        open={dialogOpen}
        mode={selectedRoutePoint ? 'edit' : 'create'}
        routePoint={selectedRoutePoint}
        isSubmitting={createRoutePointMutation.isPending || updateRoutePointMutation.isPending}
        errorMessage={
          createRoutePointMutation.error
            ? getApiErrorMessage(createRoutePointMutation.error)
            : updateRoutePointMutation.error
              ? getApiErrorMessage(updateRoutePointMutation.error)
              : null
        }
        onClose={closeDialog}
        onSubmit={handleSubmit}
      />
    </Stack>
  )
}

function getShiftTypeLabel(value: number) {
  return shiftTypes.find((type) => type.value === value)?.label ?? '-'
}

