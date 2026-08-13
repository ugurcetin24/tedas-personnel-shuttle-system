import { zodResolver } from '@hookform/resolvers/zod'
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { routePointFormSchema, type RoutePointFormSchema } from './routePointSchema'
import type { RoutePointFormValues, RoutePointListItem } from './routePointTypes'

type RoutePointFormDialogProps = {
  open: boolean
  mode: 'create' | 'edit'
  routePoint?: RoutePointListItem | null
  isSubmitting: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: RoutePointFormValues) => void
}

const defaultValues: RoutePointFormValues = {
  name: '',
  address: '',
  latitude: 39.92077,
  longitude: 32.85411,
}

export function RoutePointFormDialog({
  open,
  mode,
  routePoint,
  isSubmitting,
  errorMessage,
  onClose,
  onSubmit,
}: RoutePointFormDialogProps) {
  const form = useForm<RoutePointFormSchema>({
    resolver: zodResolver(routePointFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!open) {
      return
    }

    if (mode === 'edit' && routePoint) {
      form.reset({
        name: routePoint.name,
        address: routePoint.address ?? '',
        latitude: routePoint.latitude,
        longitude: routePoint.longitude,
      })
      return
    }

    form.reset(defaultValues)
  }, [form, mode, open, routePoint])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Guzergah Noktasi Ekle' : 'Guzergah Noktasi Duzenle'}</DialogTitle>
      <DialogContent>
        {errorMessage ? (
          <Alert severity="error" sx={{ mb: 2 }}>
            {errorMessage}
          </Alert>
        ) : null}
        <Grid
          container
          spacing={2}
          component="form"
          id="route-point-form"
          onSubmit={form.handleSubmit((values) => onSubmit(values))}
          sx={{ pt: 1 }}
        >
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="name"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField {...field} label="Nokta Adi" size="small" fullWidth error={!!fieldState.error} helperText={fieldState.error?.message} />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="address"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField {...field} label="Adres" size="small" fullWidth error={!!fieldState.error} helperText={fieldState.error?.message} />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="latitude"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Latitude"
                  type="number"
                  size="small"
                  fullWidth
                  value={field.value}
                  onChange={(event) => field.onChange(Number(event.target.value))}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="longitude"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Longitude"
                  type="number"
                  size="small"
                  fullWidth
                  value={field.value}
                  onChange={(event) => field.onChange(Number(event.target.value))}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Vazgec</Button>
        <Button type="submit" form="route-point-form" variant="contained" disabled={isSubmitting}>
          Kaydet
        </Button>
      </DialogActions>
    </Dialog>
  )
}

