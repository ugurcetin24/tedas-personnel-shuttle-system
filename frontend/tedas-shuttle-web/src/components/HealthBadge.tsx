import CheckCircle from '@mui/icons-material/CheckCircle'
import ErrorOutlined from '@mui/icons-material/ErrorOutlined'
import { Chip, CircularProgress, Tooltip } from '@mui/material'
import { useApiHealth } from '../hooks/useApiHealth'

export function HealthBadge() {
  const healthQuery = useApiHealth()

  if (healthQuery.isLoading) {
    return (
      <Chip
        icon={<CircularProgress size={16} />}
        label="API kontrol"
        variant="outlined"
        size="small"
      />
    )
  }

  if (healthQuery.isError || !healthQuery.data?.database.canConnect) {
    return (
      <Tooltip title="Backend health endpoint yanıt vermiyor">
        <Chip
          icon={<ErrorOutlined />}
          label="API bağlantısı yok"
          color="error"
          variant="outlined"
          size="small"
        />
      </Tooltip>
    )
  }

  return (
    <Tooltip title={healthQuery.data.database.provider}>
      <Chip icon={<CheckCircle />} label="API bağlı" color="success" variant="outlined" size="small" />
    </Tooltip>
  )
}
