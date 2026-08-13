import Search from '@mui/icons-material/Search'
import {
  Box,
  Button,
  Card,
  CardContent,
  Divider,
  InputAdornment,
  Stack,
  TextField,
  Typography,
} from '@mui/material'

type PlaceholderPanelProps = {
  title: string
  primaryAction?: string
}

export function PlaceholderPanel({ title, primaryAction }: PlaceholderPanelProps) {
  return (
    <Card variant="outlined" sx={{ borderRadius: 1 }}>
      <CardContent>
        <Stack
          direction="row"
          spacing={2}
          sx={{ mb: 2, justifyContent: 'space-between', alignItems: 'center' }}
        >
          <TextField
            size="small"
            placeholder="Ara"
            sx={{ width: 320, maxWidth: '100%' }}
            slotProps={{
              input: {
                startAdornment: (
                  <InputAdornment position="start">
                    <Search fontSize="small" />
                  </InputAdornment>
                ),
              },
            }}
          />
          {primaryAction ? (
            <Button variant="contained" size="small">
              {primaryAction}
            </Button>
          ) : null}
        </Stack>
        <Divider />
        <Box sx={{ py: 8, textAlign: 'center' }}>
          <Typography color="text.secondary" sx={{ fontWeight: 700 }}>
            {title} kaydı bulunamadı
          </Typography>
        </Box>
      </CardContent>
    </Card>
  )
}
