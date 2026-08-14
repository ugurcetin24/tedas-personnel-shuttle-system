import Add from '@mui/icons-material/Add'
import ArrowDownward from '@mui/icons-material/ArrowDownward'
import ArrowUpward from '@mui/icons-material/ArrowUpward'
import Edit from '@mui/icons-material/Edit'
import {
  Alert,
  Button,
  Chip,
  Divider,
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
  TextField,
  Tooltip,
  Typography,
} from '@mui/material'
import { useEffect, useState } from 'react'
import { CircleMarker, MapContainer, Polyline, Popup, TileLayer, useMap } from 'react-leaflet'
import type { LatLngTuple } from 'leaflet'
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
import type { CalculatedRoute } from '../features/routing/routingTypes'
import { useCalculateRoute, useSavedRoutes, useSaveCalculatedRoute } from '../features/routing/useRouting'
import { shiftTypes } from '../features/shifts/shiftTypes'
import { useAllShifts } from '../features/shifts/useShifts'
import { getApiErrorMessage } from '../utils/apiErrors'

export function RoutesPage() {
  const shiftsQuery = useAllShifts(true)
  const [selectedShiftId, setSelectedShiftId] = useState('')
  const [dialogOpen, setDialogOpen] = useState(false)
  const [selectedRoutePoint, setSelectedRoutePoint] = useState<RoutePointListItem | null>(null)
  const [calculatedRoute, setCalculatedRoute] = useState<CalculatedRoute | null>(null)
  const [savedRouteName, setSavedRouteName] = useState('')

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
  const savedRoutesQuery = useSavedRoutes(selectedShiftId)
  const calculateRouteMutation = useCalculateRoute(selectedShiftId)
  const saveRouteMutation = useSaveCalculatedRoute(selectedShiftId)

  const mutationError =
    createRoutePointMutation.error ??
    updateRoutePointMutation.error ??
    updateStatusMutation.error ??
    reorderMutation.error ??
    calculateRouteMutation.error ??
    saveRouteMutation.error
  const routePoints = routePointsQuery.data ?? []
  const activeRoutePoints = routePoints.filter((routePoint) => routePoint.isActive)
  const mapCenter: LatLngTuple = activeRoutePoints.length
    ? [activeRoutePoints[0].latitude, activeRoutePoints[0].longitude]
    : [39.92077, 32.85411]
  const polylinePositions: LatLngTuple[] = activeRoutePoints.map((routePoint) => [
    routePoint.latitude,
    routePoint.longitude,
  ])
  const calculatedPolylinePositions: LatLngTuple[] =
    calculatedRoute?.coordinates.map((coordinate) => [coordinate.latitude, coordinate.longitude]) ?? []
  const viewportPositions = calculatedPolylinePositions.length
    ? calculatedPolylinePositions
    : polylinePositions

  useEffect(() => {
    setCalculatedRoute(null)
    setSavedRouteName('')
    calculateRouteMutation.reset()
    saveRouteMutation.reset()
  }, [selectedShiftId])

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

  function handleCalculateRoute() {
    calculateRouteMutation.mutate(undefined, {
      onSuccess: (route) => setCalculatedRoute(route),
    })
  }

  function handleSaveRoute() {
    saveRouteMutation.mutate(savedRouteName, {
      onSuccess: () => setSavedRouteName(''),
    })
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
      {savedRoutesQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(savedRoutesQuery.error)}</Alert>
      ) : null}
      {mutationError ? <Alert severity="error">{getApiErrorMessage(mutationError)}</Alert> : null}

      <Paper variant="outlined" sx={{ borderRadius: 1, overflow: 'hidden' }}>
        <MapContainer center={mapCenter} zoom={13} scrollWheelZoom style={{ height: 360, width: '100%' }}>
          <TileLayer
            attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          />
          <RouteMapViewport positions={viewportPositions} />
          {polylinePositions.length > 1 ? (
            <Polyline
              positions={polylinePositions}
              pathOptions={{ color: '#1769aa', dashArray: '8 8', weight: 3 }}
            />
          ) : null}
          {calculatedPolylinePositions.length > 1 ? (
            <Polyline positions={calculatedPolylinePositions} pathOptions={{ color: '#d97706', weight: 5 }} />
          ) : null}
          {activeRoutePoints.map((routePoint) => (
            <CircleMarker
              key={routePoint.id}
              center={[routePoint.latitude, routePoint.longitude]}
              radius={8}
              pathOptions={{ color: '#0b5e3c', fillColor: '#1f9d63', fillOpacity: 0.9 }}
            >
              <Popup>
                <strong>
                  {routePoint.order}. {routePoint.name}
                </strong>
                <br />
                {routePoint.address ?? '-'}
              </Popup>
            </CircleMarker>
          ))}
        </MapContainer>
      </Paper>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2 }}>
        <Stack spacing={2}>
          <Stack
            direction={{ xs: 'column', md: 'row' }}
            sx={{ alignItems: { xs: 'stretch', md: 'center' }, justifyContent: 'space-between', gap: 2 }}
          >
            <Stack spacing={0.5}>
              <Typography variant="subtitle1">OSRM rota hesabi</Typography>
              <Typography variant="body2" color="text.secondary">
                {calculatedRoute
                  ? `${formatDistance(calculatedRoute.distanceMeters)} / ${formatDuration(calculatedRoute.durationSeconds)}`
                  : 'Aktif noktalar siraya gore hesaplanir.'}
              </Typography>
            </Stack>
            <Button
              variant="outlined"
              disabled={!selectedShiftId || activeRoutePoints.length < 2 || calculateRouteMutation.isPending}
              onClick={handleCalculateRoute}
            >
              Rotayi Hesapla
            </Button>
          </Stack>

          <Stack direction={{ xs: 'column', md: 'row' }} spacing={1.5}>
            <TextField
              size="small"
              label="Kayit adi"
              value={savedRouteName}
              onChange={(event) => setSavedRouteName(event.target.value)}
              disabled={!calculatedRoute || saveRouteMutation.isPending}
              sx={{ maxWidth: { md: 360 } }}
              fullWidth
            />
            <Button
              variant="contained"
              disabled={!calculatedRoute || !savedRouteName.trim() || saveRouteMutation.isPending}
              onClick={handleSaveRoute}
            >
              Kaydet
            </Button>
          </Stack>

          <Divider />

          <Stack spacing={1}>
            <Typography variant="subtitle2">Kayitli rotalar</Typography>
            {(savedRoutesQuery.data ?? []).map((route) => (
              <Stack
                key={route.id}
                direction={{ xs: 'column', md: 'row' }}
                sx={{ justifyContent: 'space-between', gap: 0.5 }}
              >
                <Typography variant="body2">{route.name}</Typography>
                <Typography variant="body2" color="text.secondary">
                  {formatDistance(route.distanceMeters)} / {formatDuration(route.durationSeconds)}
                </Typography>
              </Stack>
            ))}
            {savedRoutesQuery.data?.length === 0 ? (
              <Typography variant="body2" color="text.secondary">
                Kayitli rota yok.
              </Typography>
            ) : null}
          </Stack>
        </Stack>
      </Paper>

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
            {routePoints.map((routePoint, index) => (
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
                        disabled={index === routePoints.length - 1 || reorderMutation.isPending}
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
            {routePoints.length === 0 ? (
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

function formatDistance(distanceMeters: number) {
  return `${(distanceMeters / 1000).toFixed(1)} km`
}

function formatDuration(durationSeconds: number) {
  return `${Math.round(durationSeconds / 60)} dk`
}

function RouteMapViewport({ positions }: { positions: LatLngTuple[] }) {
  const map = useMap()

  useEffect(() => {
    if (positions.length === 0) {
      map.setView([39.92077, 32.85411], 13)
      return
    }

    if (positions.length === 1) {
      map.setView(positions[0], 14)
      return
    }

    map.fitBounds(positions, { padding: [32, 32] })
  }, [map, positions])

  return null
}
