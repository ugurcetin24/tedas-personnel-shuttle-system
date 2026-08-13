import { Stack, Typography } from '@mui/material'

type PageHeaderProps = {
  title: string
  eyebrow?: string
}

export function PageHeader({ title, eyebrow }: PageHeaderProps) {
  return (
    <Stack spacing={0.5} sx={{ mb: 3 }}>
      {eyebrow ? (
        <Typography variant="overline" color="primary.main" sx={{ fontWeight: 800 }}>
          {eyebrow}
        </Typography>
      ) : null}
      <Typography variant="h4" sx={{ fontWeight: 800 }}>
        {title}
      </Typography>
    </Stack>
  )
}
