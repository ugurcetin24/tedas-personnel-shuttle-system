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
import { shuttleFormSchema, type ShuttleFormSchema } from './shuttleSchema'
import type { ShuttleFormValues, ShuttleListItem } from './shuttleTypes'

type ShuttleFormDialogProps = {
  open: boolean
  mode: 'create' | 'edit'
  shuttle?: ShuttleListItem | null
  isSubmitting: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: ShuttleFormValues) => void
}

const defaultValues: ShuttleFormValues = {
  code: '',
  plateNumber: '',
  description: '',
}

export function ShuttleFormDialog({
  open,
  mode,
  shuttle,
  isSubmitting,
  errorMessage,
  onClose,
  onSubmit,
}: ShuttleFormDialogProps) {
  const form = useForm<ShuttleFormSchema>({
    resolver: zodResolver(shuttleFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!open) {
      return
    }

    if (mode === 'edit' && shuttle) {
      form.reset({
        code: shuttle.code,
        plateNumber: shuttle.plateNumber,
        description: shuttle.description ?? '',
      })
      return
    }

    form.reset(defaultValues)
  }, [form, mode, open, shuttle])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Servis Ekle' : 'Servis Düzenle'}</DialogTitle>
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
          id="shuttle-form"
          onSubmit={form.handleSubmit((values) => onSubmit(values))}
          sx={{ pt: 1 }}
        >
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="code"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Servis Kodu"
                  size="small"
                  fullWidth
                  disabled={mode === 'edit'}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="plateNumber"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Plaka"
                  size="small"
                  fullWidth
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12 }}>
            <Controller
              name="description"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Açıklama"
                  size="small"
                  fullWidth
                  multiline
                  minRows={3}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
        </Grid>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Vazgeç</Button>
        <Button type="submit" form="shuttle-form" variant="contained" disabled={isSubmitting}>
          Kaydet
        </Button>
      </DialogActions>
    </Dialog>
  )
}
