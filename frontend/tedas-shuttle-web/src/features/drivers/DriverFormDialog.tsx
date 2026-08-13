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
import { driverFormSchema, type DriverFormSchema } from './driverSchema'
import type { DriverFormValues, DriverListItem } from './driverTypes'

type DriverFormDialogProps = {
  open: boolean
  mode: 'create' | 'edit'
  driver?: DriverListItem | null
  isSubmitting: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: DriverFormValues) => void
}

const defaultValues: DriverFormValues = {
  firstName: '',
  lastName: '',
  phone: '',
  licenseNumber: '',
}

export function DriverFormDialog({
  open,
  mode,
  driver,
  isSubmitting,
  errorMessage,
  onClose,
  onSubmit,
}: DriverFormDialogProps) {
  const form = useForm<DriverFormSchema>({
    resolver: zodResolver(driverFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!open) {
      return
    }

    if (mode === 'edit' && driver) {
      form.reset({
        firstName: driver.firstName,
        lastName: driver.lastName,
        phone: driver.phone ?? '',
        licenseNumber: driver.licenseNumber,
      })
      return
    }

    form.reset(defaultValues)
  }, [driver, form, mode, open])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Sofor Ekle' : 'Sofor Duzenle'}</DialogTitle>
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
          id="driver-form"
          onSubmit={form.handleSubmit((values) => onSubmit(values))}
          sx={{ pt: 1 }}
        >
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="firstName"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Ad"
                  size="small"
                  fullWidth
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="lastName"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Soyad"
                  size="small"
                  fullWidth
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="phone"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Telefon"
                  size="small"
                  fullWidth
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="licenseNumber"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Ehliyet No"
                  size="small"
                  fullWidth
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
        <Button type="submit" form="driver-form" variant="contained" disabled={isSubmitting}>
          Kaydet
        </Button>
      </DialogActions>
    </Dialog>
  )
}

