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
import { personnelFormSchema, type PersonnelFormSchema } from './personnelSchema'
import type { PersonnelFormValues, PersonnelListItem } from './personnelTypes'

type PersonnelFormDialogProps = {
  open: boolean
  mode: 'create' | 'edit'
  personnel?: PersonnelListItem | null
  isSubmitting: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: PersonnelFormValues) => void
}

const defaultValues: PersonnelFormValues = {
  registrationNumber: '',
  firstName: '',
  lastName: '',
  department: '',
  title: '',
  phone: '',
  email: '',
  address: '',
  latitude: null,
  longitude: null,
}

export function PersonnelFormDialog({
  open,
  mode,
  personnel,
  isSubmitting,
  errorMessage,
  onClose,
  onSubmit,
}: PersonnelFormDialogProps) {
  const form = useForm<PersonnelFormSchema>({
    resolver: zodResolver(personnelFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!open) {
      return
    }

    if (mode === 'edit' && personnel) {
      const nameParts = personnel.fullName.split(' ')
      form.reset({
        registrationNumber: personnel.registrationNumber,
        firstName: nameParts[0] ?? '',
        lastName: nameParts.slice(1).join(' '),
        department: personnel.department ?? '',
        title: personnel.title ?? '',
        phone: personnel.phone ?? '',
        email: personnel.email ?? '',
        address: '',
        latitude: null,
        longitude: null,
      })
      return
    }

    form.reset(defaultValues)
  }, [form, mode, open, personnel])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Personel Ekle' : 'Personel Düzenle'}</DialogTitle>
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
          id="personnel-form"
          onSubmit={form.handleSubmit((values) => onSubmit(values))}
          sx={{ pt: 1 }}
        >
          <Grid size={{ xs: 12, md: 4 }}>
            <Controller
              name="registrationNumber"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Sicil Numarası"
                  size="small"
                  fullWidth
                  disabled={mode === 'edit'}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
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
          <Grid size={{ xs: 12, md: 4 }}>
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
              name="department"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Departman"
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
              name="title"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Unvan"
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
              name="email"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="E-posta"
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
              name="address"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Adres"
                  size="small"
                  fullWidth
                  multiline
                  minRows={2}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="latitude"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  value={field.value ?? ''}
                  onChange={(event) =>
                    field.onChange(event.target.value === '' ? null : Number(event.target.value))
                  }
                  label="Latitude"
                  type="number"
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
              name="longitude"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  value={field.value ?? ''}
                  onChange={(event) =>
                    field.onChange(event.target.value === '' ? null : Number(event.target.value))
                  }
                  label="Longitude"
                  type="number"
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
        <Button onClick={onClose}>Vazgeç</Button>
        <Button type="submit" form="personnel-form" variant="contained" disabled={isSubmitting}>
          Kaydet
        </Button>
      </DialogActions>
    </Dialog>
  )
}
