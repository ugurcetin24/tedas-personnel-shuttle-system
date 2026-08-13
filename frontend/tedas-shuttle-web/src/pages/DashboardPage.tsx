import AssignmentTurnedIn from '@mui/icons-material/AssignmentTurnedIn'
import DirectionsBus from '@mui/icons-material/DirectionsBus'
import Groups from '@mui/icons-material/Groups'
import Route from '@mui/icons-material/Route'
import { Box, Grid, LinearProgress, Stack, Typography } from '@mui/material'
import { MetricCard } from '../components/MetricCard'
import { PageHeader } from '../components/PageHeader'

export function DashboardPage() {
  return (
    <Stack spacing={3}>
      <PageHeader eyebrow="TEDAŞ" title="Dashboard" />
      <Grid container spacing={2}>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Toplam Personel" value="0" icon={<Groups color="primary" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Aktif Servis" value="0" icon={<DirectionsBus color="secondary" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Atanmış Personel" value="0" icon={<AssignmentTurnedIn color="success" />} />
        </Grid>
        <Grid size={{ xs: 12, md: 6, lg: 3 }}>
          <MetricCard label="Kayıtlı Güzergâh" value="0" icon={<Route color="info" />} />
        </Grid>
      </Grid>
      <Box sx={{ border: 1, borderColor: 'divider', borderRadius: 1, p: 2.5, bgcolor: 'background.paper' }}>
        <Stack spacing={1.5}>
          <Typography variant="h6" sx={{ fontWeight: 800 }}>
            Servis Dolulukları
          </Typography>
          <LinearProgress variant="determinate" value={0} sx={{ height: 10, borderRadius: 1 }} />
          <Typography color="text.secondary" sx={{ fontSize: 14 }}>
            Henüz servis vardiyası kaydı bulunmuyor.
          </Typography>
        </Stack>
      </Box>
    </Stack>
  )
}
