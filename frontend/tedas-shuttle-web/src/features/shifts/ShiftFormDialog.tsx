import { zodResolver } from '@hookform/resolvers/zod'
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from '@mui/material'
import { useEffect } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { shiftFormSchema, type ShiftFormSchema } from './shiftSchema'
import { shiftTypes, type ShiftFormValues, type ShiftListItem } from './shiftTypes'

type ShiftFormDialogProps = {
  open: boolean
  mode: 'create' | 'edit'
  shift?: ShiftListItem | null
  isSubmitting: boolean
  errorMessage?: string | null
  onClose: () => void
  onSubmit: (values: ShiftFormValues) => void
}

const defaultValues: ShiftFormValues = {
  name: '',
  shiftType: 1,
  capacity: 1,
  startTime: '08:00',
  endTime: '17:00',
}

export function ShiftFormDialog({
  open,
  mode,
  shift,
  isSubmitting,
  errorMessage,
  onClose,
  onSubmit,
}: ShiftFormDialogProps) {
  const form = useForm<ShiftFormSchema>({
    resolver: zodResolver(shiftFormSchema),
    defaultValues,
  })

  useEffect(() => {
    if (!open) {
      return
    }

    if (mode === 'edit' && shift) {
      form.reset({
        name: shift.name,
        shiftType: shift.shiftType,
        capacity: shift.capacity,
        startTime: toTimeInputValue(shift.startTime),
        endTime: toTimeInputValue(shift.endTime),
      })
      return
    }

    form.reset(defaultValues)
  }, [form, mode, open, shift])

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{mode === 'create' ? 'Vardiya Ekle' : 'Vardiya Duzenle'}</DialogTitle>
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
          id="shift-form"
          onSubmit={form.handleSubmit((values) => onSubmit(values))}
          sx={{ pt: 1 }}
        >
          <Grid size={{ xs: 12, md: 6 }}>
            <Controller
              name="name"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Vardiya Adi"
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
              name="shiftType"
              control={form.control}
              render={({ field, fieldState }) => (
                <FormControl size="small" fullWidth error={!!fieldState.error}>
                  <InputLabel id="shift-type-label">Tur</InputLabel>
                  <Select
                    {...field}
                    labelId="shift-type-label"
                    label="Tur"
                    value={field.value}
                    onChange={(event) => field.onChange(Number(event.target.value))}
                  >
                    {shiftTypes.map((type) => (
                      <MenuItem key={type.value} value={type.value}>
                        {type.label}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Controller
              name="capacity"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Kapasite"
                  type="number"
                  size="small"
                  fullWidth
                  value={field.value}
                  onChange={(event) => field.onChange(Number(event.target.value))}
                  slotProps={{ htmlInput: { min: 1 } }}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Controller
              name="startTime"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Baslangic"
                  type="time"
                  size="small"
                  fullWidth
                  slotProps={{ inputLabel: { shrink: true } }}
                  error={!!fieldState.error}
                  helperText={fieldState.error?.message}
                />
              )}
            />
          </Grid>
          <Grid size={{ xs: 12, md: 4 }}>
            <Controller
              name="endTime"
              control={form.control}
              render={({ field, fieldState }) => (
                <TextField
                  {...field}
                  label="Bitis"
                  type="time"
                  size="small"
                  fullWidth
                  slotProps={{ inputLabel: { shrink: true } }}
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
        <Button type="submit" form="shift-form" variant="contained" disabled={isSubmitting}>
          Kaydet
        </Button>
      </DialogActions>
    </Dialog>
  )
}

function toTimeInputValue(value: string) {
  return value.slice(0, 5)
}
