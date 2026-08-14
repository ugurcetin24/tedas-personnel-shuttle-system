import AssignmentTurnedIn from '@mui/icons-material/AssignmentTurnedIn'
import DirectionsBus from '@mui/icons-material/DirectionsBus'
import EventSeat from '@mui/icons-material/EventSeat'
import Groups from '@mui/icons-material/Groups'
import Map from '@mui/icons-material/Map'
import Route from '@mui/icons-material/Route'
import {
  Alert,
  Box,
  Grid,
  LinearProgress,
  Paper,
  Stack,
  Typography,
} from '@mui/material'
import { MetricCard } from '../components/MetricCard'
import { PageHeader } from '../components/PageHeader'
import { useDashboardSummary } from '../features/dashboard/useDashboard'
import { getApiErrorMessage } from '../utils/apiErrors'

export function DashboardPage() {
  const dashboardQuery = useDashboardSummary()
  const summary = dashboardQuery.data
  const metrics = summary?.metrics

  return (
    <Stack spacing={3}>
      <PageHeader eyebrow="TEDAS" title="Dashboard" />

      {dashboardQuery.isLoading ? <LinearProgress /> : null}
      {dashboardQuery.isError ? (
        <Alert severity="error">{getApiErrorMessage(dashboardQuery.error)}</Alert>
      ) : null}

      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Toplam Personel" value={formatNumber(metrics?.totalPersonnel)} icon={<Groups color="primary" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Aktif Personel" value={formatNumber(metrics?.activePersonnel)} icon={<Groups color="success" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Toplam Servis" value={formatNumber(metrics?.totalShuttles)} icon={<DirectionsBus color="secondary" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Aktif Servis" value={formatNumber(metrics?.activeShuttles)} icon={<DirectionsBus color="success" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Aktif Vardiya" value={formatNumber(metrics?.activeShifts)} icon={<EventSeat color="primary" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Atanmis Personel" value={formatNumber(metrics?.assignedPersonnel)} icon={<AssignmentTurnedIn color="success" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Atanmamis Personel" value={formatNumber(metrics?.unassignedPersonnel)} icon={<AssignmentTurnedIn color="warning" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Guzergah Noktasi" value={formatNumber(metrics?.routePointCount)} icon={<Map color="info" />} />
        </Grid>
      </Grid>

      <Paper variant="outlined" sx={{ borderRadius: 1, p: 2.5 }}>
        <Stack spacing={2}>
          <Stack direction={{ xs: 'column', md: 'row' }} sx={{ justifyContent: 'space-between', gap: 1 }}>
            <Stack spacing={0.5}>
              <Typography variant="h6" sx={{ fontWeight: 800 }}>
                Servis Doluluklari
              </Typography>
              <Typography color="text.secondary" sx={{ fontSize: 14 }}>
                Aktif servis ve vardiyalar icin anlik kapasite ozeti.
              </Typography>
            </Stack>
            <Stack direction="row" spacing={1}>
              <Typography color="text.secondary" sx={{ fontSize: 14 }}>
                Kayitli rota: {formatNumber(metrics?.savedRouteCount)}
              </Typography>
              <Route color="info" fontSize="small" />
            </Stack>
          </Stack>

          {summary && summary.shiftOccupancies.length > 0 ? (
            <Stack spacing={1.5}>
              {summary.shiftOccupancies.map((shift) => (
                <Box key={shift.shuttleShiftId}>
                  <Stack direction="row" sx={{ justifyContent: 'space-between', gap: 2, mb: 0.75 }}>
                    <Typography sx={{ fontWeight: 700 }}>
                      {shift.physicalShuttleCode} / {shift.shiftName}
                    </Typography>
                    <Typography color="text.secondary" sx={{ fontSize: 14 }}>
                      {shift.occupancy} / {shift.capacity} dolu, {shift.availableSeats} bos
                    </Typography>
                  </Stack>
                  <LinearProgress
                    variant="determinate"
                    value={Math.min(shift.utilizationPercent, 100)}
                    sx={{ height: 10, borderRadius: 1 }}
                  />
                </Box>
              ))}
            </Stack>
          ) : dashboardQuery.isLoading ? null : (
            <Alert severity="info">Aktif servis vardiyasi bulunmuyor.</Alert>
          )}
        </Stack>
      </Paper>
    </Stack>
  )
}

function formatNumber(value: number | undefined) {
  return value?.toLocaleString('tr-TR') ?? '-'
}
