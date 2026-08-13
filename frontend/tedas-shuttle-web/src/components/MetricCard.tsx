import { Card, CardContent, Stack, Typography } from '@mui/material'
import type { ReactNode } from 'react'

type MetricCardProps = {
  label: string
  value: string
  icon: ReactNode
}

export function MetricCard({ label, value, icon }: MetricCardProps) {
  return (
    <Card variant="outlined" sx={{ borderRadius: 1, height: '100%' }}>
      <CardContent>
        <Stack direction="row" spacing={2} sx={{ justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <Stack spacing={0.5}>
            <Typography color="text.secondary" sx={{ fontSize: 13, fontWeight: 700 }}>
              {label}
            </Typography>
            <Typography variant="h4" sx={{ fontWeight: 800 }}>
              {value}
            </Typography>
          </Stack>
          {icon}
        </Stack>
      </CardContent>
    </Card>
  )
}
